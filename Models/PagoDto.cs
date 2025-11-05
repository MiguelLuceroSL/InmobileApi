namespace InmobileApi.Models
{
    public class PagoDto
    {
        public int IdPago { get; set; }
        public int ContratoId { get; set; }
        public DateTime FechaPago { get; set; }
        public double Importe { get; set; }
        public int NroPago { get; set; }
    }
}