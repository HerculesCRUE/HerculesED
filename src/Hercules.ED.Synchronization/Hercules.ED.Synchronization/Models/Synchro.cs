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
        public Dictionary<string, string> GetPersons()
        {
            Dictionary<string, string> dicOrcid = new Dictionary<string, string>();
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
                        if (!string.IsNullOrEmpty(orcid) && !dicOrcid.ContainsKey(fila["persona"].value))
                        {
                            dicOrcid.Add(fila["persona"].value, orcid);
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

            return dicOrcid;
        }

        /// <summary>
        /// Obtiene la fecha de la última actualización.
        /// </summary>
        /// <param name="pOrcid">ORCID del usuario.</param>
        /// <returns></returns>
        public string GetLastUpdatedDate(string pGnossId)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append("SELECT ?fecha ");
            where.Append("WHERE { ");
            where.Append($@"FILTER(?s = <{pGnossId}>) ");
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

            return "1900-01-01";
        }

        /// <summary>
        /// Obtiene los IDs y Token del usuario para FigShare y GitHub.
        /// </summary>
        /// <param name="pOrcid">ORCID del usuario.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetUsersIDs(string pGnossId)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append("SELECT ?usuarioFigshare ?tokenFigshare ?usuarioGitHub ?tokenGitHub ");
            where.Append("WHERE { ");
            where.Append($@"FILTER(?s = <{pGnossId}>) ");
            where.Append("OPTIONAL{?s <http://w3id.org/roh/usuarioFigShare> ?usuarioFigshare. } ");
            where.Append("OPTIONAL{?s <http://w3id.org/roh/tokenFigShare> ?tokenFigshare. } ");
            where.Append("OPTIONAL{?s <http://w3id.org/roh/usuarioGitHub> ?usuarioGitHub. } ");
            where.Append("OPTIONAL{?s <http://w3id.org/roh/tokenGitHub> ?tokenGitHub. } ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("usuarioFigshare") && !string.IsNullOrEmpty(fila["usuarioFigshare"].value))
                    {
                        dicResultados.Add("usuarioFigshare", fila["usuarioFigshare"].value);
                    }
                    if (fila.ContainsKey("tokenFigshare") && !string.IsNullOrEmpty(fila["tokenFigshare"].value))
                    {
                        dicResultados.Add("tokenFigshare", fila["tokenFigshare"].value);
                    }
                    if (fila.ContainsKey("usuarioGitHub") && !string.IsNullOrEmpty(fila["usuarioGitHub"].value))
                    {
                        dicResultados.Add("usuarioGitHub", fila["usuarioGitHub"].value);
                    }
                    if (fila.ContainsKey("tokenGitHub") && !string.IsNullOrEmpty(fila["tokenGitHub"].value))
                    {
                        dicResultados.Add("tokenGitHub", fila["tokenGitHub"].value);
                    }
                }
            }

            return dicResultados;
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
                            Dictionary<string, string> listaOrcids = GetPersons();

                            foreach (KeyValuePair<string, string> item in listaOrcids)
                            {
                                // Obtención de la última fecha de modificación.
                                string ultimaFechaMod = GetLastUpdatedDate(item.Key);
                                //string ultimaFechaMod = "1500-01-01";

                                // Obtención de los datos necesarios de FigShare y GitHub.
                                Dictionary<string, string> dicIDs = GetUsersIDs(item.Key);

                                // Inserción a la cola de Rabbit.
                                colaRabbit.InsertToQueueFuentesExternas(item.Value, colaRabbit, ultimaFechaMod, dicIDs, item.Key);
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
