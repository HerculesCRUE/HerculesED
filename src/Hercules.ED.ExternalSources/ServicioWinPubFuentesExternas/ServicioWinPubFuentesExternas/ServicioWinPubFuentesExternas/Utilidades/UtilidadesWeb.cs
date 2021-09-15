using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;

using ServicioWinPubFuentesExternas.Model.ModeloOntologia;

namespace ServicioWinPubFuentesExternas.Utilidades
{


    public class UtilidadesWeb
    {
        private string urlRestService;
        protected Dictionary<string, string> headers = new Dictionary<string, string>();



        public static HttpWebResponse GETResponse { get; set; } = null;

        public UtilidadesWeb(string pUrlRestServicePradoMuseum)
        {
            urlRestService = pUrlRestServicePradoMuseum;
        }

        public List<Publication> getPublicacionesByScopusID(string scopusID, string fecha=1500)
        {
            //Generate get request
    
            string url_1 = urlRestService + "/scopus/GetROs?author_id=" + scopusID+"&year="+fecha;
            HttpWebRequest GETRequest = (HttpWebRequest)WebRequest.Create(url_1);
            GETRequest.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4";
            try
            {
                //Log.Debug("Se recupera contenido de " + url);
                try
                {
                   
                   StreamReader readStream = HacerPeticionWeb(GETRequest);
                    List<Publication> listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(readStream.ReadToEnd());
                    return listaPublicaciones;
                }
                catch (Exception ex)
                {
                    //Log.Debug(ex.Message.ToString());
                    throw ex;
                }

                finally
                {
                    if (GETResponse != null)
                    {
                        GETResponse.Close();
                        GETResponse = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return null;
        }

        private StreamReader HacerPeticionWeb(HttpWebRequest pGETRequest)
        {
            int intento = 1;
            int maxIntentos = 5;
            StreamReader readStream = null;
            while (GETResponse == null && intento <= maxIntentos)
            {
                try
                {
                    GETResponse = (HttpWebResponse)pGETRequest.GetResponse();
                    Stream stream = GETResponse.GetResponseStream();
                    readStream = new StreamReader(stream);
                }
                catch (Exception ex)
                {
                    //Log.Debug(ex.Message.ToString());
                    if (intento == maxIntentos)
                    {
                        throw ex;
                    }
                }
                finally
                {
                    intento++;
                }
            }

            return readStream;
        }

        
        

    }


}
