using System;
using System.Numerics;


public struct Vector3d
{
    public double x;
    public double y;
    public double z;

    public Vector3d(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3d operator +(Vector3d v1, Vector3d v2)
    {
        return new Vector3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }

    public static Vector3d operator -(Vector3d v1, Vector3d v2)
    {
        return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    public static Vector3d operator *(Vector3d v, double scalar)
    {
        return new Vector3d(v.x * scalar, v.y * scalar, v.z * scalar);
    }

    public static Vector3d operator *(double scalar, Vector3d v)
    {
        return new Vector3d(v.x * scalar, v.y * scalar, v.z * scalar);
    }

    public static Vector3d operator /(Vector3d v, double scalar)
    {
        if (scalar == 0.0)
            throw new DivideByZeroException("Cannot divide Vector3d by zero.");

        return new Vector3d(v.x / scalar, v.y / scalar, v.z / scalar);
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}
public static class Utils
{   
    
    private const double WGS84_A = 6378137.0;              // Екваторіальний радіус
    private const double WGS84_E2 = 0.00669437999014;      // Ексцентриситет у квадраті

    public static Vector3d LlaToEcef(double lat, double lon, double alt)
{   // Переводимо градуси в радіани, бо стандартні функції Math.Sin/Cos працюють тільки з ними
        double radLat = lat * Math.PI / 180.0;
        double radLng = lon * Math.PI / 180.0;
    // Попередньо обчислюємо синуси та косинуси, щоб не робити це кілька разів (оптимізація)
        double sinLat = Math.Sin(radLat);
        double cosLat = Math.Cos(radLat);
        double sinLng = Math.Sin(radLng);
        double cosLng = Math.Cos(radLng);
    // Обчислюємо радіус кривизни вертикала (N).
    // Оскільки Земля — це еліпсоїд, відстань від центру до поверхні різна на різних широтах.
    // Формула враховує "сплюснутість" планети через ексцентриситет (WGS84_E2).
        double N = WGS84_A / Math.Sqrt(1.0 - WGS84_E2 * sinLat * sinLat);

        double x = (N + alt) * cosLat * cosLng;
        double y = (N + alt) * cosLat * sinLng;
        double z = (N * (1.0 - WGS84_E2) + alt) * sinLat;

        return new Vector3d(x, y, z);
}
    public static Vector3 EcefToEnu(Vector3d currentEcef, Vector3d startEcef, double startLat, double startLng)
{
        double radLat = startLat * Math.PI / 180.0;
        double radLng = startLng * Math.PI / 180.0;

        double sinLat = Math.Sin(radLat);
        double cosLat = Math.Cos(radLat);
        double sinLng = Math.Sin(radLng);
        double cosLng = Math.Cos(radLng);

        // Різниця між поточною точкою та стартом
        double dx = currentEcef.x - startEcef.x;
        double dy = currentEcef.y - startEcef.y;
        double dz = currentEcef.z - startEcef.z;

        // Матричне множення (ENU перетворення)
        float e = (float)(-sinLng * dx + cosLng * dy);
        float n = (float)(-sinLat * cosLng * dx - sinLat * sinLng * dy + cosLat * dz);
        float u = (float)(cosLat * cosLng * dx + cosLat * sinLng * dy + sinLat * dz);

        return new Vector3(e, n, u); // Повертає (East, North, Up)
}
    public static void Example()
    {   
        // double startLat, startLng, startAlt, currentLat, currentLng, currentAltFromBaro = (5d, 5d, 5d, 5d, 5d, 5d);
        double startLat = -35.363266d;
        double startLng = 149.16524;
        double startAlt = 5d;
        double currentLat = 5d;
        double currentLng = 5d;
        double currentAltFromBaro = 5d;

        Vector3d startEcef = LlaToEcef(startLat, startLng, startAlt);
        Vector3d currentEcef = LlaToEcef(currentLat, currentLng, currentAltFromBaro);
        Vector3 localPos = EcefToEnu(currentEcef, startEcef, startLat, startLng);
        Console.WriteLine($"x = {localPos.X}, y = {localPos.Y}, z = {localPos.Z}");
    }
}


/* const float SeaLevelPressure = 1013.25f;
    
    public static (float x, float y) LatLngToMeters(float lat, float lon)
    {
        float r = 6378137; // WGS84 Equatorial radius
        float x = r * lon * Mathf.PI / 180;
        float z = Mathf.Log(Mathf.Tan((90 + lat) * Mathf.PI / 360)) * r;
        return (x, z);
    }

    public static float CalculateAltitude(float currentPressure)
    {
        float y = (Mathf.Pow((SeaLevelPressure / currentPressure), 1f / 5.257f) - 1.0f) * (293.15f) / 0.0065f;
        return y;
    }
    
    public static Vector3 GetPosition(float lat, float lon, float pressure)
    {
        (float x, float z) = LatLngToMeters(lat, lon);
        return new Vector3(x, CalculateAltitude(pressure), z);
    } */