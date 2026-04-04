using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using SVector3 = System.Numerics.Vector3;
using Debug = UnityEngine.Debug;

public class LineDrawer : MonoBehaviour
{
    public Gradient gradient;
    public LineRenderer lineRenderer;
    public Transform Target;
    
    KinematicPoint[] kinematicPoints;

    void Load(string filePath)
    {
        // TODO: додати вибір файлу через діалогове вікно, а не хардкодити шлях до файлу
        kinematicPoints = KinematicDataProcessor.GetKinematicPointsFromBinaryFile(filePath);
    }

    void DrawLine()
    {
        if (kinematicPoints == null || kinematicPoints.Length == 0)
        {
            Debug.LogError("No kinematic points to draw.");
            return;
        }

        lineRenderer.positionCount = kinematicPoints.Length;

        for (int i = 0; i < kinematicPoints.Length; i++)
        {
            lineRenderer.SetPosition(i, ConvertToUnityVector(kinematicPoints[i].Position));
            Debug.Log($"Point {i}: Position = {kinematicPoints[i].Position}, Speed = {kinematicPoints[i].GetSpeedMagnitude} m/s, Acceleration = {kinematicPoints[i].GetAccelerationMagnitude} m/s²");
            Debug.Log($"Point {i}: Time = {kinematicPoints[i].GetTimeInSecods} s, Latitude = {kinematicPoints[i].Latitude}, Longitude = {kinematicPoints[i].Longitude}, Altitude = {kinematicPoints[i].Altitude} m");
        }
    }

    // Сюди ви маєте передати ваш розпарсений масив даних
    public void DrawColoredTrajectory(KinematicPoint[] flightData)
    {
        // Захист від порожніх даних
        if (flightData == null || flightData.Length == 0) 
        {
            Debug.LogWarning("Дані польоту порожні!");
            return;
        }

        int pointCount = flightData.Length;
        
        lineRenderer.positionCount = pointCount;
        
        // Створюємо масив саме з Unity-векторів (UnityEngine.Vector3)
        UnityEngine.Vector3[] positions = new UnityEngine.Vector3[pointCount];

        // Змінна для пошуку максимальної швидкості
        float maxSpeed = 0f;
        
        for (int i = 0; i < pointCount; i++)
        {
            // Використовуємо вашу властивість для отримання модуля швидкості
            float currentSpeed = flightData[i].GetSpeedMagnitude;
            
            if (currentSpeed > maxSpeed)
                maxSpeed = currentSpeed;
                
            // КРИТИЧНИЙ МОМЕНТ: Конвертація System.Numerics.Vector3 у UnityEngine.Vector3
            // Звертаємося до X, Y, Z (у System.Numerics вони з великої літери)
            positions[i] = new UnityEngine.Vector3(
                flightData[i].Position.X, 
                flightData[i].Position.Y, 
                flightData[i].Position.Z
            );
        }
        
        // Передаємо конвертовані позиції в лінію
        lineRenderer.SetPositions(positions);

        // --- Генерація градієнтної текстури ---
        
        Texture2D colorTexture = new Texture2D(pointCount, 1);
        colorTexture.wrapMode = TextureWrapMode.Clamp;
        colorTexture.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < pointCount; i++)
        {
            // Нормалізуємо швидкість. Додано перевірку на нуль, щоб уникнути ділення на 0, 
            // якщо дрон весь лог простояв на місці.
            float speedRatio = maxSpeed > 0f ? (flightData[i].GetSpeedMagnitude / maxSpeed) : 0f;

            // Змішуємо кольори (Синій - повільно, Червоний - швидко)
            Color pointColor = gradient.Evaluate(speedRatio);

            colorTexture.SetPixel(i, 0, pointColor);
        }

        colorTexture.Apply();
        lineRenderer.material.mainTexture = colorTexture;
    }

    public void SetTime(float value)
    {
        var point = Mathf.Lerp(0, kinematicPoints.Length - 1, value);

        Target.position = ConvertToUnityVector(kinematicPoints[(int)point].Position);
    }

    void Start()
    {
        Load("/home/maxym/Документи/BestHackathon/Code/ParserTest/ParserTest/tmp/00000001.BIN");
        // Load("/home/maxym/Документи/BestHackathon/Code/ParserTest/ParserTest/tmp/00000019.BIN");
        DrawColoredTrajectory(kinematicPoints);
        DrawLine();
    }

    Vector3 ConvertToUnityVector(SVector3 vector)
    {
        // Конвертуємо вектор з System.Numerics.Vector3 у UnityEngine.Vector3
        return new Vector3(vector.X, vector.Z, vector.Y);
    }
}
