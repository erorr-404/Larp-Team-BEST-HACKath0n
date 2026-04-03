using MissionPlanner.Utilities;

public class App
{
    public static void Main()
    {

        // ❗ Як запускати: dotnet run > tmp/struct.log
        // Файл можна взяти інший, головгне не знищити корисні дані

        string fileName = "tmp/00000001.BIN";

        BinaryParser parser = new(fileName);
        parser.Parse();

        Console.WriteLine("--- GPS Records ---");
        foreach (var item in parser.GpsRecords)
        {
            Console.WriteLine(item.ToString());
        }

        Console.WriteLine("--- IMU Records ---");
        foreach (var item in parser.ImuRecords)
        {
            Console.WriteLine(item.ToString());
        }

        Console.WriteLine("--- BARO Records ---");
        foreach (var item in parser.BaroRecords)
        {
            Console.WriteLine(item.ToString());
        }
    }
}