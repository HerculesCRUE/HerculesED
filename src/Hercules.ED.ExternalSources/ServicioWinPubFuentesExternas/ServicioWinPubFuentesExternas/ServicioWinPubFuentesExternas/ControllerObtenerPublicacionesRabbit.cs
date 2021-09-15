using System;
using System.IO;
using System.Threading;
using System.Xml;
using Es.Riam.AMQP;
using NotificationOntology;
using ServicioWinPubFuentesExternas.Utilidades;

namespace ServicioWinPubFuentesExternas
{
    public class ControllerObtenerPublicacionesRabbit : ControllerBase
    {
        protected readonly UtilidadesWeb mUtilidadesWeb;
        private XmlDocument mXmlConfig;
        private readonly string mFicheroConfig;
        #region Constructor
        public ControllerObtenerPublicacionesRabbit(int pIntervaloSeg)
            : base(pIntervaloSeg)
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
            mResourceApi.Log.Info("Inicio Hilo Obtener Publicaciones Rabbit");
            try
            {
                mResourceApi.Log.Info("Creando AMPQManager");
                AMQPManager managerQueue = TratamientoConexiones.ObtenerColaAMQP("Cola_ServicioWinGestionPublicaciones_ObtencionPublicaciones");
                if (managerQueue != null)
                {
                    while (!TokenCancelacion.IsCancellationRequested)
                    {
                        if (!EstaProcesoEnMarcha)
                        {
                            mResourceApi.Log.Info("Escuchando la cola");
                            managerQueue.ListenToQueue(new ReceivedDelegate(ProcesarItem), new ShutDownDelegate(OnShutDown));
                            EstaProcesoEnMarcha = true;
                        }
                        Thread.Sleep(1000 * INTERVALO_SEGUNDOS);
                    }
                }
            
            }
            catch (Exception e)
            {
                mResourceApi.Log.Error(e.Message);
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
            
           
            return true;
        }
        #endregion
    }
  
}
