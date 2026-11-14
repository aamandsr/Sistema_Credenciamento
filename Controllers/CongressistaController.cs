using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Models;
using SistemaCredenciamento.Services;

namespace SistemaCredenciamento.Controllers;

public sealed class CongressistaController(
    CongressistaRepository repository,
    CongressistaCsvParser csvParser) : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Home");

    [HttpGet]
    public IActionResult Criar() => View(new Congressista());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        Congressista congressista,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(congressista);
        }

        await repository.AdicionarAsync(congressista, cancellationToken);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ImportarCsv() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> ImportarCsv(
        IFormFile? arquivo,
        CancellationToken cancellationToken)
    {
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

        string conteudo;
        using (var leitor = new StreamReader(
            arquivo.OpenReadStream(),
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true))
        {
            conteudo = await leitor.ReadToEndAsync(cancellationToken);
        }

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

        await repository.AdicionarVariosAsync(congressistas, cancellationToken);
        TempData["Mensagem"] = $"{congressistas.Count} congressista(s) cadastrado(s) pelo CSV.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
    {
        var congressista = await repository.ObterPorIdAsync(id, cancellationToken);
        return congressista is null ? NotFound() : View(congressista);
    }

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

        var linhasAfetadas = await repository.AtualizarAsync(congressista, cancellationToken);
        if (linhasAfetadas == 0 &&
            await repository.ObterPorIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var congressista = await repository.ObterPorIdAsync(id, cancellationToken);
        return congressista is null ? NotFound() : View(congressista);
    }

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
            return NotFound();
        }

        return RedirectToAction("Index", "Home");
    }
}
