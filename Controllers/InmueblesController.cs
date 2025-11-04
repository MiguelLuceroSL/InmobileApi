using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobileApi.Models;
using System.Text.Json;
using InmobileApi.Data;
using Microsoft.EntityFrameworkCore;

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
            //deserializar el json recibido
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var nuevoInmueble = JsonSerializer.Deserialize<Inmueble>(inmueble, options);

            if (nuevoInmueble == null)
                return BadRequest("No se pudo deserializar el inmueble.");

            //obtener id del propietario del token
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);
            nuevoInmueble.PropietarioId = idProp;

            //guardar imagen si existe
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

            //guardar en db
            _context.Inmuebles.Add(nuevoInmueble);
            await _context.SaveChangesAsync();

            return Ok(nuevoInmueble);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("misInmuebles")]
    public async Task<IActionResult> GetMisInmuebles()
    {
        try
        {
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

            var inmuebles = await _context.Inmuebles
                .Where(i => i.PropietarioId == idProp)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var resultado = inmuebles.Select(i => new
            {
                i.IdInmueble,
                i.Direccion,
                i.Uso,
                i.Tipo,
                i.Ambientes,
                i.Superficie,
                i.Latitud,
                i.Longitud,
                i.Valor,
                ImagenUrl = string.IsNullOrEmpty(i.ImagenRuta) ? null : $"{baseUrl}/{i.ImagenRuta}",
                i.Disponible,
                i.PropietarioId,
                i.TieneContratoVigente
            });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPut("actualizar")]
    public async Task<IActionResult> Actualizar([FromBody] Inmueble datos)
    {
        try
        {
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.IdInmueble == datos.IdInmueble && i.PropietarioId == idProp);

            if (inmueble == null)
                return NotFound("Inmueble no encontrado o no pertenece al propietario.");

            inmueble.Direccion = datos.Direccion;
            inmueble.Uso = datos.Uso;
            inmueble.Tipo = datos.Tipo;
            inmueble.Ambientes = datos.Ambientes;
            inmueble.Superficie = datos.Superficie;
            inmueble.Latitud = datos.Latitud;
            inmueble.Longitud = datos.Longitud;
            inmueble.Valor = datos.Valor;
            inmueble.Disponible = datos.Disponible;
            inmueble.TieneContratoVigente = datos.TieneContratoVigente;

            await _context.SaveChangesAsync();

            return Ok(inmueble);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("GetInmueblesConContratoVigente")]
    public async Task<IActionResult> GetInmueblesConContratoVigente()
    {
        try
        {
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

            var inmuebles = await _context.Inmuebles
                .Where(i => i.PropietarioId == idProp && i.TieneContratoVigente)
                .ToListAsync();

            if (!inmuebles.Any())
                return NotFound("No hay inmuebles con contrato vigente.");

            return Ok(inmuebles);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}