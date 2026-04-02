using MissionPlanner.Utilities;

public class App
{
    public static void Main()
    {
        string fileName = "00000001.BIN";
        string outputCsv = "log_export.csv";

        using DFLogBuffer logdata = new(fileName);
        DFLog dfLog = logdata.dflog;

        Console.WriteLine("Message types: " + string.Join(", ", logdata.SeenMessageTypes));

        Console.WriteLine("--- First 10 records (parsed) ---");
        int shown = 0;
        foreach (var item in logdata.GetEnumeratorTypeAll())
        {
            Console.WriteLine(string.Join(",", item.items));
            shown++;
            if (shown >= 10)
                break;
        }

        // Example: export only GPS records to CSV
        using (var writer = new StreamWriter(outputCsv))
        {
            writer.WriteLine("msgtype,time,raw");

            foreach (var item in logdata.GetEnumeratorType("GPS"))
            {
                var raw = string.Join(";", item.items).Replace("\"", "\"\"");
                writer.WriteLine($"{item.msgtype},{item.time:O},\"{raw}\"");
            }
        }

        Console.WriteLine("Export done: " + outputCsv);
    }
}