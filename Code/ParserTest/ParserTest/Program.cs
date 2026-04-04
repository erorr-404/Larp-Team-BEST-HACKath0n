using MissionPlanner.Utilities;

public class KinematicDataProcessor
{
    // public static void Main()
    // {

    //     // ❗ Як запускати: dotnet run > tmp/struct.log
    //     // Файл можна взяти інший, головгне не знищити корисні дані

    //     // string fileName = "/home/maxym/Документи/BestHackathon/Code/ParserTest/ParserTest/tmp/00000001.BIN";

    //     // BinaryParser parser = new(fileName);
    //     // parser.Parse();

    //     // KinematicCalculator calculator = new();
    //     // calculator.CalculateKinematicPointsFromRecords(parser.GpsRecords, parser.ImuRecords, parser.BaroRecords);

    //     // foreach (KinematicPoint point in calculator.kinematicPoints)
    //     // {
    //     //     Console.WriteLine(point.ToString());
    //     // }

    //     // Console.WriteLine("--- GPS Records ---");
    //     // foreach (var item in parser.GpsRecords)
    //     // {
    //     //     Console.WriteLine(item.ToString());
    //     // }

    //     // Console.WriteLine("--- IMU Records ---");
    //     // foreach (var item in parser.ImuRecords)
    //     // {
    //     //     Console.WriteLine(item.ToString());
    //     // }

    //     // Console.WriteLine("--- BARO Records ---");
    //     // foreach (var item in parser.BaroRecords)
    //     // {
    //     //     Console.WriteLine(item.ToString());
    //     // }
    // }

    public static KinematicPoint[] GetKinematicPointsFromBinaryFile(string fileName)
    {
        BinaryParser parser = new(fileName);
        parser.Parse();

        KinematicCalculator calculator = new();
        calculator.CalculateKinematicPointsFromRecords(parser.GpsRecords, parser.ImuRecords, parser.BaroRecords);

        return calculator.kinematicPoints;
    }
}