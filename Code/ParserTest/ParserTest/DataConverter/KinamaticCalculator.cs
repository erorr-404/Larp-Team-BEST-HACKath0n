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
        double timeStep = imuRecords[1].time - imuRecords[0].time; // обираємо крок ітерування

        // обчислюємо масив із координатами відносно стартової точки
        CalculatePositionRecords(gpsRecords);

        PositionRecord currentPosition;

        for (double time = startTime; time <= endTime; time += timeStep)
        {
            currentPosition = GetClosestPositionRecord(time); // отримуємо найближче значення
            // TODO: продовжити тут
        }
    }

    private int _positionCursor; // курсор для швидкого послідовного доступу
    
    /// <summary>
    /// Знаходить найближчий за даним часом до реальності запис про позицію за допомогою лінійної інтерполяції.
    /// </summary>
    /// <param name="time"> Час, для якого потрібно знайти позицію </param>
    /// <returns> Позиція у вигляді PositionRecord, яка відповідає заданому часу </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PositionRecord GetClosestPositionRecord(double time)
    // TODO: перевірити чи дійсно це прискорює виконання функції
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
        return new PositionRecord(time, interpolated);
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
        
        double currentLat, currentLng, currentAlt;
        Vector3d currentEcef;
        Vector3 localPos;

        foreach (GpsRecord record in gpsRecords)
        {
            currentLat = record.lat;
            currentLng = record.lng;
            currentAlt = record.alt; // FIXME: брати висоту не від gps, а від барометра

            currentEcef = Utils.LlaToEcef(currentLat, currentLng, currentAlt);
            localPos = Utils.EcefToEnu(currentEcef, startEcef, startLat, startLng);

            positionRecordsList.Add(new PositionRecord(record.time, localPos));
        }

        _positionRecords = positionRecordsList.ToArray();
    }


    /// <summary>
    /// Структура для зберігання запису про позицію з часом та координатами.
    /// </summary>
    /// <param name="timestamp"> Час запису </param>
    /// <param name="pos"> Координати позиції </param>
    private readonly struct PositionRecord(double timestamp, Vector3 pos)
    {
        public readonly double Timestamp = timestamp;
        public readonly Vector3 Position = pos;
    }
}