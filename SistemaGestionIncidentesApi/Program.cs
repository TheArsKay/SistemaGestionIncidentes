using SistemaGestionIncidentesApi.Data;
using SistemaGestionIncidentesApi.Data.Contrato;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// === CORS: política para permitir que tu WebApp (https://localhost:7241) consuma la API ===
var corsPolicyName = "AllowWebApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy
            .WithOrigins("https://localhost:7241") // <- ORIGEN del WebApp (ajusta puerto si necesitas)
            .AllowAnyHeader()
            .AllowAnyMethod();
        // .AllowCredentials(); // Descomenta sólo si envías cookies/credenciales desde el front y entonces
        // recuerda usar WithOrigins (no AllowAnyOrigin)
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// INYECTAR DEPENDENCIAS
builder.Services.AddScoped<Iincidente, IncidenteRepositorio>();
builder.Services.AddScoped<IEstadoIncidente, EstadoIncidenteRepositorio>();
builder.Services.AddScoped<INotificacionRepositorio, NotificacionRepositorio>();
builder.Services.AddScoped<IUsuario, UsuarioRepositorio>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: UseCors debe ir antes de UseAuthorization y antes de MapControllers si usas middleware por rutas
app.UseCors(corsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();
