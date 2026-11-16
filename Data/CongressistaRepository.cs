using System.Data.Common;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Data;

// Executa operações de leitura e escrita de congressistas no MySQL.
public sealed class CongressistaRepository(MySqlConnectionFactory connectionFactory)
{
    // Busca pelo ID ou retorna null quando o cadastro não existe.
    public async Task<Congressista?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, nome_cracha, tipo, pagamento, material_impresso, empresa
            FROM congressista
            WHERE id = @id;
            """;

        // Cada operação abre sua conexão e libera os recursos ao terminar.
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        // Mantém o valor fora do texto SQL.
        AddParameter(command, "@id", id);

        // Converte a linha encontrada para o modelo usado pelas views.
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

    // Lista todos os cadastros em ordem de identificador.
    public async Task<IReadOnlyList<Congressista>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, nome, nome_cracha, tipo, pagamento, material_impresso, empresa
            FROM congressista
            ORDER BY id;
            """;

        // Abre a conexão e executa a consulta de forma assíncrona.
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Resolve os índices uma vez para reutilizar na leitura de cada linha.
        var idOrdinal = reader.GetOrdinal("id");
        var nomeOrdinal = reader.GetOrdinal("nome");
        var nomeCrachaOrdinal = reader.GetOrdinal("nome_cracha");
        var tipoOrdinal = reader.GetOrdinal("tipo");
        var pagamentoOrdinal = reader.GetOrdinal("pagamento");
        var materialImpressoOrdinal = reader.GetOrdinal("material_impresso");
        var empresaOrdinal = reader.GetOrdinal("empresa");

        // Materializa os resultados antes de fechar o leitor e a conexão.
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

    // Insere um congressista e retorna o identificador gerado pelo MySQL.
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

        // Mantém a mesma conexão para inserir o registro e ler seu ID.
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

        // LAST_INSERT_ID retorna a chave criada nesta conexão.
        await using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT LAST_INSERT_ID();";
        var id = await idCommand.ExecuteScalarAsync(cancellationToken);
        if (id is null or DBNull)
        {
            throw new InvalidOperationException("The database did not return the new congressista ID.");
        }

        return Convert.ToInt32(id);
    }

    // Insere todos os registros do lote dentro de uma única transação.
    public async Task AdicionarVariosAsync(
        IReadOnlyCollection<Congressista> congressistas,
        CancellationToken cancellationToken = default)
    {
        // Uma lista vazia não representa uma importação válida.
        if (congressistas.Count == 0)
        {
            throw new ArgumentException("At least one congressista is required.", nameof(congressistas));
        }

        const string sql = """
            INSERT INTO congressista
                (nome, nome_cracha, tipo, pagamento, material_impresso, empresa)
            VALUES
                (@nome, @nomeCracha, @tipo, @pagamento, @materialImpresso, @empresa);
            """;

        // A transação garante que o lote seja gravado por completo ou descartado.
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var congressista in congressistas)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            AddParameter(command, "@nome", congressista.Nome);
            AddParameter(command, "@nomeCracha", congressista.Nome_Cracha);
            AddParameter(command, "@tipo", congressista.Tipo);
            AddParameter(command, "@pagamento", congressista.Pagamento);
            AddParameter(command, "@materialImpresso", false);
            AddParameter(command, "@empresa", congressista.Empresa);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    // Atualiza os campos do cadastro e retorna a quantidade de linhas afetadas.
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

        // O ID seleciona o registro e os demais parâmetros fornecem seus valores atualizados.
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

    // Marca como impressa somente uma etiqueta que ainda está pendente.
    public async Task<int> MarcarMaterialImpressoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE congressista
            SET material_impresso = TRUE
            WHERE id = @id AND material_impresso = FALSE;
            """;

        // A condição protege o estado contra uma segunda marcação concorrente.
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Remove um cadastro pelo ID e retorna a quantidade de linhas removidas.
    public async Task<int> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM congressista WHERE id = @id;";

        // O total afetado permite ao controller identificar IDs inexistentes.
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Cria um parâmetro de comando e converte valores nulos para DBNull.
    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
