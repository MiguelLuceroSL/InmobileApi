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

    // api/Inmuebles/cargar
    [HttpPost("cargar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Cargar([FromForm] InmuebleForm inmuebleForm)
    {
        try
        {
            //deserializo el json del inmueble (viene dentro del form)
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            //si no se pudo leer el json, corto aca
            var nuevoInmueble = JsonSerializer.Deserialize<Inmueble>(inmuebleForm.Inmueble, options);
            if (nuevoInmueble == null)
                return BadRequest("No se pudo deserializar el inmueble.");

            //saco el id del propietario del token
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);
            nuevoInmueble.PropietarioId = idProp;

            //guardar img siesque existe
            if (inmuebleForm.Imagen != null)
            {
                //le pongo un nombre random al archivo (para no pisar otros)
                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(inmuebleForm.Imagen.FileName);
                string rutaCarpeta = Path.Combine(_environment.WebRootPath, "uploads");

                //si no existe la carpeta, la creo
                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                //guardo la img en esa carpeta
                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await inmuebleForm.Imagen.CopyToAsync(stream);
                }
                //guardo la ruta de la imagen en el objeto
                nuevoInmueble.ImagenRuta = Path.Combine("uploads", nombreArchivo).Replace("\\", "/");
            }

            //lo guardo en la db
            _context.Inmuebles.Add(nuevoInmueble);
            await _context.SaveChangesAsync();

            //busco el propietario y se lo asigno al inmueble
            nuevoInmueble.Propietario = await _context.Propietarios
                .FirstOrDefaultAsync(p => p.IdPropietario == idProp);

            //la img ya se guardó, dejo esto null para no ensuciar el json
            nuevoInmueble.ImagenFile = null;

            if (nuevoInmueble.Propietario != null)
                nuevoInmueble.Propietario.Inmuebles = new List<Inmueble>(); //evito ciclos en el json

            //devuelvo ok con el inmueble nuevo
            return Ok(nuevoInmueble);
        }
        catch (Exception ex)
        {
            //si algo rompe, devuelvo el error
            return BadRequest(ex.Message);
        }
    }

    // api/Inmuebles/misInmuebles
    [HttpGet("misInmuebles")]
    public async Task<IActionResult> ObtenerMisInmuebles()
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

    // api/Inmuebles/actualizar
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

    // api/Inmuebles/obtenerInmueblesConContratoVigente
    [HttpGet("obtenerInmueblesConContratoVigente")]
    public async Task<IActionResult> ObtenerInmueblesConContratoVigente()
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

    // api/Inmuebles/3
    [HttpGet("{id}")]
    public async Task<ActionResult<Inmueble>> ObtenerInmueblePorId(int id)
    {
        try
        {
            //saco el id del propietario del token
            var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

            //busco el inmueble que tenga ese id y que sea del propietario logueado
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.IdInmueble == id && i.PropietarioId == idProp);

            //si no existe o no le pertenece, devuelvo notfound
            if (inmueble == null)
                return NotFound("No se encontró el inmueble o no pertenece al propietario.");

            //si llega hasta aca es porque lo encontró
            return Ok(inmueble);
        }
        catch (Exception ex)
        {
            //si algo falla, mando el error
            return BadRequest(ex.Message);
        }
    }
}