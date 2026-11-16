using SistemaCredenciamento.Data;
using SistemaCredenciamento.Services;

var builder = WebApplication.CreateBuilder(args);

// Registra o suporte a controllers e views Razor da aplicação MVC.
builder.Services.AddControllersWithViews();

// A fábrica mantém a configuração da conexão disponível durante toda a aplicação.
builder.Services.AddSingleton<MySqlConnectionFactory>();
// O repositório atende operações de dados durante cada requisição web.
builder.Services.AddScoped<CongressistaRepository>();
// Estes serviços não guardam estado por requisição e podem ser reutilizados.
builder.Services.AddSingleton<EtiquetaArquivoService>();
builder.Services.AddSingleton<CongressistaCsvParser>();

var app = builder.Build();

// Em produção, erros não tratados são encaminhados à página genérica de erro.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Resolve URLs MVC e disponibiliza os arquivos estáticos, como CSS e JavaScript.
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

// Define a rota padrão: página inicial, controller Home e ação Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicia o servidor web e mantém a aplicação atendendo requisições.
app.Run();
