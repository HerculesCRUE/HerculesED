using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EditorCV.Models
{
    public class AccionesEnvioDSpace
    {
        readonly ConfigService _Configuracion;

        public AccionesEnvioDSpace(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        public void EnvioDSpace(ConfigService pConfig, string pIdRecurso)
        {
            //Compruebo si está el ítem en la biblioteca
            try
            {
                string urlEstado = _Configuracion.GetUrlDSpace() + "/items/" + pIdRecurso;
                HttpClient httpClientEstado = new HttpClient();
                HttpResponseMessage responseEstado = httpClientEstado.GetAsync($"{urlEstado}").Result;
                if (responseEstado.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //En caso de no estar lo inserto

                }
                else if (responseEstado.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Si está en la biblioteca actualizo los datos

                }
                else
                {
                    throw new Exception("No se ha recibido un código de estado válido");
                }
            }
            catch (Exception)
            {

            }
        }


    }
}
