using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InmobileApi.Models
{
    public class Inmueble
	{
		[Key]
		[Display(Name = "N°")]
		public int IdInmueble { get; set; }

		[Required(ErrorMessage = "La dirección es requerida")]
		[Display(Name = "Dirección")]
		public string Direccion { get; set; } = "";

		[Required]
		public string Uso { get; set; } = "";

		[Required]
		public string Tipo { get; set; } = "";

		[Required]
		public int Ambientes { get; set; }

		[Required]
		public double Superficie { get; set; }

		public double Latitud { get; set; }
		public double Longitud { get; set; }

		public double Valor { get; set; }

		[Display(Name = "Imagen")]
		public string? ImagenRuta { get; set; } // guardás la ruta, no el blob

		[NotMapped]
		[Display(Name = "Subir imagen")]
		public IFormFile? ImagenFile { get; set; }

		public bool Disponible { get; set; } = true;

		[Display(Name = "Propietario")]
		public int PropietarioId { get; set; }

		[ForeignKey(nameof(PropietarioId))]
		public Propietario? Propietario { get; set; }

		[Display(Name = "Tiene contrato vigente")]
		public bool TieneContratoVigente { get; set; } = false;

		public List<Contrato>? Contratos { get; set; }
	}
}