using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class RealTimeStats : MonoBehaviour
{
    public Transform StatsPanel;

    public void UpdateRealTimeStats(KinematicPoint point)
    {
        StatsPanel.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Час: {point.GetTimeInSecods:F2} секунд";
        StatsPanel.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Широта: {point.Longitude:F6}°";
        StatsPanel.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"Довгота: {point.Latitude:F6}°";
        StatsPanel.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"Висота: {point.Altitude:F2} м";
        StatsPanel.GetChild(5).GetComponent<TextMeshProUGUI>().text = $"Швидкість набору висоти: {point.ClimbRate:F2} м/с";
        StatsPanel.GetChild(6).GetComponent<TextMeshProUGUI>().text = $"Координати (ENU): X={point.Position.X:F2}; Y={point.Position.Y:F2}; Z={point.Position.Z:F2}";
        StatsPanel.GetChild(7).GetComponent<TextMeshProUGUI>().text = $"Швидкість (ENU): X={point.Speed.X:F2}; Y={point.Speed.Y:F2}; Z={point.Speed.Z:F2} м/с";
        StatsPanel.GetChild(8).GetComponent<TextMeshProUGUI>().text = $"Модуль швидкості: {point.GetSpeedMagnitude:F2} м/с";
        StatsPanel.GetChild(9).GetComponent<TextMeshProUGUI>().text = $"Прискорення (ENU): X={point.Acceleration.X:F2}; Y={point.Acceleration.Y:F2}; Z={point.Acceleration.Z:F2} м/с²";
        StatsPanel.GetChild(10).GetComponent<TextMeshProUGUI>().text = $"Модуль прискорення: {point.GetAccelerationMagnitude:F2} м/с²";
        StatsPanel.GetChild(11).GetComponent<TextMeshProUGUI>().text = $"Кватеріон: X={point.Rotation.X:F2}; Y={point.Rotation.Y:F2}; Z={point.Rotation.Z:F2} W={point.Rotation.W:F2}";
    }
}
