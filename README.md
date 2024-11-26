# Introduction
This project is designed to generate SQL insert statements for specified tables in a PostgreSQL database. It reads the database schema and generates the necessary SQL scripts to populate the tables with data.

# Getting Started
Follow these steps to get the project up and running on your system:

1. **Installation Process**
    - Clone the repository to your local machine.
    - Open the project in your preferred IDE (e.g., JetBrains Rider).

2. **Software Dependencies**
    - .NET 6.0 or later
    - PostgreSQL database
    - Newtonsoft.Json library

3. **Configuration**
    - Create a `config.json` file in the project root with the following structure:
      ```json
      {
        "Host": "Localhost:5432",
        "Database": "blueprint",
        "Username": "postgres",
        "Password": "123456",
        "TableNames": ["customer", "messaging_log"],
        "Schema": "public",
        "OutputPath": "./1. insert_statement V1.2.sql",
        "QueryOutputPath": "./2. generator_query V1.2.sql"
      }
      ```

# Build and Test
To build and test the project:

1. Open the project in your IDE.
2. Build the project to restore dependencies and compile the code.
3. Run the application to generate the SQL scripts.

# Contribute
Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes and push the branch to your fork.
4. Create a pull request with a description of your changes.

For more information on creating good readme files, refer to the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)