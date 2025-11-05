public class ContratoDTO
{
    public int IdContrato { get; set; }
    public int InquilinoId { get; set; }
    public int InmuebleId { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public double CuotaMensual { get; set; }
    public bool Vigente { get; set; }
}