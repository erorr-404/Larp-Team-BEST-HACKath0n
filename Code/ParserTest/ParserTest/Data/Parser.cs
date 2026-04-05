
using MissionPlanner.Utilities;
using static MissionPlanner.Utilities.DFLog;
using System.Collections.Generic;
using System;


/// <summary>
/// Клас для парсингу бінарного файлу та збереження результату у 3 масивах: GpsRecords, ImuRecords та BaroRecords.
/// Цей клас є посередником між сирими даними та структурованими даними, які можна використовувати для аналізу.
/// Він відповідає за зчитування даних з бінарного файлу, конвертацію їх у відповідні структури та збереження результату у відповідних масивах.
/// </summary>
/// <param name="fileName"> Шилях до бінарного файлу </param>
public class BinaryParser
{

    public BinaryParser(string fileName)
    {
        _fileName = fileName;
    }

    private readonly string _fileName;

    public GpsRecord[] GpsRecords { get; private set; } = System.Array.Empty<GpsRecord>();
    public ImuRecord[] ImuRecords { get; private set; } = System.Array.Empty<ImuRecord>();
    public BaroRecord[] BaroRecords { get; private set; } = System.Array.Empty<BaroRecord>();


    /// <summary>
    /// Парсить бінарний файл та зберігає результат у 3 масивах: GpsRecords, ImuRecords та BaroRecords.
    /// </summary>
    /// <returns> Повертає true, якщо парсинг успішний, інакше false. </returns>
    public bool Parse()
    {
        // парсимо бінарник
        using DFLogBuffer logdata = new DFLogBuffer(_fileName);

        // зберігаємо проміжний результат
        var gps = new List<GpsRecord>();
        var imu = new List<ImuRecord>();
        
        var baro = new List<BaroRecord>();

        int baroIndex = 1;

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

                // дані baro відправляємо до списків baro
                case "BARO":
                    BaroRecord? baroRecord = TryMapBaro(item);
                    if (baroRecord.HasValue)
                    {
                        baro.Add(baroRecord.Value);
                    }
                    baroIndex++;
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


    /// <summary>
    /// Ця функція пробує перетворити рядки на gps структуру. Якщо конвертація не вдається, повертає null.
    /// </summary>
    /// <param name="item"> Елемент даних для конвертації </param>
    /// <returns> Об'єкт GpsRecord, якщо конвертація успішна, інакше null </returns>
    private static GpsRecord? TryMapGps(DFItem item)
    {
        if (!double.TryParse(item["Lat"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat))
            return null;

        if (!double.TryParse(item["Lng"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng))
            return null;

        if (!float.TryParse(item["Alt"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var alt))
            return null;

        if (!float.TryParse(item["Spd"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var spd))
            return null;

        if (!int.TryParse(item["NSats"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var nstats))
            return null;

        return new GpsRecord(
            item.timems, // час мав би бути double, а не float
            lat,
            lng,
            alt,
            spd,
            nstats);
    }


    /// <summary>
    /// Ця функція пробує перетворити рядки на imu структуру. Якщо конвертація не вдається, повертає null.
    /// </summary>
    /// <param name="item"> Елемент даних для конвертації </param>
    /// <returns> Об'єкт ImuRecord, якщо конвертація успішна, інакше null </returns>
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
            item.timems, // якого біса тут був float??
            gyrX,
            gyrY,
            gyrZ,
            accX,
            accY,
            accZ);
    }
    

    /// <summary>
    /// Ця функція пробує перетворити рядки на baro структуру. Якщо конвертація не вдається, повертає null.
    /// </summary>
    /// <param name="item"> Елемент даних для конвертації </param>
    /// <returns> Об'єкт BaroRecord, якщо конвертація успішна, інакше null </returns>
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
        
        if (!int.TryParse(item["I"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var baroIndex))
            return null;

        return new BaroRecord(
            item.timems, // якого дідька тут теж???
            alt,
            temp,
            press,
            crt,
            baroIndex); // номер барометра, з якого були взяті дані
    }

    public void Dispose()
    {
        // очищуємо масиви з результатами, щоб звільнити пам'ять
        GpsRecords = Array.Empty<GpsRecord>();
        ImuRecords = Array.Empty<ImuRecord>();
        BaroRecords = Array.Empty<BaroRecord>();
    }
}