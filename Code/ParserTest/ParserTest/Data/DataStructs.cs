public readonly record struct GpsRecord(double time, double lat, double lng, float alt, float spd, int nstats)
{
    // Це структурпа для збереження запису про GPS-дані
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

