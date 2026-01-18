using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using Nexus.Stock.API.Infra.Data;
using Serilog;
using System.Text;
using Nexus.Stock.API.Application.Consumers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. Logs (Serilog) - Lê do appsettings
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger (Configurado com Cadeado de Segurança)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Nexus Stock API", 
        Version = "v1",
        Description = "API de Gestão de Estoque - Nexus Commerce"
    });

    // --- Configuração da Segurança (O Cadeado) ---
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT assim: Bearer {seu_token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 3. Banco de Dados (SQL SERVER)
builder.Services.AddDbContext<StockDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Segurança (JWT)
var secretKey = builder.Configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("Chave JWT não encontrada nas configurações.");

var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, 
        ValidateAudience = false 
    };
});

// 5. RabbitMQ (MassTransit)
builder.Services.AddMassTransit(x => {
    x.AddConsumer<StockUpdateConsumer>();
    
    x.UsingRabbitMq((context, cfg) => {
        var rabbitConn = builder.Configuration.GetConnectionString("RabbitMq");
        
        cfg.Host(rabbitConn);
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// 6. Migração Automática do Banco
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<StockDbContext>();
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
        else
        {
            db.Database.EnsureCreated();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Ocorreu um erro ao migrar o banco de dados.");
    }
}

// Configuração do Pipeline HTTP
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthentication(); // Quem é você?
app.UseAuthorization();  // O que você pode fazer?

app.MapControllers();

app.Run();