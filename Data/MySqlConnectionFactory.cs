using MySql.Data.MySqlClient;

namespace SistemaCredenciamento.Data;

public sealed class MySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "A connection string 'MySql' is missing. Configure it with .NET User Secrets.");
    }

    public MySqlConnection CreateConnection() => new(_connectionString);
}
