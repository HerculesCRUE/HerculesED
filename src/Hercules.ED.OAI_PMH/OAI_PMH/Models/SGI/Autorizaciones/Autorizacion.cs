namespace OAI_PMH.Models.SGI.Autorizacion
{
    public class Autorizacion : SGI_Base
    {
        public int id { get; set; }
        public string solicitanteRef { get; set; }
        public string titulo { get; set; }
        public string entidadRef { get; set; }
        public string responsableRef { get; set; }
        public string datosResponsable { get; set; }
    }
}
