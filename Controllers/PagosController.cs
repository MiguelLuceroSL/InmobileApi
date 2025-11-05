using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobileApi.Models;
using InmobileApi.Data;

namespace InmobileApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly DataContext _context;

        public PagosController(DataContext context)
        {
            _context = context;
        }

        // api/pagos/contrato/1
        [HttpGet("contrato/{id}")]
        public async Task<IActionResult> ObtenerPagosPorContrato(int id)
        {
            try
            {
                var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

                //verificar que el contrato pertenece a un inmueble del propietario
                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                    .FirstOrDefaultAsync(c => c.IdContrato == id && c.Inmueble.PropietarioId == idProp);

                if (contrato == null)
                    return BadRequest("El contrato no existe o no pertenece a uno de sus inmuebles.");

                //obtener pagos del contrato
                var pagos = await _context.Pagos
                    .Where(p => p.ContratoId == id)
                    .ToListAsync();

                if (pagos == null || pagos.Count == 0)
                    return NotFound("No hay pagos registrados para este contrato.");

                var pagosDTO = pagos.Select(p => new PagoDto
                {
                    IdPago = p.IdPago,
                    ContratoId = p.ContratoId,
                    FechaPago = p.FechaPago,
                    Importe = p.Importe,
                    NroPago = p.NroPago
                }).ToList();

                return Ok(pagosDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}