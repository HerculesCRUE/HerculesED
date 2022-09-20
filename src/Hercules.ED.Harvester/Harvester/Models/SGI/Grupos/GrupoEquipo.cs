namespace OAI_PMH.Models.SGI.Grupos
{
    public class GrupoEquipo : SGI_Base
    {
        public long? id { get; set; }
        public long? grupoId { get; set; }
        public string personaRef { get; set; }
        public RolProyecto rol { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFin { get; set; }
        public string dedicacion { get; set; }
        public float? participacion { get; set; }
    }
}
