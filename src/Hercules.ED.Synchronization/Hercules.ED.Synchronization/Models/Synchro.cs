using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.Synchronization.Config;
using Quartz;
using static Hercules.ED.Synchronization.Program;

namespace Hercules.ED.Synchronization.Models
{
    public class Synchro
    {
        public ResourceApi mResourceApi;
        public ConfigService mConfiguracion;
        public string mPrefijos;

        /// <summary>
        /// Obtiene los ORCID de las personas activas.
        /// </summary>
        /// <returns>Lista de identificadores.</returns>
        public Dictionary<string, string> GetPersons()
        {
            Dictionary<string, string> dicOrcid = new();
            int limit = 10000;
            int offset = 0;
            bool salirBucle = false;

            do
            {
                // Consulta sparql.
                string select = $@"{mPrefijos} SELECT * WHERE {{ SELECT ?persona ?orcid ";
                string where = $@"WHERE {{
                                ?persona a foaf:Person. 
                                ?persona roh:ORCID ?orcid. 
                                ?persona roh:isActive 'true'. 
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
        /// <param name="pGnossId">GnossId del usuario.</param>
        /// <returns>Fecha de la última actualización de fuentes externas del usuario.</returns>
        public string GetLastUpdatedDate(string pGnossId)
        {
            // Consulta sparql.
            string select = $@"{mPrefijos} SELECT ?fecha ";
            string where = $@"WHERE {{
                                FILTER(?s = <{pGnossId}>)
                                OPTIONAL{{?s roh:lastUpdatedDate ?fecha. }} }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count == 1 && resultadoQuery.results.bindings.First().ContainsKey("fecha"))
            {
                string fechaGnoss = resultadoQuery.results.bindings.First()["fecha"].value;
                string anio = fechaGnoss.Substring(0, 4);
                string mes = fechaGnoss.Substring(4, 2);
                string dia = fechaGnoss.Substring(6, 2);

                return $@"{anio}-{mes}-{dia}";
            }

            return "1900-01-01";
        }

        /// <summary>
        /// Obtiene los IDs y Token del usuario para FigShare y GitHub.
        /// </summary>
        /// <param name="pOrcid">ORCID del usuario.</param>
        /// <returns>Diccionario con el tipo de usuario/token y el valor.</returns>
        public Dictionary<string, string> GetUsersIDs(string pGnossId)
        {
            Dictionary<string, string> dicResultados = new();

            // Consulta sparql.
            string select = $@"{mPrefijos} SELECT ?usuarioFigshare ?tokenFigshare ?usuarioGitHub ?tokenGitHub ";
            string where = $@"WHERE {{
                                FILTER(?s = <{pGnossId}>)
                                OPTIONAL{{?s roh:usuarioFigShare ?usuarioFigshare. }}
                                OPTIONAL{{?s roh:tokenFigShare ?tokenFigshare. }}
                                OPTIONAL{{?s roh:usuarioGitHub ?usuarioGitHub. }}
                                OPTIONAL{{?s roh:tokenGitHub ?tokenGitHub. }} }}";

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
            // Inicializa la configuración.
            Queue colaRabbit = new (mConfiguracion);

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        // Obtiene la diferencia entre horas.
                        CronExpression expression = new (mConfiguracion.cronExternalSource);
                        DateTimeOffset? time = expression.GetTimeAfter(DateTimeOffset.UtcNow);
                        Thread.Sleep((time.Value.UtcDateTime - DateTimeOffset.UtcNow));

                        // Obtención de los ORCID de las personas.
                        Dictionary<string, string> listaOrcids = GetPersons();

                        foreach (KeyValuePair<string, string> item in listaOrcids)
                        {
                            // Obtención de la última fecha de modificación.
                            string ultimaFechaMod = GetLastUpdatedDate(item.Key);

                            // Obtención de los datos necesarios de FigShare y GitHub.
                            Dictionary<string, string> dicIDs = GetUsersIDs(item.Key);

                            // Inserción a la cola de Rabbit.
                            colaRabbit.InsertToQueueFuentesExternas(item.Value, colaRabbit, ultimaFechaMod, dicIDs, item.Key);
                        }

                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log($@"[ERROR] {ex.Message} {ex.StackTrace}");
                        Thread.Sleep(60000); // 1min.
                    }
                }
            }).Start();
        }
    }
}
