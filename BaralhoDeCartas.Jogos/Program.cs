using BaralhoDeCartas.Api;
using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Services;
using BaralhoDeCartas.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Adicionar configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowScalar", policy =>
    {
        policy.WithOrigins("https://localhost:*")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Scalar para forçar HTTPS
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

// Configurar HttpClient
builder.Services.AddHttpClient<IBaralhoApiClient, BaralhoApiClient>();

// Registrar serviços
builder.Services.AddScoped<IMaiorCartaService, MaiorCartaService>();
builder.Services.AddScoped<IBlackjackService, BlackjackService>();
builder.Services.AddScoped<IJogadorFactory, JogadorFactory>();
builder.Services.AddScoped<ICartaFactory, CartaFactory>();
builder.Services.AddScoped<IBaralhoFactory, BaralhoFactory>();
builder.Services.AddScoped<IJogoFactory, JogoFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // UseHttpsRedirection apenas em produção
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowScalar");

app.MapScalarApiReference();
app.MapOpenApi();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
