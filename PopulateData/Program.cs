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

config = JsonConvert.DeserializeObject<DatabaseConfig>(File.ReadAllText("config.json"));
if (config == null)
{
    Console.WriteLine("Config file is empty. Please update the config.json file with the correct values.");
    return;
}

var databaseScript = new DatabaseScript(config);
databaseScript.GenerateScripts(true);

