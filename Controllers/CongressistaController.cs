using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Models;
using SistemaCredenciamento.Services;

namespace SistemaCredenciamento.Controllers;

// Recebe as ações de cadastro, importação, edição e exclusão de congressistas.
public sealed class CongressistaController(
    CongressistaRepository repository,
    CongressistaCsvParser csvParser) : Controller
{
    // Mantém a rota antiga de congressistas apontando para a tela principal.
    public IActionResult Index() => RedirectToAction("Index", "Home");

    // Exibe o formulário de criação vazio.
    [HttpGet]
    public IActionResult Criar() => View(new Congressista());

    // Valida e salva o congressista enviado pelo formulário.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        Congressista congressista,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reexibe os dados e os erros de validação sem gravar no banco.
            return View(congressista);
        }

        await repository.AdicionarAsync(congressista, cancellationToken);
        return RedirectToAction("Index", "Home");
    }

    // Exibe as instruções e o seletor de arquivo CSV.
    [HttpGet]
    public IActionResult ImportarCsv() => View();

    // Valida, interpreta e importa em lote os registros do arquivo CSV.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> ImportarCsv(
        IFormFile? arquivo,
        CancellationToken cancellationToken)
    {
        // Valida o arquivo antes de ler seu conteúdo.
        if (arquivo is null || arquivo.Length == 0)
        {
            ModelState.AddModelError(nameof(arquivo), "Selecione um arquivo CSV.");
            return View();
        }

        if (!Path.GetExtension(arquivo.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(arquivo), "O arquivo selecionado deve ter a extensão .csv.");
            return View();
        }

        if (arquivo.Length > 5_242_880)
        {
            ModelState.AddModelError(nameof(arquivo), "O arquivo CSV deve ter no máximo 5 MB.");
            return View();
        }

        // Decodifica UTF-8 e reconhece BOM, se o arquivo possuir essa marca.
        string conteudo;
        using (var leitor = new StreamReader(
            arquivo.OpenReadStream(),
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true))
        {
            conteudo = await leitor.ReadToEndAsync(cancellationToken);
        }

        // O parser também valida o cabeçalho, os campos e os valores aceitos.
        IReadOnlyList<Congressista> congressistas;
        try
        {
            congressistas = csvParser.Parse(conteudo);
        }
        catch (CongressistaCsvException exception)
        {
            ModelState.AddModelError(nameof(arquivo), exception.Message);
            return View();
        }

        // Só persiste depois que todo o arquivo foi interpretado corretamente.
        await repository.AdicionarVariosAsync(congressistas, cancellationToken);
        TempData["Mensagem"] = $"{congressistas.Count} congressista(s) cadastrado(s) pelo CSV.";
        return RedirectToAction("Index", "Home");
    }

    // Busca e apresenta os dados atuais para edição.
    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
    {
        var congressista = await repository.ObterPorIdAsync(id, cancellationToken);
        return congressista is null ? NotFound() : View(congressista);
    }

    // Valida e atualiza os dados enviados para o congressista identificado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        Congressista congressista,
        CancellationToken cancellationToken)
    {
        if (id != congressista.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(congressista);
        }

        // Uma atualização pode afetar zero linhas se os dados não mudaram.
        // Confere se o registro existe antes de decidir entre sucesso e 404.
        var linhasAfetadas = await repository.AtualizarAsync(congressista, cancellationToken);
        if (linhasAfetadas == 0 &&
            await repository.ObterPorIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        return RedirectToAction("Index", "Home");
    }

    // Apresenta uma confirmação antes de remover um registro.
    [HttpGet]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var congressista = await repository.ObterPorIdAsync(id, cancellationToken);
        return congressista is null ? NotFound() : View(congressista);
    }

    // Remove o registro após a confirmação enviada por POST.
    [HttpPost]
    [ActionName(nameof(Excluir))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarExclusao(
        int id,
        CancellationToken cancellationToken)
    {
        var linhasAfetadas = await repository.ExcluirAsync(id, cancellationToken);
        if (linhasAfetadas == 0)
        {
            // Nenhuma linha removida significa que o identificador não existe.
            return NotFound();
        }

        return RedirectToAction("Index", "Home");
    }
}
