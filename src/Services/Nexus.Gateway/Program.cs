using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuração do Ocelot ---
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// --- 2. Serviços do Swagger e MVC ---
// Necessários para a página de documentação rodar
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 3. Ativa Arquivos Estáticos ---
// Permite carregar o CSS e JS da página do Swagger
app.UseStaticFiles();

// --- 4. Ativa o Gerador do Swagger ---
// (Faltava esta linha no seu código!)
app.UseSwagger(); 

// --- 5. Configura a Interface Visual (Menu Dropdown) ---
app.UseSwaggerUI(options =>
{
    // Apontando para os "apelidos" /docs/ que criamos no ocelot.json
    options.SwaggerEndpoint("/docs/identity.json", "1. Identity (Autenticação)");
    options.SwaggerEndpoint("/docs/stock.json", "2. Stock (Estoque)");
    options.SwaggerEndpoint("/docs/sales.json", "3. Sales (Vendas)");
    
    // Define que o Swagger abre em /swagger
    options.RoutePrefix = "swagger"; 
});

// --- 6. Ocelot entra por último (O Porteiro) ---
await app.UseOcelot();

app.Run();