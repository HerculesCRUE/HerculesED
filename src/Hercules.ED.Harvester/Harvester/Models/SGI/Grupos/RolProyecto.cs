namespace OAI_PMH.Models.SGI.Grupos
{
    public class RolProyecto : SGI_Base
    {
        public long? id { get; set; }
        public string abreviatura { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public bool? rolPrincipal { get; set; }
        public string orden { get; set; }
        public string equipo { get; set; }
        public bool? activo { get; set; }
    }
}
