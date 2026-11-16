using System.Text;

namespace SistemaCredenciamento.Services;

// Acrescenta linhas de impressão ao arquivo local de etiquetas.
public sealed class EtiquetaArquivoService
{
    // Registra o nome no arquivo etiquetas.txt localizado na Área de Trabalho do processo.
    public Task AdicionarImpressaoAsync(
        string nomeCracha,
        CancellationToken cancellationToken = default)
    {
        // O caminho é relativo ao usuário que está executando o servidor web.
        var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktopDirectory))
        {
            throw new InvalidOperationException(
                "Não foi possível localizar a Área de Trabalho do usuário que executa a aplicação.");
        }

        // Mantém cada etiqueta em uma única linha e registra a data e hora da operação.
        var nomeEmUmaLinha = nomeCracha
            .Replace('\r', ' ')
            .Replace('\n', ' ');
        var registro = $"{nomeEmUmaLinha} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}{Environment.NewLine}";
        var caminhoArquivo = Path.Combine(desktopDirectory, "etiquetas.txt");

        // Usa UTF-8 sem BOM e permite cancelar a escrita junto com a requisição.
        return File.AppendAllTextAsync(
            caminhoArquivo,
            registro,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }
}
