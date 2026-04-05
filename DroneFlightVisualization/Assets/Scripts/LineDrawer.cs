using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using SVector3 = System.Numerics.Vector3;
using Quaternion = UnityEngine.Quaternion;
using SQuaternion = System.Numerics.Quaternion;
using Debug = UnityEngine.Debug;
using System.Numerics;
using UnityEngine.UI;
using TMPro;
using SFB;
using Mapbox.Unity.Map;
using System.IO;
using System.Text;

public class LineDrawer : MonoBehaviour
{
    public Gradient gradient;
    public LineRenderer lineRenderer;
    public Transform Target;
    public Slider TimeSlider;
    public TextMeshProUGUI PauseButtonText;
    public Transform StatsPanel;
    public RealTimeStats RealTimeStatsDisplay;
    public AbstractMap map;

    public bool IsPlaying = false;
    public float TimeScale = 1f;
    
    KinematicPoint[] kinematicPoints;

    public int SatsThreshold = 5;

    public void Load(string filePath)
    {
        try
        {
            kinematicPoints = KinematicDataProcessor.GetKinematicPointsFromBinaryFile(filePath, SatsThreshold, (string Message) => { Debug.Log(Message); });
            DrawColoredTrajectory(kinematicPoints);

            var flightStats = new FlightStats(kinematicPoints);

            StatsPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Макс. швидкість: {flightStats.MaxVelocity:F2} м/с";
            StatsPanel.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Макс. прискорення: {flightStats.MaxAcceleration:F2} м/с²";
            StatsPanel.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"Макс. швидкість підйому: {flightStats.MaxClimbRate:F2} м/с";
            StatsPanel.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"Тривалість польоту: {flightStats.FlightDuration:F2} хв";
            StatsPanel.GetChild(5).GetComponent<TextMeshProUGUI>().text = $"Дистанція польоту: {flightStats.FlightDistance:F2} м";

            
            var mapPos = new Mapbox.Utils.Vector2d(kinematicPoints[0].Latitude, kinematicPoints[0].Longitude);

            map.SetCenterLatitudeLongitude(mapPos);
            // Центруємо карту по координатах точки зльоту

            map.SetCenterLatitudeLongitude(mapPos);
            map.UpdateMap(mapPos);

            SetTime(0f);
        }
        catch (System.ArgumentException ex)
        {
            Debug.LogError($"Failed to calculate flight statistics and display trajectory. Maybe flightStats.Length == 0: {ex.Message}");
            return;
        } catch (System.Exception ex)
        {
            Debug.LogError($"An unexpected error occurred while loading the file: {ex.Message}");
            return;
        }
    }

    public void LoadFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Оберіть файл", "", "", false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedFilePath = paths[0];
            Load(selectedFilePath);
        }
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
            float currentSpeed = flightData[i].GetSpeedMagnitude;
            
            if (currentSpeed > maxSpeed)
                maxSpeed = currentSpeed;
                
            positions[i] = ConvertToUnityVector(flightData[i].Position);
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

            Debug.Log($"Time: {flightData[i].GetTimeInSecods} s, NSats: ... Lat: {flightData[i].Latitude}, Lon: {flightData[i].Longitude}, Alt: {flightData[i].Altitude} m, Position: {flightData[i].Position}");
        }

        colorTexture.Apply();
        lineRenderer.material.mainTexture = colorTexture;
    }

    public void SetTime(float value)
    {
        if (kinematicPoints == null) return;
        var point = Mathf.Lerp(0, kinematicPoints.Length - 1, value);

        Target.localPosition = ConvertToUnityVector(kinematicPoints[(int)point].Position);
        Target.GetChild(0).transform.rotation = ConvertToUnityQuaternion(kinematicPoints[(int)point].Rotation);

        RealTimeStatsDisplay.UpdateRealTimeStats(kinematicPoints[(int)point]);
    }

    public void ChangePlayState()
    {
        IsPlaying = !IsPlaying;
        PauseButtonText.text = IsPlaying ? "||" : ">";
    }

    public void SetTimeScale(int value)
    {
        switch (value)
        {
            case 0:
                TimeScale = -1f;
                break;
            case 1:
                TimeScale = 1f;
                break;
            case 2:
                TimeScale = 2f;
                break; 
            case 3:
                TimeScale = 4f;
                break; 
            case 4:
                TimeScale = 8f;
                break; 
            default:
                TimeScale = 1f;
                break;
        }
    }

    void Update()
    {
        if (!IsPlaying) return;
        var value = TimeSlider.value + Time.deltaTime / kinematicPoints.Length * 100f * TimeScale;
        value = Mathf.Clamp(value, 0f, 1f);
        TimeSlider.value = value;
        SetTime(value);
    }

    public void SaveToFile()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "");
        if (path.Length == 0) return;
        SaveKinematicPoints(path);
    }


    public void SaveKinematicPoints(string filePath)
    {
        if (kinematicPoints.Length == 0) throw new System.Exception("No Kinemaptic Points to save.");

        var sb = new StringBuilder();

        foreach (KinematicPoint point in kinematicPoints)
            sb.AppendLine(point.ToString());
        
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log("Saved to: " + filePath);
    }

    Vector3 ConvertToUnityVector(SVector3 vector)
    {
        // Конвертуємо вектор з System.Numerics.Vector3 у UnityEngine.Vector3
        return new Vector3(vector.X, vector.Z, vector.Y);
    }

    Quaternion ConvertToUnityQuaternion(SQuaternion quaternion)
    {
        // Конвертуємо кватерніон з System.Numerics.Quaternion у UnityEngine.Quaternion
        return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }
}
