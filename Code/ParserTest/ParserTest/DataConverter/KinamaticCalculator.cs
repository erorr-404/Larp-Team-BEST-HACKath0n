// TODO: незабути про компенсацію гравітації в IMU

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

/// <summary>
/// Клас для конвертації gps, imu та baro записів у масив kinematicPoints, який містить координати, швидкість та прискорення відносно стартової точки.
/// </summary>
public class KinematicCalculator
{
    const float Gravity = 9.80665f;

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
        double timeStepSeconds = timeStep / 1000.0;

        int capacity = (int)((endTime - startTime) / timeStep);

        // System.Console.WriteLine(capacity);
        List<KinematicPoint> kinematicPointsList = new List<KinematicPoint>(capacity);
        // обчислюємо масив із координатами відносно стартової точки
        CalculatePositionRecords(gpsRecords);
        
        // для інтегрування
        Vector3d velocityPrev = new Vector3d(0, 0, 0);
        Vector3d accPrev = new Vector3d(0, 0, 0);
        Vector3d angularVelocityPrev = new Vector3d(0, 0, 0);
        
        // барометр
        BaroRecord startBaro = GetClosestBaroRecord(baroRecords, startTime);
        float baroAlt0 = startBaro.alt;
        float absoluteAlt0 = GetClosestPositionRecord(startTime).Altitude;

        Quaternion attitude = Quaternion.Identity;

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
            
            Vector3 omegaBody = new Vector3(currentImu.gyrX, currentImu.gyrY, currentImu.gyrZ);
            attitude = IntegrateGyro(attitude, omegaBody, (float)timeStepSeconds);

            // 1) сире прискорення в body frame
            Vector3 accBody = new Vector3(currentImu.accX, currentImu.accY, currentImu.accZ);

            // 2) в ENU
            Vector3 accEnu = Vector3.Transform(accBody, attitude);

            // 3) компенсація гравітації (Up = +Z в ENU)
            Vector3 accLinearEnu = accEnu + new Vector3(0f, 0f, Gravity);

            // FIXME: також потрібно врахувати гравітацію, щоб коректно обчислювати прискорення

            // Якщо побачим, що на стоянці Z виходить ~+19.6 або ~-19.6, зміни знак на мінус:
            // Vector3 accLinearEnu = accEnu - new Vector3(0f, 0f, Gravity);

            Vector3d accI = new Vector3d(accLinearEnu.X, accLinearEnu.Y, accLinearEnu.Z);
            Vector3d velocityI = velocityPrev + ((accI + accPrev) / 2.0) * timeStepSeconds;

            Vector3d angularVelocity = new Vector3d(currentImu.gyrX, currentImu.gyrY, currentImu.gyrZ);
            Vector3d angularPosition = angularVelocityPrev + ((angularVelocity + angularVelocityPrev) / 2) * (timeStepSeconds);

            BaroRecord currentBaro = GetClosestBaroRecord(baroRecords, time);
            float deltaAlt = currentBaro.alt - baroAlt0;
            float altitude = absoluteAlt0 + deltaAlt;

            Vector3 pos = currentPosition.Position;
            pos.Z = deltaAlt;

            Vector3 vel = velocityI.ToVector3();
            vel.Z = currentBaro.crt;
            
            KinematicPoint currentKinematicPoint = new KinematicPoint();
            currentKinematicPoint.Timestamp = time - startTime;
            currentKinematicPoint.Longitude = currentPosition.Longitude;
            currentKinematicPoint.Latitude = currentPosition.Latitude;
            currentKinematicPoint.Altitude = altitude;
            currentKinematicPoint.Position = pos;
            currentKinematicPoint.Speed = vel;
            currentKinematicPoint.Acceleration = accI.ToVector3();
            currentKinematicPoint.Rotation = new Quaternion(angularPosition.ToVector3(), 0);
            currentKinematicPoint.angularSpeed = angularVelocity.ToVector3();
            currentKinematicPoint.ClimbRate = currentBaro.crt;

            kinematicPointsList.Add(currentKinematicPoint);

