using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.RabbitConsume.Models.Services
{
    public class DataPerson
    {
        // Prefijos.
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configJson/prefijos.json")));
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static Guid mCommunityID = mCommunityApi.GetCommunityId();

        public static void ModifyDate(string pIdGnoss, DateTime pDate)
        {
            // Obtención de datos antiguos.
            string fechaAntigua = string.Empty;
            string idRecurso = string.Empty;

            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?s ?fecha ");
            where.Append("WHERE { ");
            where.Append($@"FILTER(?s = <{pIdGnoss}>)");
            where.Append("OPTIONAL {?s roh:lastUpdatedDate ?fecha. } ");
            where.Append("} ");
            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("fecha"))
                    {
                        fechaAntigua = fila["fecha"].value;
                    }

                    idRecurso = fila["s"].value;
                }
            }

            // Conversión de fecha.
            string fechaFinal = $@"{pDate.ToString("yyyy/MM/dd").Replace("/", "")}000000";

            // Inserción/Modificación de triples.
            mResourceApi.ChangeOntoly("person");
            Guid guid = mResourceApi.GetShortGuid(idRecurso);            

            if(!string.IsNullOrEmpty(fechaAntigua))
            {
                // Modificación.
                Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
                List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();

                // Modificación (Triples).
                TriplesToModify triple = new TriplesToModify();
                triple.Predicate = $@"http://w3id.org/roh/lastUpdatedDate";
                triple.NewValue = fechaFinal;
                triple.OldValue = fechaAntigua;
                listaTriplesModificacion.Add(triple);

                dicModificacion.Add(guid, listaTriplesModificacion);
                Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
            }
            else
            {
                // Inserción.
                Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
                List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();

                // Inserción (Triples).                 
                TriplesToInclude triple = new TriplesToInclude();
                triple.Predicate = $@"http://w3id.org/roh/lastUpdatedDate";
                triple.NewValue = fechaFinal;
                listaTriplesInsercion.Add(triple);

                dicInsercion.Add(guid, listaTriplesInsercion);
                Dictionary<Guid, bool> insertado = mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
            }            
        }
    }
}
