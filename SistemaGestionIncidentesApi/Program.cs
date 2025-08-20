using FinancieraAPI.Data;
using SistemaGestionIncidentesApi.Data;
using SistemaGestionIncidentesApi.Data.Contrato;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// INYECTAR DEPENDENCIAS
builder.Services.AddScoped<IEstadoIncidente, EstadoIncidenteRepositorio>();
builder.Services.AddScoped<Iincidente, IncidenteRespositorio>();
builder.Services.AddScoped<IUsuario, UsuarioRepositorio>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
