using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backendapi.Data;
using backendapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backendapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Credenciales inválidas" });

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.username == request.Username);
        
        if (user == null || !VerifyPassword(request.Password, user.passwordhash))
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

        if (!user.activo)
            return StatusCode(403, new { message = "El usuario no está activo" });

        var token = GenerateToken(user);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only for HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        };
        
        Response.Cookies.Append("AuthToken", token, cookieOptions);

        return Ok(new { username = user.username, nombre = user.nombre, message = "Login exitoso" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        return Ok(new { message = "Sesión cerrada correctamente" });
    }

    [Authorize]
    [HttpGet("perfil")]
    public async Task<IActionResult> Perfil()
    {
        var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (username == null) return Unauthorized();

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.username == username);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.iduser,
            user.nombre,
            user.apellido,
            user.correo,
            user.fecha_nacimiento,
            user.activo,
            user.username
        });
    }

    private string GenerateToken(Usuario user)
    {
        var secretKey = _config["JwtKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? "SuperSecretKeyQueDeberiaEstarEnElEnvXD12345!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.username),
            new Claim(ClaimTypes.Name, user.nombre)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtIssuer"],
            audience: _config["JwtAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private bool VerifyPassword(string password, string storedHash)
    {
        // For simplicity using BCrypt
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
