using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InmobileApi.Models
{
	
	public class Propietario
	{
		[Key]
		[Display(Name = "Código Int.")]
		public int IdPropietario { get; set; }

		[Required]
		public string Nombre { get; set; } = "";

		[Required]
		public string Apellido { get; set; } = "";

		[Required]
		public string Dni { get; set; } = "";

		[Display(Name = "Teléfono")]
		public string Telefono { get; set; } = "";

		[Required, EmailAddress]
		public string Email { get; set; } = "";

		[Required(ErrorMessage = "La clave es obligatoria"), DataType(DataType.Password)]
		[JsonIgnore]
		public string? Clave { get; set; } = "";

		public List<Inmueble>? Inmuebles { get; set; }

		public override string ToString()
		{
			var res = $"{Nombre} {Apellido}";
			if (!String.IsNullOrEmpty(Dni))
			{
				res += $" ({Dni})";
			}
			return res;
		}
	}
}