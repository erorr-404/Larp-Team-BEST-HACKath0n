
using System.Numerics;


/// <summary>
/// Структура для зберігання кінематичних даних, включаючи час, GPS-координати, позицію у Декартових координатах, швидкість, прискорення, а також орієнтацію та кутову швидкість.
/// Ця структура є результатом конвертації сирих даних GPS, IMU та BARO у більш зручний формат для аналізу та візуалізації.
/// </summary>
public readonly record struct KinematicPoint
{
    /// <summary> Час у секундах відносно старту. </summary>
    public readonly double Timestamp; // час у секундах відносно старту

    // оригінальні дані gps

    /// <summary> Широта, градуси </summary>
    public readonly double Longitude;
    
    /// <summary> Довгота, градуси </summary>
    public readonly double Latitude;

    /// <summary> Висота над рівнем моря, метри </summary>
    public readonly float Altitude;

    /// <summary> Позиція у Декартових координатах (ENU), метри </summary>
    public readonly Vector3 Position;

    /// <summary> Лінійна швидкість, м/с </summary>
    public readonly float LinearSpeed;

    /// <summary> Лінійне прискорення, м/с² </summary>
    public readonly float LinearAcceleration;


    /// <summary> Орієнтація у вигляді кватерніона </summary>
    public readonly Quaternion Rotation;

    /// <summary> Кутова швидкість, рад/с </summary>
    public readonly Vector3 angularSpeed;
    
    /// <summary> Кутове прискорення, рад/с² </summary>
    public readonly Vector3 angularAcceleration;
}