namespace OAI_PMH.Models.SGI.Project
{
    public class PalabraClave : SGI_Base
    {
        public int id { get; set; }
        public int proyectoId { get; set; }
        public string palabraClaveRef { get; set; }
    }
}
