using EditorCV.Models.EnvioDSpace;
using EditorCV.Models.PreimportModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;

namespace EditorCV.Models
{
    public class AccionesEnvioDSpace
    {
        readonly ConfigService _Configuracion;

        public AccionesEnvioDSpace(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        public void EnvioDSpace(string pIdRecurso)
        {
            //Compruebo si está el ítem en la biblioteca
            try
            {
                //Compruebo el estado del servicio
                Status status = GetStatus();

                //Compruebo si estoy autenticado
                if (status.authenticated != "true")
                {
                    Authentication();
                }

                //Inserto o actualizo
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

        private Status GetStatus()
        {
            Status status = new Status();

            try
            {
                string urlStatus = _Configuracion.GetUrlDSpace() + "/status";
                HttpClient httpClientStatus = new HttpClient();
                HttpResponseMessage responseStatus = httpClientStatus.GetAsync($"{urlStatus}").Result;
                if (responseStatus.IsSuccessStatusCode)
                {
                    status = responseStatus.Content.ReadFromJsonAsync<Status>().Result;
                    if (status.okay != "true")
                    {
                        throw new Exception("Status not okay");
                    }
                }
            }
            catch (Exception)
            {
                return status;
            }

            return status;
        }

        private void Authentication()
        {
            try
            {
                string urlLogin = _Configuracion.GetUrlDSpace() + "/login";

                UserDspace user = new UserDspace
                {
                    email = _Configuracion.GetUsernameDspace(),
                    password = _Configuracion.GetPasswordDspace()
                };

                string content = JsonConvert.SerializeObject(user);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpClient httpClientLoguin = new HttpClient();
                var result = httpClientLoguin.PostAsync(urlLogin, byteContent).Result;

                //Asigno el token
                //TODO
                tokenAuth = "";

            }
            catch (Exception)
            {

            }
        }
    }
}
