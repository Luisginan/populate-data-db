using Newtonsoft.Json;
Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Populate Data Generator ===\n");
Console.ResetColor();

DatabaseConfig? config;
if (!File.Exists("config.json"))
{
    config = new DatabaseConfig(
        "Localhost:5432",
        "blueprint", 
        "postgres",
        "123456",
        ["customer", "messaging_log"],
        "public",
        "./1. insert_statement V1.2.sql",
        "./2. generator_query V1.2.sql"
    );
    File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Config file created. Please update the 'config.json' file with the correct values.");
    Console.ResetColor();
    return;
}

var configJson = File.ReadAllText("config.json");
config = JsonConvert.DeserializeObject<DatabaseConfig>(configJson);
if (config == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Config file is empty or invalid. Please update the 'config.json' file with the correct values.");
    Console.ResetColor();
    return;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Config file loaded successfully.\n");
Console.ResetColor();

Console.WriteLine("Configuration:");
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine($"  Host: {config.Host}");
Console.WriteLine($"  Database: {config.Database}");
Console.WriteLine($"  Username: {config.Username}");
Console.WriteLine($"  Schema: {config.Schema}");
Console.WriteLine($"  Output Path: {config.OutputPath}");
Console.WriteLine($"  Query Output Path: {config.QueryOutputPath}");
Console.WriteLine("  Table Names:");
foreach (var tableName in config.TableNames)
{
    Console.WriteLine($"    - {tableName}");
}
Console.ResetColor();

Console.Write("\nStart generating scripts? (Y/N): ");
var key = Console.ReadKey();
Console.WriteLine();
if (key.Key != ConsoleKey.Y)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Operation cancelled. Exiting...");
    Console.ResetColor();
    return;
}

var databaseScript = new DatabaseScript(config);
databaseScript.GenerateScripts(false);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nScript generation completed successfully.");
Console.ResetColor();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();