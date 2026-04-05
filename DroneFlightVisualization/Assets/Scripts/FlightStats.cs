using System;
using UnityEngine;


/// <summary>
/// Цей клас обчислює статистику польоту, включаючи максимальну швидкість, прискорення, швидкість підйому та тривалість польоту на основі масиву KinematicPoint.
/// </summary>
class FlightStats
{
    private const double _earthRadiusMeters = 6371000.0;

    public float MaxVelocity { get; private set; }
    public float MaxAcceleration { get; private set; }
    public float MaxClimbRate { get; private set; }
    public double FlightDuration { get; private set; }
    public double FlightDistance { get; private set; }

    /// <summary>
    /// Конструктор, який приймає масив KinematicPoint і обчислює статистику польоту.
    /// </summary>
    /// <param name="kinematicPoints"> Масив точок кінематики </param>
    public FlightStats(KinematicPoint[] kinematicPoints)
    {
        if (kinematicPoints == null || kinematicPoints.Length == 0)
        {
            Debug.LogError("Flight data is null or empty. Cannot calculate flight statistics.");
            throw new System.ArgumentException("Flight data cannot be null or empty.");
        }

        FlightDistance = 0f;
        MaxVelocity = 0f;
        MaxAcceleration = 0f;
        MaxClimbRate = 0f;
        FlightDuration = (kinematicPoints[^1].Timestamp - kinematicPoints[0].Timestamp) / 1000.0 / 60; // в хвилинах

        KinematicPoint prevKinematicPoint = default;
        bool hasPreviousPoint = false;

        foreach (KinematicPoint kinematicPoint in kinematicPoints)
        {   
            // знаходження максимальної швидкості
            float velocityMag = kinematicPoint.GetSpeedMagnitude;
            if (velocityMag > MaxVelocity) MaxVelocity = velocityMag;
            
            // знаходження максимального прискорення
            float accelerationMag = kinematicPoint.GetAccelerationMagnitude;
            if (accelerationMag > MaxAcceleration) MaxAcceleration = accelerationMag;

            float climbRate = kinematicPoint.ClimbRate;
            if (climbRate > MaxClimbRate) MaxClimbRate = climbRate;

            if (hasPreviousPoint)
            {
                FlightDistance += CalculateHaversineDistance(prevKinematicPoint, kinematicPoint);
            }

            prevKinematicPoint = kinematicPoint;
            hasPreviousPoint = true;
        }
    }


    /// <summary>
    /// Ця функція обчислює відстань між двома координатами WGS-89
    /// </summary>
    /// <param name="point1"> Перша точка </param>
    /// <param name="point2"> Друга точка </param>
    /// <returns> Відстань у метрах </returns>
    public static double CalculateHaversineDistance(KinematicPoint point1, KinematicPoint point2)
    {
        // Конвертуємо градуси у радіани
        double lat1Rad = DegreesToRadians(point1.Latitude);
        double lat2Rad = DegreesToRadians(point2.Latitude);
        
        double deltaLat = DegreesToRadians(point2.Latitude - point1.Latitude);
        double deltaLon = DegreesToRadians(point2.Longitude - point1.Longitude);

        // Формула гаверсинусів
        double a = Math.Sin(deltaLat / 2.0) * Math.Sin(deltaLat / 2.0) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2.0) * Math.Sin(deltaLon / 2.0);

        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        // Повертаємо дистанцію у метрах
        return _earthRadiusMeters * c;
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }
}