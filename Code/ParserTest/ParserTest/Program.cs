using System;
using MissionPlanner.Utilities;

public class KinematicDataProcessor
{
    /// <summary>
    /// Отримує масив KinematicPoint з бінарного файлу, використовуючи BinaryParser для парсингу та KinematicCalculator для конвертації даних.
    /// </summary>
    /// <param name="fileName"> Шилях до бінарного файлу </param>
    /// <param name="minNStats"> Мінімальна кількість супутників (NSats) для включення GPS-даних у розрахунок кінематики. Записи з NSats менше цього значення будуть ігноруватися. </param>
    /// <returns> Масив KinematicPoint, який містить кінематичні дані, обчислені на основі GPS, IMU та BARO записів з бінарного файлу. </returns>
    /// <exception cref="Exception"> Викидає виключення, якщо виникає помилка під час обробки даних, наприклад, якщо файл не може бути прочитаний або якщо не вдалося обчислити жодної кінематичної точки через недостатню кількість супутників. </exception>
    public static KinematicPoint[] GetKinematicPointsFromBinaryFile(string fileName, int minNStats, Action<String> debugLog)
    {
        try
        {
            BinaryParser parser = new BinaryParser(fileName);
            parser.Parse();

            KinematicCalculator calculator = new KinematicCalculator(minNStats);
            calculator.CalculateKinematicPointsFromRecords(parser.GpsRecords, parser.ImuRecords, parser.BaroRecords, debugLog);

            return calculator.kinematicPoints;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to process kinematic data from file {fileName}: {ex.Message}", ex);
        }
    }
}