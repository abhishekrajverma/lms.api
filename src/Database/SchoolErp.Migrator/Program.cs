using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var scriptsDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "Scripts");
var schemaScript = Path.Combine(scriptsDir, "001-schemas.sql");

if (!File.Exists(schemaScript))
{
    Console.WriteLine($"Schema script not found: {schemaScript}");
    return 1;
}

var sql = await File.ReadAllTextAsync(schemaScript);
var connectionString = configuration.GetConnectionString("SchoolErp")
    ?? "Server=(localdb)\\mssqllocaldb;Database=SchoolErp;Trusted_Connection=True;TrustServerCertificate=True";

await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
await connection.OpenAsync();

foreach (var batch in sql.Split("GO", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
{
    if (string.IsNullOrWhiteSpace(batch))
        continue;
    await using var cmd = connection.CreateCommand();
    cmd.CommandText = batch;
    await cmd.ExecuteNonQueryAsync();
}

Console.WriteLine("Schema migration completed.");
return 0;
