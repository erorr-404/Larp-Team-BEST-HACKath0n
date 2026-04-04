// TODO: незабути про компенсацію гравітації в IMU

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Клас для конвертації gps, imu та baro записів у масив kinematicPoints, який містить координати, швидкість та прискорення відносно стартової точки.
/// </summary>
public class KinematicCalculator
{
    public KinematicPoint[] kinematicPoints = Array.Empty<KinematicPoint>();

    // це масив із координатами відносно стартової точки
    // створений для того, щоб один раз конвертувати всі записи gps у вектори
    // а оскільки gps оновлюється рідше, ніж imu будем брати середнє значення
    // між двома точками для більної плавності


    private PositionRecord[] _positionRecords = Array.Empty<PositionRecord>();

    /// <summary>
    /// Обчислює кінематичні точки з заданих записів.
    /// </summary>
    /// <param name="gpsRecords"> Масив записів GPS </param>
    /// <param name="imuRecords"> Масив записів IMU </param>
    /// <param name="baroRecords"> Масив записів BARO </param>
    public void CalculateKinematicPointsFromRecords(GpsRecord[] gpsRecords, ImuRecord[] imuRecords, BaroRecord[] baroRecords)
    {

        // використовуємо дані з IMU, оскільки вони мають майбільшу частоту запису
        double startTime = imuRecords[0].time; // обираємо початкове значення кроку (момент початку запису)
        double endTime = imuRecords[imuRecords.Length - 1].time; // обираємо кінцеве значення часу (момент завершення запису)
        double timeStep = (imuRecords[imuRecords.Length - 1].time - imuRecords[0].time) / (imuRecords.Length - 1); // обираємо крок ітерування
        double timeStepSeconds = timeStep / 1_000_000;

        int capacity = (int)((endTime - startTime) / timeStep);

        System.Console.WriteLine(capacity);
        List<KinematicPoint> kinematicPointsList = new(capacity);
        // обчислюємо масив із координатами відносно стартової точки
        CalculatePositionRecords(gpsRecords);
        
        Vector3d velocityPrev = new(0, 0, 0);
        Vector3d accPrev = new(0, 0, 0);
        Vector3d angularVelocityPrev = new(0, 0, 0);
        

        // Шо тут робиться:
        // ми ітеруємось через часові мітки, із imu записів
        // оскільки частота отримання даних gps набагато рідша за imu
        // то ми значення, яке найімовірніше було в той момент
        // за допомогою лінійної інтерполяції: https://uk.wikipedia.org/wiki/%D0%9B%D1%96%D0%BD%D1%96%D0%B9%D0%BD%D0%B0_%D1%96%D0%BD%D1%82%D0%B5%D1%80%D0%BF%D0%BE%D0%BB%D1%8F%D1%86%D1%96%D1%8F
        // дані з imu беремо найближчі до даної позначки часу із масива imuRecords
        // найімовірніше це будуть прямо точні дані, бо початок, кінець та крок часових позначок
        // ми беремо саме з IMU
        for (double time = startTime; time <= endTime; time += timeStep)
        {
            PositionRecord currentPosition = GetClosestPositionRecord(time); // отримуємо найближче значення
            ImuRecord currentImu = GetClosestImuRecord(imuRecords, time); // отримуємо найближчий запис IMU

            // FIXME: тут потрібно врахувати дані з барометра, щоб коректно обчислювати висоту та вертикальну швидкість
            // FIXME: також потрібно врахувати гравітацію, щоб коректно обчислювати прискорення

            Vector3d accI = new Vector3d(currentImu.accX, currentImu.accY, currentImu.accZ);
            Vector3d velocityI = velocityPrev + ((accI + accPrev) / 2) * (timeStepSeconds);

            Vector3d angularVelocity = new Vector3d(currentImu.gyrX, currentImu.gyrY, currentImu.gyrZ);
            Vector3d angularPosition = angularVelocityPrev + ((angularVelocity + angularVelocityPrev) / 2) * (timeStepSeconds);

            KinematicPoint currentKinematicPoint = new();
            currentKinematicPoint.Timestamp = time;
            currentKinematicPoint.Longitude = currentPosition.Longitude;
            currentKinematicPoint.Latitude = currentPosition.Latitude;
            currentKinematicPoint.Altitude = currentPosition.Altitude;
            currentKinematicPoint.Position = currentPosition.Position;
            currentKinematicPoint.Speed = velocityI.ToVector3();
            currentKinematicPoint.Acceleration = accI.ToVector3();
            currentKinematicPoint.Rotation = new Quaternion(angularPosition.ToVector3(), 0);
            currentKinematicPoint.angularSpeed = angularVelocity.ToVector3();

            kinematicPointsList.Add(currentKinematicPoint);

            // замінюємо попереднє значення на поточне для наступної ітерації
            accPrev = accI;
            velocityPrev = velocityI;
            angularVelocityPrev = angularVelocity;
        }

        // після обчислення всіх кінематичних точок скидаємо курсори
        _imuRecordsCursor = 0;
        _positionCursor = 0;
        kinematicPoints = kinematicPointsList.ToArray();
    }

    private int _imuRecordsCursor = 0; // курсор для швидкого послідовного доступу

