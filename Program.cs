using System.Globalization;
using CsvHelper;
using Newtonsoft.Json;
using SpbuVkApiCrawler;
using SpbuVkApiCrawler.Models;

var secrets = JsonConvert.DeserializeObject<Secrets>(File.ReadAllText("secrets.json"));

switch (args.Length)
{
    // Check if --help argument is provided
    case 1 when args[0] == "--help":
        ShowHelp();
        return;
    // Parse command line arguments
    case < 3:
        ShowError("Invalid number of arguments. Please provide start/end date and at least one university query.");
        return;
}

if (!DateTime.TryParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
{
    ShowError("Invalid start date format. Please provide the start date in yyyy-MM-dd format.");
    return;
}

if (!DateTime.TryParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
{
    ShowError("Invalid end date format. Please provide the end date in yyyy-MM-dd format.");
    return;
}

string?[] queries = args[2..];

var filename = $"{queries[0]}_{startTime:yyyy-dd-M}_{endTime:yyyy-dd-M}.csv";

var vkApi = new VkApi(secrets.ServiceApiKey, secrets.Version);

await using var writer = new StreamWriter(filename);
await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
csv.WriteHeader<PostData>();
csv.NextRecord();
var vkCrawler = new VkCrawler(vkApi, startTime, endTime, csv);
foreach (var query in queries)
{
    await vkCrawler.GetByQueryAsync(query);
}
return;

void ShowHelp()
{
    Console.WriteLine("Crawls vk for posts with query terms. Outputs <query1><start><end>.csv");
    Console.WriteLine("Usage: dotnet run -- <start_date> <end_date> <query1> [<query2> ...]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  start_date - The start date in yyyy-MM-dd format.");
    Console.WriteLine("  end_date   - The end date in yyyy-MM-dd format.");
    Console.WriteLine("  query1+     - University query terms.");
}

void ShowError(string message)
{
    Console.WriteLine("Error: " + message);
    Console.WriteLine("Run the program with --help argument for usage information.");
}