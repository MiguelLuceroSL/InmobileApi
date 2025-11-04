using InmobileApi.Data;
using InmobileApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InmobileApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PropietariosController : ControllerBase
    {
        private readonly DataContext _context;

        public PropietariosController(DataContext context)
        {
            _context = context;
        }

        // /api/Propietarios (perfil del propietario autenticado)
        [HttpGet]
        public async Task<ActionResult<Propietario>> GetPerfil()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name);
                var propietario = await _context.Propietarios.FirstOrDefaultAsync(p => p.Email == email);

                if (propietario == null)
                    return NotFound("Propietario no encontrado");

                return Ok(propietario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // /api/Propietarios/actualizar
        [Authorize]
        [HttpPut("actualizar")]
        public async Task<ActionResult> Actualizar([FromBody] PropietarioUpdateDto datos)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name);
                var propietario = await _context.Propietarios.FirstOrDefaultAsync(p => p.Email == email);

                if (propietario == null)
                    return NotFound("Propietario no encontrado");

                propietario.Nombre = datos.Nombre;
                propietario.Apellido = datos.Apellido;
                propietario.Dni = datos.Dni;
                propietario.Telefono = datos.Telefono;
                propietario.Email = datos.Email;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Perfil actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // /api/Propietarios/cambiarClave
        [HttpPut("cambiarClave")]
        public async Task<ActionResult> CambiarClave([FromForm] string claveActual, [FromForm] string nuevaClave)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Name);
                var propietario = await _context.Propietarios.FirstOrDefaultAsync(p => p.Email == email);

                if (propietario == null)
                    return NotFound("Propietario no encontrado");

                if (!BCrypt.Net.BCrypt.Verify(claveActual, propietario.Clave))
                    return BadRequest("La contraseña actual es incorrecta.");

                propietario.Clave = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
                await _context.SaveChangesAsync();

                return Ok("Contraseña actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}