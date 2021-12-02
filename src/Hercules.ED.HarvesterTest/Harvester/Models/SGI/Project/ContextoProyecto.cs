using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.Project
{
    public class ContextoProyecto : SGI_Base
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string Objetivos { get; set; }
        public string Intereses { get; set; }
        public string ResultadosPrevistos { get; set; }
        public string PropiedadResultados { get; set; }
        public AreaTematica AreaTematicaConvocatoria { get; set; }
        public AreaTematica AreaTematica { get; set; }
    }
}
