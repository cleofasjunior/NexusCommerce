using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using Nexus.Sales.API.Infra.Data;
using Serilog;
using System.Text;
using Nexus.Sales.API.Application.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. Logs (Serilog) - Configuração profissional via JSON
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger (Agora com CADEADO DE SEGURANÇA)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Nexus Sales API", 
        Version = "v1", 
        Description = "API de Vendas e Pedidos - Nexus Commerce"
    });

    // --- Configuração do Cadeado (Authorize) ---
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
builder.Services.AddDbContext<SalesDbContext>(options => 
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
    x.UsingRabbitMq((context, cfg) => {
        var rabbitConn = builder.Configuration.GetConnectionString("RabbitMq");
        
        cfg.Host(rabbitConn);
        cfg.ConfigureEndpoints(context);
    });
});

// 6. Integração HTTP com Estoque
builder.Services.AddHttpClient<StockIntegrationService>(client =>
{
    // Aponta para o serviço de Estoque (Porta 6001)
    client.BaseAddress = new Uri("http://localhost:6001"); 
});

var app = builder.Build();

// 7. Migração Automática e Segura
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<SalesDbContext>();
        
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
        Log.Error(ex, "Erro crítico ao migrar o banco de dados de Vendas.");
    }
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();