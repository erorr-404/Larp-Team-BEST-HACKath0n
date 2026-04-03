using MissionPlanner.Utilities;

public class App
{
    public static void Main()
    {
        string fileName = "tmp/00000001.BIN";

        BinariesParser parser = new(fileName);
        parser.Parse();

        foreach (var item in parser.GpsRecords)
        {
            Console.WriteLine(item.ToString());
        }


        // string outputCsv = "tmp/log_export.csv";

        // using DFLogBuffer logdata = new(fileName);
        // DFLog dfLog = logdata.dflog;

        // Console.WriteLine("Message types: " + string.Join(", ", logdata.SeenMessageTypes));

        // Console.WriteLine("--- First 10 records (parsed) ---");
        // int shown = 0;
        // foreach (var item in logdata.GetEnumeratorType("GPS[0].Alt"))
        // {
        //     Console.WriteLine(string.Join(",", item.items));
        //     shown++;
        //     if (shown >= 10)
        //         break;
        // }

        // // Example: export only GPS records to CSV
        // using (var writer = new StreamWriter(outputCsv))
        // {
        //     // writer.WriteLine("msgtype,time,raw");

        //     foreach (var item in logdata.GetEnumeratorTypeAll())
        //     {
        //         var raw = string.Join(";", item.items).Replace("\"", "\"\"");
        //         writer.WriteLine($"{item.msgtype},{item.time:O},\"{raw}\"");
        //     }
        // }

        // Console.WriteLine("Export done: " + outputCsv);
    }
}