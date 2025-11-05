using InmobileApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InmobileApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContratosController : ControllerBase
    {
        private readonly DataContext _context;

        public ContratosController(DataContext context)
        {
            _context = context;
        }

        // api/Contratos/inmueble/1
        [HttpGet("inmueble/{id}")]
        public async Task<IActionResult> ObtenerContratoPorInmueble(int id)
        {
            try
            {
                //obtener id del propietario logueado
                var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

                //verificar que el inmueble pertenece al propietario
                var inmueble = await _context.Inmuebles
                    .FirstOrDefaultAsync(i => i.IdInmueble == id && i.PropietarioId == idProp);

                if (inmueble == null)
                    return BadRequest("El inmueble no existe o no pertenece al propietario.");

                //obtener contrato vigente de ese inmueble
                var contrato = await _context.Contratos
                    .FirstOrDefaultAsync(c => c.InmuebleId == id && c.Vigente);

                if (contrato == null)
                    return NotFound("No hay contrato vigente para este inmueble.");

                //mapear a dto
                var contratoDTO = new ContratoDTO
                {
                    IdContrato = contrato.IdContrato,
                    InquilinoId = contrato.InquilinoId,
                    InmuebleId = contrato.InmuebleId,
                    FechaDesde = contrato.FechaDesde,
                    FechaHasta = contrato.FechaHasta,
                    CuotaMensual = contrato.CuotaMensual,
                    Vigente = contrato.Vigente
                };

                return Ok(contratoDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}