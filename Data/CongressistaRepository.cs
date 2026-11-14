using System.Data.Common;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Data;

public sealed class CongressistaRepository(MySqlConnectionFactory connectionFactory)
{
    public async Task<IReadOnlyList<Congressista>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, nome_cracha, tipo, pagamento, material_impresso, empresa
            FROM congressistas
            ORDER BY id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("id");
        var nomeOrdinal = reader.GetOrdinal("nome");
        var nomeCrachaOrdinal = reader.GetOrdinal("nome_cracha");
        var tipoOrdinal = reader.GetOrdinal("tipo");
        var pagamentoOrdinal = reader.GetOrdinal("pagamento");
        var materialImpressoOrdinal = reader.GetOrdinal("material_impresso");
        var empresaOrdinal = reader.GetOrdinal("empresa");

        var congressistas = new List<Congressista>();
        while (await reader.ReadAsync(cancellationToken))
        {
            congressistas.Add(new Congressista
            {
                Id = reader.GetInt32(idOrdinal),
                Nome = reader.GetString(nomeOrdinal),
                Nome_Cracha = reader.GetString(nomeCrachaOrdinal),
                Tipo = reader.GetString(tipoOrdinal),
                Pagamento = reader.GetString(pagamentoOrdinal),
                Material_Impresso = reader.GetBoolean(materialImpressoOrdinal),
                Empresa = reader.IsDBNull(empresaOrdinal)
                    ? null
                    : reader.GetString(empresaOrdinal)
            });
        }

        return congressistas;
    }

    public async Task<int> AdicionarAsync(
        Congressista congressista,
        CancellationToken cancellationToken = default)
    {
        const string insertSql = """
            INSERT INTO congressistas
                (nome, nome_cracha, tipo, pagamento, material_impresso, empresa)
            VALUES
                (@nome, @nomeCracha, @tipo, @pagamento, @materialImpresso, @empresa);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.CommandText = insertSql;
            AddParameter(insertCommand, "@nome", congressista.Nome);
            AddParameter(insertCommand, "@nomeCracha", congressista.Nome_Cracha);
            AddParameter(insertCommand, "@tipo", congressista.Tipo);
            AddParameter(insertCommand, "@pagamento", congressista.Pagamento);
            AddParameter(insertCommand, "@materialImpresso", congressista.Material_Impresso);
            AddParameter(insertCommand, "@empresa", congressista.Empresa);
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT LAST_INSERT_ID();";
        var id = await idCommand.ExecuteScalarAsync(cancellationToken);
        if (id is null or DBNull)
        {
            throw new InvalidOperationException("The database did not return the new congressista ID.");
        }

        return Convert.ToInt32(id);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
