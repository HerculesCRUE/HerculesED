using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Configuration;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace ServicioWinPubFuentesExternas
{
    #region Delegados

    /// <summary>
    /// Delegado para poder utilizar un método con parámetros en los hilos que necesitan lanzarse
    /// </summary>
    /// <param name="pNotificador">Será un objeto de tipo Controller</param>
    public delegate void ParameterizedThreadStart(Object pNotificador);

    #endregion

    partial class TareasService : ServiceBase
    {
        #region Miembros

        private bool mDebug;

        List<ControllerBase> mControladores = new List<ControllerBase>();
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor sin parámetros
        /// </summary>
        public TareasService()
            : base()
        {
            InitializeComponent();
        }

        #endregion

        protected override void OnStop()
        {
            foreach (ControllerBase controller in mControladores)
            {
                controller.TokenCancelacion.Cancel();
            }
        }

        /// <summary>
        /// Comienzo del servicio
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                Dictionary<string, string> paramsServ = ObtenerListaParametrosServicio();
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                int interval = 1;
                int tiempoEntreHilos = 3;

                if (paramsServ.ContainsKey("intervalo"))
                {
                    int.TryParse(paramsServ["intervalo"], out interval);
                }
                if (paramsServ.ContainsKey("tiempoEntreHilos"))
                {
                    int.TryParse(paramsServ["tiempoEntreHilos"], out tiempoEntreHilos);
                }
               
                mControladores.Add(new ControllerObtenerPublicacionesDiario(interval));
                mControladores.Add(new ControllerObtenerPublicacionesRabbit(interval));

                foreach (ControllerBase controller in mControladores)
                {
                    controller.TokenCancelacion = new CancellationTokenSource();
                    Task tarea = Task.Factory.StartNew(() => controller.RealizarMantenimiento());

                    while (tarea.Status != TaskStatus.Running)
                    {
                        Thread.Sleep(tiempoEntreHilos * 1000);
                    }
                }
            }
            catch (Exception ex)
            {
                GuardarLog(ex.ToString());
            }
        }
        
        private Dictionary<string, string> ObtenerListaParametrosServicio()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();

            XmlDocument docXml = new XmlDocument();

            FileStream stream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "Config/service.xml", FileMode.Open);
            docXml.Load(stream);

            XmlNode nodoParam = docXml.SelectSingleNode("servicio-gnoss/parametros-servicio");

            foreach (XmlNode nodo in nodoParam.ChildNodes)
            {
                if (nodo.NodeType != XmlNodeType.Comment && !param.ContainsKey(nodo.Name) && nodo.Attributes["valor"] != null)
                {
                    param.Add(nodo.Name, nodo.Attributes["valor"].Value);
                }
            }

            stream.Close();
            stream.Dispose();
            stream = null;

            return param;
        }

        /// <summary>
        /// Escribe fisicamente las entradas en el log
        /// </summary>
        /// <param name="infoEntry"></param>
        public static void GuardarLog(String pInfoEntry)
        {
            try
            {
                if (pInfoEntry != String.Empty)
                {
                    string nombreLog = "error";
                    string directorioLog = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "logs";

                    if (!Directory.Exists(directorioLog))
                    {
                        Directory.CreateDirectory(directorioLog);
                    }
                    string ficheroLog = directorioLog + Path.DirectorySeparatorChar + nombreLog;
                    string nombreFichero = ficheroLog + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    // File access and writing
                    FileStream logFile = null;

                    if (File.Exists(nombreFichero))
                    {
                        logFile = new FileStream(nombreFichero, FileMode.Append, FileAccess.Write);
                    }
                    else
                    {
                        logFile = new FileStream(nombreFichero, FileMode.Create, FileAccess.Write);
                    }
                    TextWriter logWriter = new StreamWriter(logFile, Encoding.UTF8);

                    // Log entry
                    CultureInfo culture = new CultureInfo(CultureInfo.CurrentCulture.ToString());
                    String logEntry = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss", culture) + " " + pInfoEntry;

                    logWriter.WriteLine(logEntry);
                    logWriter.Close();
                    logFile.Close();
                }
            }
            catch (Exception) { }
        }

      

    }
}

