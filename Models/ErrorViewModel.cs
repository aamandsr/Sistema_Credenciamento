namespace SistemaCredenciamento.Models;

// Dados apresentados pela página de erro da aplicação.
public class ErrorViewModel
{
    // Identificador útil para localizar a requisição nos logs do servidor.
    public string? RequestId { get; set; }

    // A view só exibe o identificador quando ele está disponível.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
