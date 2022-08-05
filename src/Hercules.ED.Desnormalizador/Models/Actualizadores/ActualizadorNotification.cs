using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace DesnormalizadorHercules.Models.Actualizadores
{
    class ActualizadorNotification : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorNotification(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        public void ActualizarNotificaciones()
        {
            List<string> listResult = new List<string>();

            int limit = 10000;
            int offset = 0;
            //Fecha en formato GNOSS de hace 7 días.
            string dateNow = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            dateNow += "000000";
            while (true)
            {
                //Consulta.
                string select = "SELECT distinct ?s ";
                string where = $@"WHERE {{
                                SELECT *
                                WHERE{{
						?s a <http://w3id.org/roh/Notification> . 
						?s <http://purl.org/dc/terms/issued> ?o . FILTER(xsd:integer(?o) < {dateNow} )
					}}order by (?o) }}limit {limit} offset {offset}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "notification");
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    listResult.Add(fila["s"].value);
                }
                offset += limit;

                if (resultData.results.bindings.Count < limit)
                {
                    break;
                }
            }


            //Borra los recursos.
            Parallel.ForEach(listResult, new ParallelOptions { MaxDegreeOfParallelism = 5 }, identificador =>
            {
                int numIntentos = 0;
                while (true)
                {
                    numIntentos++;
                    //Si el número de intentos es superior a 10 no continuo intentando eliminar el recurso, e inserto en el log el identificador.
                    if (numIntentos > 10)
                    {
                        mResourceApi.Log.Error("El recurso " + identificador + " no se ha podido eliminar");
                        break;
                    }

                    //Eliminar el recurso
                    try
                    {
                        if (mResourceApi.PersistentDelete(mResourceApi.GetShortGuid(identificador)))
                        {
                            //Si elimino el recurso salgo del bucle.
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        mResourceApi.Log.Error(e.Message);
                    }
                }
            });

        }
    }
}