            // замінюємо попереднє значення на поточне для наступної ітерації
            accPrev = accI;
            velocityPrev = velocityI;
            angularVelocityPrev = angularVelocity;
        }

        // після обчислення всіх кінематичних точок скидаємо курсори
        _imuRecordsCursor = 0;
        _positionCursor = 0;
        _baroRecordsCursor = 0;
        kinematicPoints = kinematicPointsList.ToArray();
    }

    private int _imuRecordsCursor = 0; // курсор для швидкого послідовного доступу

    /// <summary> Ця функція повертає найближчий запис IMU до заданого часу. </summary>
    /// <param name="imuRecords"> Масив записів IMU </param>
    /// <param name="time"> Час, для якого потрібно знайти найближчий запис IMU </param>
    /// <returns> Запис IMU, який найближче до заданого часу </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImuRecord GetClosestImuRecord(ImuRecord[] imuRecords, double time) 
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

    /// <summary>
    /// Інтегрування повороту.
    /// </summary>
    /// <param name="q"></param>
    /// <param name="omegaRadPerSec"></param>
    /// <param name="dtSec"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Quaternion IntegrateGyro(Quaternion q, Vector3 omegaRadPerSec, float dtSec)
    {
        Quaternion omegaQ = new Quaternion(omegaRadPerSec, 0f);
        Quaternion qDot = Quaternion.Multiply(q, omegaQ);

        q = new Quaternion(
            q.X + 0.5f * qDot.X * dtSec,
            q.Y + 0.5f * qDot.Y * dtSec,
            q.Z + 0.5f * qDot.Z * dtSec,
            q.W + 0.5f * qDot.W * dtSec
        );

        return Quaternion.Normalize(q);
    }

    private int _positionCursor; // курсор для швидкого послідовного доступу
    
    /// <summary>
    /// Обчислює найближчий за даним часом до реальності запис про позицію за допомогою лінійної інтерполяції.
    /// </summary>
    /// <param name="time"> Час, для якого потрібно знайти позицію </param>
    /// <returns> Позиція у вигляді PositionRecord, яка відповідає заданому часу </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PositionRecord GetClosestPositionRecord(double time)
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


    private int _baroRecordsCursor = 0; // курсор для швидкого послідовного доступу

    /// <summary>
    /// Вираховує найближчі до даної мітки часу дані барометра, використовуючи лінійну інтерполяцію.
    /// </summary>
    /// <param name="baroRecords"> Масив із записами з барометра. </param>
    /// <param name="time"> Мітка часу у мікросекунда від початку запису. </param>
    /// <returns> Новий запис BaroRecord з інтерпольованими даними. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BaroRecord GetClosestBaroRecord(BaroRecord[] baroRecords, double time)
    {
        int len = baroRecords.Length;

        if (len == 0) return default;
        if (len == 1) return baroRecords[0];

        int last = len - 1;

        // Граничні випадки
        if (time <= baroRecords[0].time)
        {
            _baroRecordsCursor = 0;
            return baroRecords[0];
        }

        if (time >= baroRecords[last].time)
        {
            _baroRecordsCursor = last - 1;
            return baroRecords[last];
        }

        int i = _baroRecordsCursor;
        if ((uint)i >= (uint)last)
            i = 0;

        // Fast-path: час зростає, рухаємося тільки вперед
        if (time >= baroRecords[i].time)
        {
            while (i + 1 < len && baroRecords[i + 1].time < time)
                i++;
        }
        else
        {
            // Fallback: якщо time пішов назад, шукаємо інтервал бінарним пошуком
            int lo = 0;
            int hi = last;

            while (lo + 1 < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (baroRecords[mid].time <= time)
                    lo = mid;
                else
                    hi = mid;
            }

            i = lo;
        }

        _baroRecordsCursor = i;

        ref readonly var a = ref baroRecords[i];
        ref readonly var b = ref baroRecords[i + 1];

        double dt = b.time - a.time;
        if (dt <= 1e-12)
            return a; // захист від дубльованих timestamp

        float t = (float)((time - a.time) / dt);

        if (t <= 0f) return a;
        if (t >= 1f) return b;

        float alt = a.alt + (b.alt - a.alt) * t;
        float temp = a.temp + (b.temp - a.temp) * t;
        float press = a.press + (b.press - a.press) * t;
        float crt = a.crt + (b.crt - a.crt) * t;

        // порядок параметрів: (time, alt, temp, press, crt)
        return new BaroRecord(time, alt, temp, press, crt, a.baroIndex);
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
            currentAlt = record.alt;

            currentEcef = Utils.LlaToEcef(currentLat, currentLng, (double)currentAlt);
            localPos = Utils.EcefToEnu(currentEcef, startEcef, startLat, startLng);

            positionRecordsList.Add(new PositionRecord(record.time, localPos, currentLng, currentLat, currentAlt));
        }

        _positionRecords = positionRecordsList.ToArray();
    }


    /// <summary>
    /// Структура для зберігання запису про позицію з часом та координатами. Бере дані виключно із GPS, тому перед передачою даних далі висоту треба замінити на дані із барометра.
    /// </summary>
    /// <param name="timestamp"> Час запису </param>
    /// <param name="pos"> Координати позиції </param>
    /// <param name="lng"> Довгота </param>
    /// <param name="pos"> Широта </param>
    private readonly struct PositionRecord
    {
        public readonly double Timestamp;
        public readonly Vector3 Position;

        public readonly double Longitude;
        public readonly double Latitude;
        public readonly float Altitude;

        public PositionRecord(double timestamp, Vector3 pos, double lng, double lat, float alt)
        {
            Timestamp = timestamp;
            Position = pos;
            Longitude = lng;
            Latitude = lat;
            Altitude = alt;
        }
    }
}
