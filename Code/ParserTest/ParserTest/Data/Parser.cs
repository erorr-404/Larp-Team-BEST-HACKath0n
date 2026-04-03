
using MissionPlanner.Utilities;
using static MissionPlanner.Utilities.DFLog;

public class BinariesParser(string fileName)
{
    private readonly string _fileName = fileName;

    public GpsRecord[] GpsRecords { get; private set; } = System.Array.Empty<GpsRecord>();
    public ImuRecord[] ImuRecords { get; private set; } = System.Array.Empty<ImuRecord>();
    public BaroRecord[] BaroRecords { get; private set; } = System.Array.Empty<BaroRecord>();

    // завантажуємо дані із бінарника у 3 масиви
    public bool Parse()
    {
        // парсимо бінарник
        using DFLogBuffer logdata = new(_fileName);

        // зберігаємо проміжний результат
        var gps = new List<GpsRecord>();
        var imu = new List<ImuRecord>();
        var baro = new List<BaroRecord>();

        // ітеруємся через сирі дані
        foreach (DFItem item in logdata.GetEnumeratorTypeAll())
        {
            switch (item.msgtype)
            {
                // дані gps відправляємо до списку gps
                case "GPS":
                    GpsRecord? record = TryMapGps(item);
                    if (record.HasValue)
                    {
                        gps.Add(record.Value);
                    }
                    break;
                
                // дані imu відправляємо до списку imu
                case "IMU":
                    ImuRecord? imuRecord = TryMapImu(item);
                    if (imuRecord.HasValue)
                    {
                        imu.Add(imuRecord.Value);
                    }
                    break;

                // дані baro відправляємо до списку baro
                case "BARO":
                    BaroRecord? baroRecord = TryMapBaro(item);
                    if (baroRecord.HasValue)
                    {
                        baro.Add(baroRecord.Value);
                    }
                    break;
                
                // всі інші дані відкидаємо
                default:
                    continue;
            }
        }

        // зберігаєо результат
        GpsRecords = gps.ToArray();
        ImuRecords = imu.ToArray();
        BaroRecords = baro.ToArray();

        // повертаємо тру
        return true;
    }

    // ця функція пробує перетворити рядки на gps структуру
    private static GpsRecord? TryMapGps(DFItem item)
    {
        if (!float.TryParse(item["Lat"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat))
            return null;

        if (!float.TryParse(item["Lng"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng))
            return null;

        if (!float.TryParse(item["Alt"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var alt))
            return null;

        if (!float.TryParse(item["Spd"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var spd))
            return null;

        if (!int.TryParse(item["NSats"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var nstats))
            return null;

        return new GpsRecord(
            (float)item.timems,
            lat,
            lng,
            alt,
            spd,
            nstats);
    }

    // ця функція пробує перетворити рядки на imu структуру
    private static ImuRecord? TryMapImu(DFItem item) 
    {
        if (!float.TryParse(item["GyrX"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gyrX))
            return null;

        if (!float.TryParse(item["GyrY"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gyrY))
            return null;

        if (!float.TryParse(item["GyrZ"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gyrZ))
            return null;

        if (!float.TryParse(item["AccX"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var accX))
            return null;

        if (!float.TryParse(item["AccY"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var accY))
            return null;

        if (!float.TryParse(item["AccZ"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var accZ))
            return null;

        return new ImuRecord(
            (float)item.timems,
            gyrX,
            gyrY,
            gyrZ,
            accX,
            accY,
            accZ);
    }

    // ця функція пробує перетворити рядки на baro структуру
    private static BaroRecord? TryMapBaro(DFItem item)
    {
        if (!float.TryParse(item["Alt"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var alt))
            return null;

        if (!float.TryParse(item["Temp"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var temp))
            return null;

        if (!float.TryParse(item["Press"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var press))
            return null;

        if (!float.TryParse(item["CRt"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var crt))
            return null;

        return new BaroRecord(
            (float)item.timems,
            alt,
            temp,
            press,
            crt);
    }
}