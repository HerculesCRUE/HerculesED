namespace OAI_PMH.Models.SGI.ProduccionCientifica
{
    public class Autor : SGI_Base
    {
        public string PersonaRef { get; set; }
        public string Firma { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public float Orden { get; set; }
        public string OrcidId { get; set; }
        public bool Ip { get; set; }
    }
}
