using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobileApi.Models;
using System.Text.Json;
using InmobileApi.Data;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InmueblesController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public InmueblesController(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("cargar")]
    public async Task<IActionResult> Cargar([FromForm] IFormFile imagen, [FromForm] string inmueble)
    {
        try
        {
            // 1️⃣ Deserializar el JSON recibido
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var nuevoInmueble = JsonSerializer.Deserialize<Inmueble>(inmueble, options);

            if (nuevoInmueble == null)
                return BadRequest("No se pudo deserializar el inmueble.");

            // 2️⃣ Obtener id del propietario del token
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);
            nuevoInmueble.PropietarioId = idProp;

            // 3️⃣ Guardar imagen si existe
            if (imagen != null)
            {
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                string rutaCarpeta = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }
                nuevoInmueble.ImagenRuta = Path.Combine("uploads", nombreArchivo).Replace("\\", "/");
            }

            // 4️⃣ Guardar en base de datos
            _context.Inmuebles.Add(nuevoInmueble);
            await _context.SaveChangesAsync();

            return Ok(nuevoInmueble);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}