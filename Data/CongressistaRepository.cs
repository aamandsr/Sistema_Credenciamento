using System.Data.Common;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Data;

public sealed class CongressistaRepository(MySqlConnectionFactory connectionFactory)
{
    public async Task<Congressista?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, nome_cracha, tipo, pagamento, material_impresso, empresa
            FROM congressista
            WHERE id = @id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Congressista
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Nome = reader.GetString(reader.GetOrdinal("nome")),
            Nome_Cracha = reader.GetString(reader.GetOrdinal("nome_cracha")),
            Tipo = reader.GetString(reader.GetOrdinal("tipo")),
            Pagamento = reader.GetString(reader.GetOrdinal("pagamento")),
            Material_Impresso = reader.GetBoolean(reader.GetOrdinal("material_impresso")),
            Empresa = reader.IsDBNull(reader.GetOrdinal("empresa"))
                ? null
                : reader.GetString(reader.GetOrdinal("empresa"))
        };
    }

    public async Task<IReadOnlyList<Congressista>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, nome_cracha, tipo, pagamento, material_impresso, empresa
            FROM congressista
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
            INSERT INTO congressista
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

    public async Task<int> AtualizarAsync(
        Congressista congressista,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE congressista
            SET nome = @nome,
                nome_cracha = @nomeCracha,
                tipo = @tipo,
                pagamento = @pagamento,
                material_impresso = @materialImpresso,
                empresa = @empresa
            WHERE id = @id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@id", congressista.Id);
        AddParameter(command, "@nome", congressista.Nome);
        AddParameter(command, "@nomeCracha", congressista.Nome_Cracha);
        AddParameter(command, "@tipo", congressista.Tipo);
        AddParameter(command, "@pagamento", congressista.Pagamento);
        AddParameter(command, "@materialImpresso", congressista.Material_Impresso);
        AddParameter(command, "@empresa", congressista.Empresa);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM congressista WHERE id = @id;";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
