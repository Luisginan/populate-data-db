public class DatabaseConfig
{
    public string Host { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Schema { get; set; }
    public string OutputPath { get; set; }
    public string QueryOutputPath { get; set; }
    public List<string> TableNames { get; set; }

    public DatabaseConfig(
        string host,
        string database,
        string username,
        string password,
        List<string> tableNames,
        string schema = "public",
        string outputPath = "insert_statements.sql",
        string queryOutputPath = "generate_insert_query.sql")
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentException("Host is required");
        if (string.IsNullOrEmpty(database))
            throw new ArgumentException("Database is required");
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required");
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password is required");
        if (tableNames == null || tableNames.Count == 0)
            throw new ArgumentException("TableNames is required and cannot be empty");

        Host = host;
        Database = database;
        Username = username;
        Password = password;
        TableNames = tableNames;
        Schema = schema;
        OutputPath = outputPath;
        QueryOutputPath = queryOutputPath;
    }

    public string GetConnectionString()
    {
        return $"Host={Host};Database={Database};Username={Username};Password={Password}";
    }
}