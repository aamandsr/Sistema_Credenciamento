using MySql.Data.MySqlClient;

namespace SistemaCredenciamento.Data;

// Cria conexões MySQL usando a configuração da aplicação.
public sealed class MySqlConnectionFactory
{
    private readonly string _connectionString;

    // Lê a connection string "MySql" e falha explicitamente se ela não estiver definida.
    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "A connection string 'MySql' is missing. Configure it with .NET User Secrets.");
    }

    // Retorna uma conexão nova, ainda não aberta, para o repositório.
    public MySqlConnection CreateConnection() => new(_connectionString);
}
