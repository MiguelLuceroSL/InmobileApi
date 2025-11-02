using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobileApi.Models
{
	public class Contrato
	{
		[Key]
		public int IdContrato { get; set; }

		[Required]
		[Display(Name = "Inquilino")]
		public int InquilinoId { get; set; }

		[ForeignKey(nameof(InquilinoId))]
		public Inquilino? Inquilino { get; set; }

		[Required]
		[Display(Name = "Inmueble")]
		public int InmuebleId { get; set; }

		[ForeignKey(nameof(InmuebleId))]
		public Inmueble? Inmueble { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Desde")]
		public DateTime FechaDesde { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Hasta")]
		public DateTime FechaHasta { get; set; }

		[Display(Name = "Cuota mensual ($)")]
		public double CuotaMensual { get; set; }

		[Display(Name = "Vigente")]
		public bool Vigente { get; set; } = true;

		public List<Pago>? Pagos { get; set; }
	}
}