using SistemaCredenciamento.Data;
using SistemaCredenciamento.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<MySqlConnectionFactory>();
builder.Services.AddScoped<CongressistaRepository>();
builder.Services.AddSingleton<EtiquetaArquivoService>();
builder.Services.AddSingleton<CongressistaCsvParser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
