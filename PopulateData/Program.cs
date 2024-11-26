// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;

Console.WriteLine("Populate Data Generator");


DatabaseConfig? config;
if (!File.Exists("config.json"))
{
    config = new DatabaseConfig("Localhost:5432", "blueprint", "postgres", "123456", ["customer", "messaging_log"], "public", "./1. insert_statement V1.2.sql", "./2. generator_query V1.2.sql");
    File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
    Console.WriteLine("Config file created. Please update the config.json file with the correct values.");
    return;
}

var configJson = File.ReadAllText("config.json");
config = JsonConvert.DeserializeObject<DatabaseConfig>(configJson);
if (config == null)
{
    Console.WriteLine("Config file is empty. Please update the config.json file with the correct values.");
    return;
}

Console.WriteLine("Config file loaded.");
//show config except password
Console.WriteLine($"Host: {config.Host}, Database: {config.Database}, Username: {config.Username}, Schema: {config.Schema}, OutputPath: {config.OutputPath}, QueryOutputPath: {config.QueryOutputPath}, TableNames: {string.Join(", ", config.TableNames)}");
Console.Write("Start Generate Script? (Y/N): ");
var key = Console.ReadKey();
Console.WriteLine();
if (key.Key != ConsoleKey.Y)
{
    Console.WriteLine("Exit");
    return;
}

var databaseScript = new DatabaseScript(config);
databaseScript.GenerateScripts(false);
Console.WriteLine("Generate Script Completed");
Console.WriteLine("Press any key to exit");
Console.ReadKey();


