using UnityEngine;


/// <summary>
/// Цей клас обчислює статистику польоту, включаючи максимальну швидкість, прискорення, швидкість підйому та тривалість польоту на основі масиву KinematicPoint.
/// </summary>
class FlightStats
{
    public float MaxVelocity { get; private set; }
    public float MaxAcceleration { get; private set; }
    public float MaxClimbRate { get; private set; }
    public double FlightDuration {get; private set;}

    /// <summary>
    /// Конструктор, який приймає масив KinematicPoint і обчислює статистику польоту.
    /// </summary>
    /// <param name="flightData"> Масив точок кінематики </param>
    public FlightStats(KinematicPoint[] flightData)
    {
        MaxVelocity = 0f;
        MaxAcceleration = 0f;
        MaxClimbRate = 0f;
        FlightDuration = flightData[^1].Timestamp - flightData[0].Timestamp; // в секундах

        float prevAlt = flightData[0].Altitude;

        foreach (KinematicPoint kinematicPoint in flightData)
        {   
            // знаходження максимальної швидкості
            float velocityMag = kinematicPoint.GetSpeedMagnitude;
            if (velocityMag > MaxVelocity) MaxVelocity = velocityMag;
            
            // знаходження максимального прискорення
            float accelerationMag = kinematicPoint.GetAccelerationMagnitude;
            if (accelerationMag > MaxAcceleration) MaxAcceleration = accelerationMag;

            float altDelta = Mathf.Abs(prevAlt - kinematicPoint.Altitude);
            if (altDelta > MaxClimbRate) MaxClimbRate = altDelta;
        }
    }
}