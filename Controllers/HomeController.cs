using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaCredenciamento.Data;
using SistemaCredenciamento.Models;

namespace SistemaCredenciamento.Controllers;

public class HomeController(CongressistaRepository repository) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var congressistas = await repository.ListarAsync(cancellationToken);
        return View(congressistas);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
