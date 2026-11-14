using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Controllers;

public sealed class CongressistaController(CongressistaRepository repository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var congressistas = await repository.ListarAsync(cancellationToken);
        return View(congressistas);
    }

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
        return RedirectToAction(nameof(Index));
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

        return RedirectToAction(nameof(Index));
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

        return RedirectToAction(nameof(Index));
    }
}
