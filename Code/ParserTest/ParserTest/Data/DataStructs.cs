

/// <summary>
/// Ця структура використовуєься для зберігання запису про GPS-дані, включаючи час, широту, довготу, висоту, швидкість та кількість супутників.
/// </summary>
/// <param name="time"> Час у мікросекундах відносно початку роботи польотника </param>
/// <param name="lat"> Широта, градуси </param>
/// <param name="lng"> Довгота, градуси </param>
/// <param name="alt"> Висота, метри </param>
/// <param name="spd"> Швидкість, м/с </param>
/// <param name="nstats"> Кількість супутників </param>
public readonly record struct GpsRecord(double time, double lat, double lng, float alt, float spd, int nstats)
{
    readonly double Time = time;
    readonly double Latitude = lat;
    readonly double Longitude = lng;
    readonly float Altitude = alt;
    readonly float Speed = spd;
    readonly int NStats = nstats;

    public override string ToString()
    {
        return $"Time: {Time}, Lat: {Latitude}, Lng: {Longitude}, Alt: {Altitude}, Spd: {Speed}, NSats: {NStats}";
    }
}


/// <summary>
/// Ця структура використовується для зберігання запису про IMU-дані, включаючи час, швидкість обертання навколо осей X, Y та Z, а також лінійне прискорення вздовж осей X, Y та Z.
/// </summary>
/// <param name="time"> Час у мікросекундах відносно початку роботи польотника </param>
/// <param name="gyrX"> Швидкість обертання навколо осі X, rad/s </param>
/// <param name="gyrY"> Швидкість обертання навколо осі Y, rad/s </param>
/// <param name="gyrZ"> Швидкість обертання навколо осі Z, rad/s </param>
/// <param name="accX"> Лінійне прискорення вздовж осі X, m/s/s </param>
/// <param name="accY"> Лінійне прискорення вздовж осі Y, m/s/s </param>
/// <param name="accZ"> Лінійне прискорення вздовж осі Z, m/s/s </param>
public readonly record struct ImuRecord(double time, float gyrX, float gyrY, float gyrZ, float accX, float accY, float accZ)
{
    readonly double Time = time;

    // швидкість обертання навколо осей X, Y та Z, rad/s
    readonly float GyrX = gyrX;
    readonly float GyrY = gyrY;
    readonly float GyrZ = gyrZ;

    // лінійне прискорення вздовж осей X, Y та Z, m/s/s
    readonly float AccX = accX;
    readonly float AccY = accY;
    readonly float AccZ = accZ;

    public override string ToString()
    {
        return $"Time: {Time}, GyrX: {GyrX}, GyrY: {GyrY}, GyrZ: {GyrZ}, AccX: {AccX}, AccY: {AccY}, AccZ: {AccZ}";
    }
}


/// <summary>
/// Ця структура використовується для зберігання запису про BARO-дані, включаючи час, висоту, температуру, тиск та швидкість набору висоти.
/// </summary>
/// <param name="time"> Час у мікросекундах відносно початку роботи польотника </param>
/// <param name="alt"> Висота, метри </param>
/// <param name="temp"> Температура, градуси Цельсія </param>
/// <param name="press"> Атмосферний тиск, hPa </param>
/// <param name="crt"> Швидкість набору висоти, m/s </param>
public readonly record struct BaroRecord(double time, float alt, float temp, float press, float crt)
{
    readonly double Time = time;
    readonly float Alt = alt;
    readonly float Temp = temp;
    readonly float Press = press; // атмосферний тиск, hPa
    readonly float CRt = crt; // climb rate, швидкість набору висоти, m/s

    public override string ToString()
    {
        return $"Time: {Time}, Alt: {Alt}, Temp: {Temp}, Press: {Press}, CRt: {CRt}";
    }
}

