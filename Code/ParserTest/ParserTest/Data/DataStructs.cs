using System.Collections.Generic;
using System;

/// <summary>
/// Ця структура використовуєься для зберігання запису про GPS-дані, включаючи час, широту, довготу, висоту, швидкість та кількість супутників.
/// </summary>
public readonly struct GpsRecord
{
    public readonly double time;
    public readonly double lat;
    public readonly double lng;
    public readonly float alt;
    public readonly float spd;
    public readonly int nstats;

    public GpsRecord(double time, double lat, double lng, float alt, float spd, int nstats)
    {
        this.time = time;
        this.lat = lat;
        this.lng = lng;
        this.alt = alt;
        this.spd = spd;
        this.nstats = nstats;
    }

    public override string ToString()
    {
        return $"Time: {time}, Lat: {lat}, Lng: {lng}, Alt: {alt}, Spd: {spd}, NSats: {nstats}";
    }
}


/// <summary>
/// Ця структура використовується для зберігання запису про IMU-дані, включаючи час, швидкість обертання навколо осей X, Y та Z, а також лінійне прискорення вздовж осей X, Y та Z.
/// </summary>
public readonly struct ImuRecord
{
    public readonly double time;

    // швидкість обертання навколо осей X, Y та Z, rad/s
    public readonly float gyrX;
    public readonly float gyrY;
    public readonly float gyrZ;

    // лінійне прискорення вздовж осей X, Y та Z, m/s/s
    public readonly float accX;
    public readonly float accY;
    public readonly float accZ;

    public ImuRecord(double time, float gyrX, float gyrY, float gyrZ, float accX, float accY, float accZ)
    {
        this.time = time;
        this.gyrX = gyrX;
        this.gyrY = gyrY;
        this.gyrZ = gyrZ;
        this.accX = accX;
        this.accY = accY;
        this.accZ = accZ;
    }

    /// <summary>
    /// Обчислює величину лінійного прискорення на основі компонентів accX, accY та accZ за формулою: sqrt(accX^2 + accY^2 + accZ^2).
    /// </summary>
    /// <returns> Величина лінійного прискорення, m/s² </returns>
    public float GetLinearAccelerationMagnitude => MathF.Sqrt(accX * accX + accY * accY + accZ * accZ);

    public override string ToString()
    {
        return $"Time: {time}, GyrX: {gyrX}, GyrY: {gyrY}, GyrZ: {gyrZ}, AccX: {accX}, AccY: {accY}, AccZ: {accZ}";
    }
}


/// <summary>
/// Ця структура використовується для зберігання запису про BARO-дані, включаючи час, висоту, температуру, тиск та швидкість набору висоти.
/// </summary>
public readonly struct BaroRecord
{
    public readonly double time;
    public readonly float alt;
    public readonly float temp;
    public readonly float press; // атмосферний тиск, hPa
    public readonly float crt; // climb rate, швидкість набору висоти, m/s

    public BaroRecord(double time, float alt, float temp, float press, float crt)
    {
        this.time = time;
        this.alt = alt;
        this.temp = temp;
        this.press = press;
        this.crt = crt;
    }

    public override string ToString()
    {
        return $"Time: {time}, Alt: {alt}, Temp: {temp}, Press: {press}, CRt: {crt}";
    }
}

