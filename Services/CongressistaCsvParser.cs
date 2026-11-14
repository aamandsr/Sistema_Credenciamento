using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Services;

public sealed class CongressistaCsvParser
{
    public IReadOnlyList<Congressista> Parse(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            throw new CongressistaCsvException("O arquivo CSV está vazio.");
        }

        conteudo = conteudo.TrimStart('\uFEFF');
        var primeiraLinha = conteudo.Split(new[] { '\r', '\n' }, 2)[0];
        var delimitador = primeiraLinha.Count(caractere => caractere == ';') >=
            primeiraLinha.Count(caractere => caractere == ',')
            ? ";"
            : ",";

        using var parser = new TextFieldParser(new StringReader(conteudo));
        parser.TextFieldType = FieldType.Delimited;
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = false;
        parser.SetDelimiters(delimitador);

        string[]? cabecalhos;
        try
        {
            cabecalhos = parser.ReadFields();
        }
        catch (MalformedLineException exception)
        {
            throw new CongressistaCsvException("O cabeçalho do CSV está malformado.", exception);
        }

        if (cabecalhos is null || cabecalhos.Length == 0)
        {
            throw new CongressistaCsvException("O CSV precisa ter uma linha de cabeçalho.");
        }

        var colunas = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var indice = 0; indice < cabecalhos.Length; indice++)
        {
            var coluna = Normalizar(cabecalhos[indice]);
            if (coluna is not ("nome" or "nomecracha" or "tipo" or "pagamento" or "empresa"))
            {
                throw new CongressistaCsvException(
                    $"A coluna '{cabecalhos[indice]}' não é reconhecida.");
            }

            if (!colunas.TryAdd(coluna, indice))
            {
                throw new CongressistaCsvException(
                    $"A coluna '{cabecalhos[indice]}' está repetida no cabeçalho.");
            }
        }

        var colunasObrigatorias = new[] { "nome", "nomecracha", "tipo", "pagamento" };
        var ausentes = colunasObrigatorias.Where(coluna => !colunas.ContainsKey(coluna)).ToArray();
        if (ausentes.Length > 0)
        {
            throw new CongressistaCsvException(
                $"Colunas obrigatórias ausentes: {string.Join(", ", ausentes)}.");
        }

        var congressistas = new List<Congressista>();
        var numeroRegistro = 1;
        while (!parser.EndOfData)
        {
            string[]? campos;
            try
            {
                campos = parser.ReadFields();
            }
            catch (MalformedLineException exception)
            {
                throw new CongressistaCsvException(
                    $"O registro CSV próximo à linha {exception.LineNumber} está malformado.",
                    exception);
            }

            numeroRegistro++;
            if (campos is null || campos.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            if (campos.Length != cabecalhos.Length)
            {
                throw new CongressistaCsvException(
                    $"O registro {numeroRegistro} tem {campos.Length} campo(s); eram esperados {cabecalhos.Length}.");
            }

            var nome = ObterCampo(campos, colunas, "nome");
            var nomeCracha = ObterCampo(campos, colunas, "nomecracha");
            var tipo = ObterCampo(campos, colunas, "tipo");
            var pagamento = ObterCampo(campos, colunas, "pagamento");
            var empresa = colunas.ContainsKey("empresa")
                ? ObterCampo(campos, colunas, "empresa")
                : string.Empty;

            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(nomeCracha))
            {
                throw new CongressistaCsvException(
                    $"O registro {numeroRegistro} precisa ter nome e nome_cracha.");
            }

            var tipoCadastrado = Normalizar(tipo) switch
            {
                "congressista" => "Congressista",
                "palestrante" => "Palestrante",
                _ => throw new CongressistaCsvException(
                    $"Tipo inválido no registro {numeroRegistro}. Use Congressista ou Palestrante.")
            };

            var pagamentoCadastrado = Normalizar(pagamento) switch
            {
                "sim" => "Sim",
                "nao" => "Não",
                "cortesia" => "Cortesia",
                _ => throw new CongressistaCsvException(
                    $"Pagamento inválido no registro {numeroRegistro}. Use Sim, Não ou Cortesia.")
            };

            congressistas.Add(new Congressista
            {
                Nome = nome,
                Nome_Cracha = nomeCracha,
                Tipo = tipoCadastrado,
                Pagamento = pagamentoCadastrado,
                Material_Impresso = false,
                Empresa = string.IsNullOrWhiteSpace(empresa) ? null : empresa
            });
        }

        if (congressistas.Count == 0)
        {
            throw new CongressistaCsvException("O CSV não contém congressistas para cadastrar.");
        }

        return congressistas;
    }

    private static string ObterCampo(
        IReadOnlyList<string> campos,
        IReadOnlyDictionary<string, int> colunas,
        string coluna) =>
        campos[colunas[coluna]].Trim();

    private static string Normalizar(string valor)
    {
        var semAcentos = valor.Trim().Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(semAcentos.Length);

        foreach (var caractere in semAcentos)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(char.ToLowerInvariant(caractere));
            }
        }

        return resultado.ToString()
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}

public sealed class CongressistaCsvException(string message, Exception? innerException = null)
    : Exception(message, innerException);