    /// <summary> Ця функція повертає найближчий запис IMU до заданого часу. </summary>
    /// <param name="imuRecords"> Масив записів IMU </param>
    /// <param name="time"> Час, для якого потрібно знайти найближчий запис IMU </param>
    /// <returns> Запис IMU, який найближче до заданого часу </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImuRecord GetClosestImuRecord(ImuRecord[] imuRecords, double time) 
    // TODO: перевірити чи [MethodImpl(MethodImplOptions.AggressiveInlining)] дійсно це прискорює виконання функції
    {
        int len = imuRecords.Length;

        // базова перевірка на дурачка
        if (len == 0) return default;
        if (len == 1) return imuRecords[0];

        int last = len - 1;

        if (time <= imuRecords[0].time)
        {
            _imuRecordsCursor = 0;
            return imuRecords[0];
        }

        if (time >= imuRecords[last].time)
        {
            _imuRecordsCursor = last;
            return imuRecords[last];
        }

        int i = _imuRecordsCursor;
        if ((uint)i >= (uint)last) i = 0;

        if (time >= imuRecords[i].time)
        {
            while (i + 1 < len && imuRecords[i + 1].time <= time)
            i++;
        }
        else
        {
            int lo = 0;
            int hi = last;

            while (lo + 1 < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (imuRecords[mid].time <= time) lo = mid;
                else hi = mid;
            }

            i = lo;
        }

        int j = i + 1;
        double dtLeft = time - imuRecords[i].time;
        double dtRight = imuRecords[j].time - time;

        int nearest = (dtRight < dtLeft) ? j : i;
        _imuRecordsCursor = nearest;

        return imuRecords[nearest];
    }

    private int _positionCursor; // курсор для швидкого послідовного доступу
    
    /// <summary>
    /// Обчислює найближчий за даним часом до реальності запис про позицію за допомогою лінійної інтерполяції.
    /// </summary>
    /// <param name="time"> Час, для якого потрібно знайти позицію </param>
    /// <returns> Позиція у вигляді PositionRecord, яка відповідає заданому часу </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PositionRecord GetClosestPositionRecord(double time)
    // TODO: перевірити чи [MethodImpl(MethodImplOptions.AggressiveInlining)] дійсно це прискорює виконання функції
    {
        var records = _positionRecords;
        int len = records.Length;

        if (len == 0)
            return default; // Або throw, якщо хочете strict-поведінку.
        if (len == 1)
            return records[0];

        int last = len - 1;

        // Швидкі граничні випадки.
        if (time <= records[0].Timestamp)
        {
            _positionCursor = 0;
            return records[0];
        }

        if (time >= records[last].Timestamp)
        {
            _positionCursor = last - 1;
            return records[last];
        }

        int i = _positionCursor;
        if ((uint)i >= (uint)last)
            i = 0;

        // Основний fast-path: час зростає, рухаємо курсор тільки вперед.
        if (time >= records[i].Timestamp)
        {
            while (i + 1 < len && records[i + 1].Timestamp < time)
                i++;
        }
        else
        {
            // Fallback: якщо time пішов назад, знайдемо інтервал бінарним пошуком.
            int lo = 0;
            int hi = last;
            while (lo + 1 < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (records[mid].Timestamp <= time)
                    lo = mid;
                else
                    hi = mid;
            }
            i = lo;
        }

        _positionCursor = i;

        ref readonly var a = ref records[i];
        ref readonly var b = ref records[i + 1];

        double dt = b.Timestamp - a.Timestamp;
        if (dt <= 1e-12)
            return a; // Захист від дубльованих timestamp.

        float t = (float)((time - a.Timestamp) / dt);

        if (t <= 0f) return a;
        if (t >= 1f) return b;

        Vector3 interpolated = Vector3.Lerp(a.Position, b.Position, t);
        double interpolatedLongitude = a.Longitude + (b.Longitude - a.Longitude) * t;
        double interpolatedLatitude = a.Latitude + (b.Latitude - a.Latitude) * t;
        float interpolatedAltitude = a.Altitude + (b.Altitude - a.Altitude) * t;

        return new PositionRecord(time, interpolated, interpolatedLongitude, interpolatedLatitude, interpolatedAltitude);
    }


    /// <summary>
    /// Обчислює масив із координатами відносно стартової точки на основі gps записів.
    /// </summary>
    /// <param name="gpsRecords"> Масив записів GPS </param>
    private void CalculatePositionRecords(GpsRecord[] gpsRecords)
    {
        var positionRecordsList = new List<PositionRecord>();

        double startLat = gpsRecords[0].lat;
        double startLng = gpsRecords[0].lng;
        double startAlt = gpsRecords[0].alt;

        Vector3d startEcef = Utils.LlaToEcef(startLat, startLng, startAlt);
        
        double currentLat, currentLng;
        float currentAlt;
        Vector3d currentEcef;
        Vector3 localPos;

        foreach (GpsRecord record in gpsRecords)
        {
            currentLat = record.lat;
            currentLng = record.lng;
            currentAlt = record.alt; // FIXME: брати висоту не від gps, а від барометра

            currentEcef = Utils.LlaToEcef(currentLat, currentLng, (double)currentAlt);
            localPos = Utils.EcefToEnu(currentEcef, startEcef, startLat, startLng);

            positionRecordsList.Add(new PositionRecord(record.time, localPos, currentLng, currentLat, currentAlt));
        }

        _positionRecords = positionRecordsList.ToArray();
    }


    /// <summary>
    /// Структура для зберігання запису про позицію з часом та координатами.
    /// </summary>
    /// <param name="timestamp"> Час запису </param>
    /// <param name="pos"> Координати позиції </param>
    /// <param name="lng"> Довгота </param>
    /// <param name="pos"> Широта </param>
    private readonly struct PositionRecord(double timestamp, Vector3 pos, double lng, double lat, float alt)
    {
        public readonly double Timestamp = timestamp;
        public readonly Vector3 Position = pos;

        public readonly double Longitude = lng;
        public readonly double Latitude = lat;
        public readonly float Altitude = alt;
    }
}
