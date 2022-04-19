namespace OAI_PMH.Models.SGI.ProduccionCientifica
{
    public class IndiceImpacto : SGI_Base
    {
        public string FuenteImpacto { get; set; }
        public float Indice { get; set; }
        public string Ranking { get; set; }
        public string Anio { get; set; }
        public string OtraFuenteImpacto { get; set; }
        public float PosicionPublicacion { get; set; }
        public float NumeroRevistas { get; set; }
        public bool Revista25 { get; set; }
    }
}
