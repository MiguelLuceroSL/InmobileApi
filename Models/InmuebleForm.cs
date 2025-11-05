public class InmuebleForm
{
    public string Inmueble { get; set; } = "";  //aca nos llega el json del inmueble
    public IFormFile? Imagen { get; set; }      //aca nos llega el archivo de la foto
}