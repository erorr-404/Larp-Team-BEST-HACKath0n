
using System.Numerics;


/// <summary>
/// Структура для зберігання кінематичних даних, включаючи час, GPS-координати, позицію у Декартових координатах, швидкість, прискорення, а також орієнтацію та кутову швидкість.
/// Ця структура є результатом конвертації сирих даних GPS, IMU та BARO у більш зручний формат для аналізу та візуалізації.
/// </summary>
public struct KinematicPoint
{
    /// <summary> Час у мікросекундах відносно старту. </summary>
    public  double Timestamp;

    /// <summary> Повертає час у секундах </summary>
    public double GetTimeInSecods => Timestamp / 1000.0;

    // оригінальні дані gps

    /// <summary> Широта, градуси </summary>
    public  double Longitude;
    
    /// <summary> Довгота, градуси </summary>
    public  double Latitude;

    /// <summary> Висота над рівнем моря, метри </summary>
    public  float Altitude;

    /// <summary> Швидкісь набору всисоти, метри на секунду </summary>
    public float ClimbRate;

    // дані, отримані в результаті конвертації gps координат у декартові координати відносно стартової точки

    /// <summary> Позиція у Декартових координатах (ENU), метри </summary>
    public  Vector3 Position;

    /// <summary> Вектор лінійної швидкості у Декартових координатах (ENU), м/с </summary>
    public  Vector3 Speed;

    /// <summary>
    /// Обчислює модуль (magnitude) лінійної швидкості на основі компонентів Speed за формулою: sqrt(SpeedX^2 + SpeedY^2 + SpeedZ^2).
    /// </summary>
    /// <returns> Модуль (magnitude) лінійної швидкості, m/s </returns>
    public float GetSpeedMagnitude => Speed.Length();

    /// <summary>
    /// Вектор лінійного прискорення у Декартових координатах (ENU), м/с².
    /// </summary>
    public  Vector3 Acceleration;
    
    /// <summary>
    /// Обчислює модуль (magnitude) лінійного прискорення на основі компонентів Acceleration за формулою: sqrt(AccX^2 + AccY^2 + AccZ^2).
    /// </summary>
    /// <returns> Модуль (magnitude) лінійного прискорення, m/s² </returns>

    public float GetAccelerationMagnitude => Acceleration.Length();
    
    // поворот та кутова швидкість, отримані з даних IMU

    /// <summary> Орієнтація у вигляді кватерніона </summary>
    public  Quaternion Rotation;

    /// <summary> Кутова швидкість, рад/с </summary>
    public  Vector3 angularSpeed;


    public override string ToString()
    {
        return $"Time: {Timestamp}, Lat: {Latitude}, Lng: {Longitude}, Alt: {Altitude}, Pos: {Position}, Spd: {Speed}, Acc: {Acceleration}, Rot: {Rotation}, AngularSpd: {angularSpeed}";
    }
}