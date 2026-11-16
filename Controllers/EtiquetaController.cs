using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Services;

namespace SistemaCredenciamento.Controllers;

// Controla a validação e o registro da impressão de etiquetas.
public sealed class EtiquetaController(
    CongressistaRepository repository,
    EtiquetaArquivoService arquivoService,
    ILogger<EtiquetaController> logger) : Controller
{
    // Mantém a rota antiga de etiquetas apontando para a tela principal.
    public IActionResult Index() => RedirectToAction("Index", "Home");

    // Registra uma impressão quando as regras do congressista permitem.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Imprimir(int id, CancellationToken cancellationToken)
    {
        var congressista = await repository.ObterPorIdAsync(id, cancellationToken);
        if (congressista is null)
        {
            return NotFound();
        }

        // Pagamento pendente bloqueia a impressão, mas cortesia é permitida.
        if (string.Equals(
                congressista.Pagamento.Trim(),
                "Não",
                StringComparison.OrdinalIgnoreCase))
        {
            TempData["Erro"] = $"Pagamento pendente para {congressista.Nome}. A etiqueta não foi impressa.";
            return RedirectToAction("Index", "Home");
        }

        // Evita duplicar uma etiqueta já marcada como impressa.
        if (congressista.Material_Impresso)
        {
            TempData["Mensagem"] = $"A etiqueta de {congressista.Nome_Cracha} já foi impressa.";
            return RedirectToAction("Index", "Home");
        }

        // O nome do crachá é o texto que será enviado ao arquivo de impressão.
        if (string.IsNullOrWhiteSpace(congressista.Nome_Cracha))
        {
            TempData["Erro"] = "Preencha o nome no crachá antes de imprimir a etiqueta.";
            return RedirectToAction("Index", "Home");
        }

        // Grava a saída primeiro e converte falhas conhecidas em mensagens ao usuário.
        try
        {
            await arquivoService.AdicionarImpressaoAsync(
                congressista.Nome_Cracha,
                cancellationToken);
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "Não foi possível gravar a etiqueta do congressista {Id}.", id);
            TempData["Erro"] = "Não foi possível gravar a etiqueta na Área de Trabalho.";
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Sem permissão para gravar a etiqueta do congressista {Id}.", id);
            TempData["Erro"] = "Sem permissão para gravar etiquetas na Área de Trabalho.";
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Não foi possível localizar a Área de Trabalho para a etiqueta do congressista {Id}.", id);
            TempData["Erro"] = "Não foi possível localizar a Área de Trabalho do computador que executa a aplicação.";
            return RedirectToAction("Index", "Home");
        }

        // A operação condicional também protege contra duas impressões concorrentes.
        var linhasAfetadas = await repository.MarcarMaterialImpressoAsync(id, cancellationToken);
        if (linhasAfetadas == 0)
        {
            var estadoAtual = await repository.ObterPorIdAsync(id, cancellationToken);
            if (estadoAtual is null)
            {
                return NotFound();
            }

            TempData["Mensagem"] = $"A etiqueta de {estadoAtual.Nome_Cracha} já foi impressa.";
            return RedirectToAction("Index", "Home");
        }

        // TempData mantém o aviso disponível depois do redirecionamento.
        TempData["Mensagem"] = $"Etiqueta de {congressista.Nome_Cracha} adicionada a etiquetas.txt.";
        return RedirectToAction("Index", "Home");
    }
}
