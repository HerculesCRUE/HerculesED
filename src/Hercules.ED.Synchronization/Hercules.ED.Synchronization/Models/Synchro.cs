using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.Synchronization.Config;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hercules.ED.Synchronization.Program;

namespace Hercules.ED.Synchronization.Models
{   
    public class Synchro
    {
        public ResourceApi mResourceApi;
        public ConfigService configuracion;

        /// <summary>
        /// Obtiene los ORCID de las personas activas.
        /// </summary>
        /// <returns>Lista de identificadores.</returns>
        public List<string> GetPersons()
        {
            HashSet<string> listaOrcid = new HashSet<string>();
            int limit = 10000;
            int offset = 0;
            bool salirBucle = false;

            do
            {
                // Consulta sparql.
                string select = "SELECT * WHERE { SELECT ?persona ?orcid ";
                string where = $@"WHERE {{
                                ?persona a <http://xmlns.com/foaf/0.1/Person>. 
                                ?persona <http://w3id.org/roh/ORCID> ?orcid. 
                                ?persona <http://w3id.org/roh/isActive> 'true'. 
                                }} ORDER BY DESC(?persona) }} LIMIT {limit} OFFSET {offset} ";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string orcid = fila["orcid"].value;
                        if (!string.IsNullOrEmpty(orcid))
                        {
                            listaOrcid.Add(orcid);
                        }
                    }

                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        salirBucle = true;
                    }
                }
                else
                {
                    salirBucle = true;
                }

            } while (!salirBucle);

            List<string> listaOrdenada = listaOrcid.ToList();
            listaOrdenada.Sort();

            return listaOrdenada;
        }

        /// <summary>
        /// Obtiene la fecha de la última actualización.
        /// </summary>
        /// <param name="pOrcid">ORCID del usuario.</param>
        /// <returns></returns>
        public string GetLastUpdatedDate(string pOrcid)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append("SELECT ?fecha ");
            where.Append("WHERE { ");
            where.Append($@"?s <http://w3id.org/roh/ORCID> '{pOrcid}'. ");
            where.Append("OPTIONAL{?s roh:lastUpdatedDate ?fecha. } ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count == 1)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("fecha"))
                    {
                        string fechaGnoss = fila["fecha"].value;
                        string anio = fechaGnoss.Substring(0, 4);
                        string mes = fechaGnoss.Substring(4, 2);
                        string dia = fechaGnoss.Substring(6, 2);

                        return $@"{anio}-{mes}-{dia}";
                    }
                }
            }

            return "1500-01-01";
        }

        /// <summary>
        /// Se queda a la escucha hasta que se cumpla la expresión cron y ejecute la tarea.
        /// </summary>
        public void ProcessComplete()
        {
            Queue colaRabbit = new Queue(configuracion);
            string expresionCron = configuracion.GetCronExternalSource();

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var expression = new CronExpression(expresionCron);
                        DateTimeOffset? time = expression.GetTimeAfter(DateTimeOffset.UtcNow);

                        if (time.HasValue)
                        {
                            Thread.Sleep((time.Value.UtcDateTime - DateTimeOffset.UtcNow));

                            // Obtención de los ORCID de las personas.
                            List<string> listaOrcids = GetPersons();

                            foreach(string orcid in listaOrcids)
                            {
                                string ultimaFechaMod = GetLastUpdatedDate(orcid);
                                colaRabbit.InsertToQueueFuentesExternas(orcid, colaRabbit, ultimaFechaMod);
                            }                            
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log($@"Error - {ex}");
                        Thread.Sleep(60000);
                    }
                }
            }).Start();
        }
    }
}
