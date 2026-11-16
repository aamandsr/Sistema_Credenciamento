using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Controllers;

// Carrega a listagem principal e disponibiliza a tela de erro.
public class HomeController(CongressistaRepository repository) : Controller
{
    // Busca todos os congressistas e os envia para a view inicial.
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var congressistas = await repository.ListarAsync(cancellationToken);
        return View(congressistas);
    }

    // Mostra um identificador de requisição sem armazenar a resposta em cache.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
