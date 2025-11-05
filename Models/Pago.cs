using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobileApi.Models
{
	public class Pago
	{
		[Key]
		public int IdPago { get; set; }

		[Required]
		[Display(Name = "Contrato")]
		public int ContratoId { get; set; }

		[ForeignKey(nameof(ContratoId))]
		public Contrato? Contrato { get; set; }

		[Display(Name = "N° de Pago")]
		public int NroPago { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Fecha de Pago")]
		public DateTime FechaPago { get; set; }

		[Display(Name = "Importe ($)")]
		public double Importe { get; set; }
	}
}