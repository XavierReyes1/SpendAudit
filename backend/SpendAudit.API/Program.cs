using DotNetEnv;
using SpendAudit.Application;
using SpendAudit.Infrastructure;
using SpendAudit.Application.Interfaces;
using SpendAudit.API;

// Cargar variables del .env
if (File.Exists(".env")) Env.NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

// Registrar capas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar HttpContext y Tenant Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, HttpCurrentTenantService>();

var app = builder.Build();

app.Run();