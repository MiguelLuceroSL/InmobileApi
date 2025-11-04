using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InmobileApi.Data;
using InmobileApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InmobileApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public AuthController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // /api/Auth/signup
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromForm] Propietario model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existe = await _context.Propietarios.AnyAsync(p => p.Email == model.Email);
                if (existe)
                    return BadRequest("Ya existe un propietario con ese email");

                //hash de la clave con BCrypt
                model.Clave = BCrypt.Net.BCrypt.HashPassword(model.Clave);

                await _context.Propietarios.AddAsync(model);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Signup), new
                {
                    model.IdPropietario,
                    model.Nombre,
                    model.Apellido,
                    model.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // /api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] LoginView loginView)
        {
            try
            {
                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(x => x.Email == loginView.Usuario);

                if (propietario == null)
                    return BadRequest("Usuario o contraseña incorrectos");

                //verificar con BCrypt
                bool passwordOk = BCrypt.Net.BCrypt.Verify(loginView.Clave, propietario.Clave);

                if (!passwordOk)
                    return BadRequest("Usuario o contraseña incorrectos");

                //generar token
                var secretKey = _config["TokenAuthentication:SecretKey"];
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, propietario.Email),
            new Claim("FullName", propietario.Nombre + " " + propietario.Apellido),
            new Claim(ClaimTypes.Role, "Propietario"),
            new Claim("id", propietario.IdPropietario.ToString())
        };

                var token = new JwtSecurityToken(
                    issuer: _config["TokenAuthentication:Issuer"],
                    audience: _config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(4),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new { token = tokenString });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}