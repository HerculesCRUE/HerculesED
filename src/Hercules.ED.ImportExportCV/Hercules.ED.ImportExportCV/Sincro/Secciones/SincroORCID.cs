using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportExportCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Sincro.Secciones;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hercules.ED.ImportExportCV.Sincro.Secciones
{
    public class SincroORCID : SeccionBase
    {
        public SincroORCID(cvnRootResultBean cvn, string cvID, ConfigService configuracion) : base(cvn, cvID, configuracion)
        {

        }

        /// <summary>
        /// Añade a la persona el triple del ORCID
        /// </summary>
        /// <param name="idPersona">Identificador largo de la persona</param>
        /// <param name="orcid">ORCID</param>
        public bool SincroDatosPersonaORCID(string idPersona, string orcid)
        {
            List<TriplesToInclude> triplesToIncludes = new List<TriplesToInclude>();
            Guid idMainEntity = mResourceApi.GetShortGuid(idPersona);
            triplesToIncludes.Add(new TriplesToInclude(orcid, ImportadorWebCV.Variables.DatosIdentificacion.ORCID));

            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>()
                {
                    {
                        idMainEntity, triplesToIncludes
                    }
                };

            return mResourceApi.InsertPropertiesLoadedResources(triplesToInclude)[idMainEntity];
        }
    }
}
