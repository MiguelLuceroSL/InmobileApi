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
    public class ContratosController : ControllerBase
    {
        private readonly DataContext _context;

        public ContratosController(DataContext context)
        {
            _context = context;
        }

        // /api/Contratos/crear
        [HttpPost("crear")]
        public async Task<IActionResult> CrearContrato([FromBody] Contrato contrato)
        {
            try
            {
                var idProp = int.Parse(User.Claims.First(c => c.Type == "id").Value);

                var inmueble = await _context.Inmuebles 
                    .FirstOrDefaultAsync(i => i.IdInmueble == contrato.InmuebleId && i.PropietarioId == idProp); //verificar que el inmueble pertenece al propietario logueado

                if (inmueble == null)
                    return BadRequest("El inmueble no existe o no pertenece al propietario.");

                
                var inquilino = await _context.Inquilinos.FindAsync(contrato.InquilinoId); //verificar que el inquilino existe
                if (inquilino == null)
                    return BadRequest("El inquilino no existe.");

                
                if (contrato.FechaDesde >= contrato.FechaHasta) //validar fechas
                    return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");

                
                inmueble.TieneContratoVigente = true; //marcar inmueble con contrato vigente

                contrato.Vigente = true; //se crea como vigente

                await _context.Contratos.AddAsync(contrato);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(CrearContrato), new { contrato.IdContrato }, contrato);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}