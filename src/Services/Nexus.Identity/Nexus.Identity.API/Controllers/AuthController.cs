using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Nexus.Identity.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    // Injetamos o Gerenciador de Usuários (Banco) e as Configurações (appsettings)
    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // 1. Endpoint de Registro (Cria usuário no Banco SQL)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Cria um objeto de usuário padrão do Identity
        var user = new IdentityUser 
        { 
            UserName = request.Email, 
            Email = request.Email 
        };

        // Tenta salvar no banco com a senha criptografada
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            return Ok(new { message = "Usuário criado com sucesso!" });
        }

        // Se der erro (ex: senha fraca, email repetido), retorna o motivo
        return BadRequest(result.Errors);
    }

    // 2. Endpoint de Login (Verifica no Banco SQL)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Busca o usuário pelo Email
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Verifica se existe e se a senha bate (o Identity faz o hash automático)
        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Email ou senha inválidos" });
    }

    // 3. Gerador de Token (Lê a chave do appsettings.json)
    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        // Pega a chave do arquivo de configuração (Segurança real)
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("Chave JWT não configurada."));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Role, "Admin") // Simplificação: Todo mundo é Admin por enquanto
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// DTOs atualizados para usar Email
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}