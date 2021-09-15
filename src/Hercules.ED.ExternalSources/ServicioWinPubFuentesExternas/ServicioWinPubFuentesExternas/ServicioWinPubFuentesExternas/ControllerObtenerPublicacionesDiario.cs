using ServicioWinPubFuentesExternas.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

using System.Threading.Tasks;
using System.Xml;
using ServicioWinPubFuentesExternas.Model.ModeloOntologia;
using NotificationOntology;
namespace ServicioWinPubFuentesExternas
{
    public class ControllerObtenerPublicacionesDiario : ControllerBase
    {
        private readonly int mDiasLanzarObtencionPublicaciones;
        private readonly string mUrlRestService;
        private string mFechaUltimaAct;
        private XmlDocument mXmlConfig;
        private readonly string mFicheroConfig;
        protected readonly UtilidadesWeb mUtilidadesWeb;

        #region Constructor

        public ControllerObtenerPublicacionesDiario(int pIntervaloSeg) : base(pIntervaloSeg)
        {
            mFicheroConfig = System.AppDomain.CurrentDomain.BaseDirectory + "Config/service.xml";
            mXmlConfig = new XmlDocument();
            FileStream stream = new FileStream(mFicheroConfig, FileMode.Open);
            mXmlConfig.Load(stream);
            stream.Close();
            stream.Dispose();

            XmlNode nodo = mXmlConfig.SelectSingleNode("servicio-gnoss/parametros-servicio/urlRestService");
            if (nodo.NodeType != XmlNodeType.Comment && nodo.LastChild.Value != null)
            {
                mUtilidadesWeb = new UtilidadesWeb(nodo.LastChild.Value);
            }
        }

        #endregion

        #region Métodos publicos

        public override void RealizarMantenimiento()
        {
            mResourceApi.Log.Info("Inicio Hilo Obtener Publicaciones Diario");

            while (true)
            {
                try
                {
              /*if (LanzarProceso("diasLanzarObtencionPublicaciones", mDiasLanzarObtencionPublicaciones))
                    {
                        DateTime ahora = DateTime.UtcNow;
                        //TODO: Completar proceso
                        GuardarFechaProceso(ahora, "diasLanzarObtencionPublicaciones");
                    } */
                    string id = "7004419066";

                    string fecha ="2020";
                    //Todo: Obtener scopus_id del la cadena de arriba... 
                    List<Publication> listOfPublication =  mUtilidadesWeb.getPublicacionesByScopusID(id,fecha);
                    
                    foreach (Publication publication in listOfPublication)
                    {
                       Notification notification = new Notification();
                        notification.Dc_source= "Scopus";
                        notification.Dc_type="Publication";
                        notification.Dc_description =JsonConvert.SerializeObject(publication);
                        DateTime ahora = DateTime.UtcNow;
                        notification.Dc_date=ahora;

                        mResourceApi.ChangeOntoly("notification.owl");
                        mResourceApi.LoadSecondaryResource(notification.ToGnossApiResource(mResourceApi, "Notification_"+ Guid.NewGuid()));
                    }
                }
                catch (Exception ex)
                {
                    mResourceApi.Log.Error(ex.ToString());
                }
                finally
                {
                    GC.Collect();
                    Thread.Sleep(INTERVALO_SEGUNDOS * 1000);
                }
            }


        }

        #endregion


        #region Métodos privados
        private void OnShutDown()
        {
            EstaProcesoEnMarcha = false;
        }

        private bool ProcesarItem(string item)
        {
            mResourceApi.Log.Info("Nuevo mensaje Cola: Cola_ServicioWinGestionPublicaciones_ObtencionPublicaciones ");
            mResourceApi.Log.Info("Item: " + item);
            //TODO: Obtener las publicaciones del usuario
            return true;
        }

        private bool LanzarProceso(string pParametroLanzamiento, int pDiasEsperar)
        {
            mXmlConfig = new XmlDocument();

            FileStream stream = new FileStream(mFicheroConfig, FileMode.Open);
            mXmlConfig.Load(stream);
            stream.Close();
            stream.Dispose();

            XmlNode nodoLanzamiento = mXmlConfig.SelectSingleNode("servicio-gnoss/parametros-servicio/" + pParametroLanzamiento);

            if (nodoLanzamiento.Attributes["ultimaActualizacion"] != null)
            {
                DateTime actua = UtilidadesConversionFecha.ConvertirFechaGnossADatetime(nodoLanzamiento.Attributes["ultimaActualizacion"].Value);
                mFechaUltimaAct = nodoLanzamiento.Attributes["ultimaActualizacion"].Value;
                return (actua.AddDays(pDiasEsperar) <= DateTime.Now);
            }
            else
            {
                return true;
            }
        }

        private void GuardarFechaProceso(DateTime pFecha, string pParametroLanzamiento)
        {
            XmlNode nodoLanzamiento = mXmlConfig.SelectSingleNode("servicio-gnoss/parametros-servicio/" + pParametroLanzamiento);

            if (nodoLanzamiento.Attributes["ultimaActualizacion"] == null)
            {
                nodoLanzamiento.Attributes.Append(mXmlConfig.CreateAttribute("ultimaActualizacion"));
            }

            nodoLanzamiento.Attributes["ultimaActualizacion"].Value = UtilidadesConversionFecha.ConvertirFechaDateTimeAFormatoGnoss(pFecha);

            StreamWriter writer = new StreamWriter(mFicheroConfig, false, Encoding.UTF8);
            mXmlConfig.Save(writer);
            writer.Flush();
            writer.Close();
        }
        #endregion
    }

}
