using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Npgsql;

public class DatabaseScript
{
    

    private readonly DatabaseConfig _config;
    private readonly List<string> _generatedQueries;

    public DatabaseScript(DatabaseConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _generatedQueries = new List<string>();
    }

    public DatabaseScript(
        string host,
        string database,
        string username,
        string password,
        List<string> tableNames,
        string schema = "public",
        string outputPath = "insert_statements.sql",
        string queryOutputPath = "generate_insert_query.sql")
    {
        _config = new DatabaseConfig(
            host,
            database,
            username,
            password,
            tableNames,
            schema,
            outputPath,
            queryOutputPath
        );
        _generatedQueries = new List<string>();
    }

    private string GetValueFormatting(string columnName, string dataType, string udtName)
    {
        // Handle array types
        if (dataType.EndsWith("[]"))
        {
            return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ARRAY_TO_STRING({columnName}, ',') || '''' END";
        }

        switch (dataType.ToLower())
        {
            // Numeric types
            case "smallint":
            case "integer":
            case "bigint":
            case "serial":
            case "bigserial":
            case "smallserial":
            case "oid":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE {columnName}::text END";

            case "decimal":
            case "numeric":
            case "real":
            case "double precision":
            case "money":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE ({columnName})::text END";

            // Character types
            case "character varying":
            case "varchar":
            case "character":
            case "char":
            case "text":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || REPLACE({columnName}::text, '''', '''''') || '''' END";

            // Date/Time types
            case "timestamp without time zone":
            case "timestamp with time zone":
            case "timestamp":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || TO_CHAR({columnName}, 'YYYY-MM-DD HH24:MI:SS.US') || '''' END";
            
            case "date":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || TO_CHAR({columnName}, 'YYYY-MM-DD') || '''' END";
            
            case "time without time zone":
            case "time with time zone":
            case "time":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || TO_CHAR({columnName}, 'HH24:MI:SS.US') || '''' END";

            case "interval":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";

            // Boolean type
            case "boolean":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' WHEN {columnName} THEN 'true' ELSE 'false' END";

            // Binary data
            case "bytea":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '\\x' || encode({columnName}, 'hex') END";

            // Network address types
            case "cidr":
            case "inet":
            case "macaddr":
            case "macaddr8":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";

            // Geometric types
            case "point":
            case "line":
            case "lseg":
            case "box":
            case "path":
            case "polygon":
            case "circle":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";

            // JSON types
            case "json":
            case "jsonb":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || REPLACE(({columnName})::text, '''', '''''') || '''' END";

            // UUID type
            case "uuid":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";

            // XML type
            case "xml":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || REPLACE(({columnName})::text, '''', '''''') || '''' END";

            // Bit string types
            case "bit":
            case "bit varying":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' " +
                       $"WHEN {columnName}::text = '1' THEN 'B''1''' " +
                       $"WHEN {columnName}::text = '0' THEN 'B''0''' " +
                       $"ELSE 'B''' || ({columnName})::text || '''' END";

            // Range types
            case "int4range":
            case "int8range":
            case "numrange":
            case "tsrange":
            case "tstzrange":
            case "daterange":
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";

            // Domain types or custom types
            default:
                // Handle ENUM types
                if (udtName.Contains("_enum_"))
                {
                    return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";
                }
                // Default handling for unknown types
                return $"CASE WHEN {columnName} IS NULL THEN 'NULL' ELSE '''' || ({columnName})::text || '''' END";
        }
    }

    private string BuildSelectQuery(string tableName)
    {
        var columnQuery = @"
            SELECT 
                column_name, 
                data_type,
                udt_name,
                is_nullable
            FROM 
                information_schema.columns 
            WHERE 
                table_schema = @schema 
                AND table_name = @tableName
            ORDER BY 
                ordinal_position;";

        var selectBuilder = new StringBuilder();
        selectBuilder.AppendLine($"-- Query to generate INSERT statements for table: {tableName}");
        selectBuilder.Append($"SELECT 'INSERT INTO {_config.Schema}.{tableName} (");

        using (var connection = new NpgsqlConnection(_config.GetConnectionString()))
        {
            connection.Open();
            var columnNames = new StringBuilder();
            var valuesPart = new StringBuilder();

            using (var command = new NpgsqlCommand(columnQuery, connection))
            {
                command.Parameters.AddWithValue("@schema", _config.Schema);
                command.Parameters.AddWithValue("@tableName", tableName);

                using (var reader = command.ExecuteReader())
                {
                    var isFirst = true;
                    while (reader.Read())
                    {
                        string columnName = reader["column_name"].ToString();
                        string dataType = reader["data_type"].ToString();
                        string udtName = reader["udt_name"].ToString();

                        if (!isFirst)
                        {
                            columnNames.Append(", ");
                            valuesPart.Append(" || ', ' || ");
                        }

                        columnNames.Append(columnName.Contains(" ") ? $"\"{columnName}\"" : columnName);
                        valuesPart.Append(GetValueFormatting(columnName, dataType, udtName));

                        isFirst = false;
                    }
                }
            }

            selectBuilder.Append(columnNames);
            selectBuilder.Append(") VALUES (' || ");
            selectBuilder.Append(valuesPart);
            selectBuilder.Append(" || ');' AS script");
            selectBuilder.AppendLine($" FROM {_config.Schema}.{tableName};");
        }

        string query = selectBuilder.ToString();
        _generatedQueries.Add(query);
        return query;
    }

    private void SaveGeneratedQueries()
    {
        try
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("-- Generated SQL queries to create INSERT statements");
            queryBuilder.AppendLine($"-- Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            queryBuilder.AppendLine("-- This file contains the queries used to generate the INSERT statements");
            queryBuilder.AppendLine();

            foreach (var query in _generatedQueries)
            {
                queryBuilder.AppendLine(query);
                queryBuilder.AppendLine();
            }

            File.WriteAllText(_config.QueryOutputPath, queryBuilder.ToString());
            Console.WriteLine($"Successfully saved generating queries to {_config.QueryOutputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving generating queries: {ex.Message}");
        }
    }

    public void GenerateScripts(bool printToConsole = false)
    {
        try
        {
            _generatedQueries.Clear();

            using (var writer = new System.IO.StreamWriter(_config.OutputPath))
            {
                writer.WriteLine($"-- Generated INSERT statements");
                writer.WriteLine($"-- Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                foreach (var tableName in _config.TableNames)
                {
                    var commentLine = $"-- Insert statements for table: {tableName}";
                    writer.WriteLine(commentLine);
                    if (printToConsole) Console.WriteLine(commentLine);

                    string query = BuildSelectQuery(tableName);
                    
                    using (var connection = new NpgsqlConnection(_config.GetConnectionString()))
                    {
                        connection.Open();
                        using (var command = new NpgsqlCommand(query, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var script = reader["script"].ToString();
                                writer.WriteLine(script);
                                if (printToConsole) Console.WriteLine(script);
                            }
                        }
                    }

                    writer.WriteLine();
                    if (printToConsole) Console.WriteLine();
                }
            }
            Console.WriteLine($"Successfully generated insert statements and saved to {_config.OutputPath}");

            SaveGeneratedQueries();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating insert statements: {ex.Message}");
            Console.WriteLine($"Details: {ex.StackTrace}");
        }
    }
}