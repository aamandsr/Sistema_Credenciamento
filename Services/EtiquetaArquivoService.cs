using System.Text;

namespace SistemaCredenciamento.Services;

public sealed class EtiquetaArquivoService
{
    public Task AdicionarImpressaoAsync(
        string nomeCracha,
        CancellationToken cancellationToken = default)
    {
        var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(desktopDirectory))
        {
            throw new InvalidOperationException(
                "Não foi possível localizar a Área de Trabalho do usuário que executa a aplicação.");
        }

        var nomeEmUmaLinha = nomeCracha
            .Replace('\r', ' ')
            .Replace('\n', ' ');
        var registro = $"{nomeEmUmaLinha} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}{Environment.NewLine}";
        var caminhoArquivo = Path.Combine(desktopDirectory, "etiquetas.txt");

        return File.AppendAllTextAsync(
            caminhoArquivo,
            registro,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }
}
