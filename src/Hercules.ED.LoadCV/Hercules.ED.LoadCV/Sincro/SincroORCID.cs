using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hercules.ED.ImportExportCV.Sincro.Secciones
{
    public class SincroORCID
    {

        /// <summary>
        /// Añade a la persona el triple del ORCID
        /// </summary>
        /// <param name="idPersona">Identificador largo de la persona</param>
        /// <param name="orcid">ORCID</param>
        public bool InsertaORCIDPersona(string idPersona, string orcid, ResourceApi resourceApi)
        {
            List<TriplesToInclude> triplesToIncludes = new List<TriplesToInclude>();
            Guid idMainEntity = resourceApi.GetShortGuid(idPersona);

            triplesToIncludes.Add(new TriplesToInclude(orcid, "http://w3id.org/roh/ORCID"));

            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
                {
                    {
                        idMainEntity, triplesToIncludes
                    }
                };

            return resourceApi.InsertPropertiesLoadedResources(triplesToInclude)[idMainEntity];
        }

        /// <summary>
        /// Añade a la persona el triple del ORCID
        /// </summary>
        /// <param name="idPersona">Identificador largo de la persona</param>
        /// <param name="orcid">ORCID</param>
        public bool ActualizaORCIDPersona(string idPersona, string oldOrcid, string orcid, ResourceApi resourceApi)
        {
            List<TriplesToModify> triplesToModifies = new List<TriplesToModify>();
            Guid idMainEntity = resourceApi.GetShortGuid(idPersona);

            triplesToModifies.Add(new TriplesToModify(orcid, oldOrcid, "http://w3id.org/roh/ORCID"));

            Dictionary<Guid, List<TriplesToModify>> triplesToModify = new Dictionary<Guid, List<TriplesToModify>>()
                {
                    {
                        idMainEntity, triplesToModifies
                    }
                };

            return resourceApi.ModifyPropertiesLoadedResources(triplesToModify)[idMainEntity];
        }
    }
}
