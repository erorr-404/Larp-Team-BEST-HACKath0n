
using System.Numerics;

public readonly record struct KinematicPoint
{
    public readonly double Timestamp; // час у секундах відносно старту

    // оригінальні дані gps
    public readonly double Longitude;
    public readonly double Latitude;
    public readonly float Altitude;

    public readonly Vector3 Position; // позиція у Декартових координатах (ENU)
    public readonly float LinearSpeed;
    public readonly float LinearAcceleration;

    public readonly Quaternion Rotation;
    public readonly Vector3 angularSpeed;
    public readonly Vector3 angularAcceleration;
}