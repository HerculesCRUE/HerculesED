using Es.Riam.AMQP;
using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Xml;
namespace ServicioWinPubFuentesExternas.Utilidades
{
    public static class TratamientoConexiones
    {
        private static ResourceApi mresourceApi;
        public static ResourceApi MresourceApi
        {
            get
            {
                if (mresourceApi == null)
                {
                    mresourceApi = new ResourceApi(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\configOAuth\OAuth_V3.config");
                }
                return mresourceApi;
            }
        }

        public static AMQPManager ObtenerColaAMQP(string colaConexion)
        {
            if (Environment.GetEnvironmentVariable("useEnvironmentVariables") != null && Environment.GetEnvironmentVariable("useEnvironmentVariables").ToLowerInvariant().Equals("true"))
            {
                return CargarConfiguracionAMQPDesdeVariablesEntorno(colaConexion);
            }
            else
            {
                return CargarConfiguracionAMQPDesdeFichero(colaConexion);
            }
        }

        private static AMQPManager CargarConfiguracionAMQPDesdeFichero(string colaConexion)
        {
            AMQPManager managerQueue = null;
            Dictionary<string, string> listaColas = new Dictionary<string, string>();
            string conexion = string.Empty;
            AMQPClientTypes conexionType = AMQPClientTypes.AzureServiceBus;

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\config\colasAMQP.config");

                XmlNode xmlNodes = xDoc.SelectSingleNode("config/queues");

                foreach (XmlNode nodo in xmlNodes.ChildNodes)
                {
                    if (nodo.NodeType != XmlNodeType.Comment && !listaColas.ContainsKey(nodo.Name))
                    {
                        listaColas.Add(nodo.Name, nodo.InnerText);
                    }
                }

                xmlNodes = xDoc.SelectSingleNode("config/connectionType");
                string conexionTypeString = xmlNodes.InnerText;
                if (conexionTypeString == "Rabbit")
                {
                    conexionType = AMQPClientTypes.RabbitMQ;
                }

                xmlNodes = xDoc.SelectSingleNode("config/connectionString");
                conexion = xmlNodes.InnerText;

                managerQueue = new AMQPManager(conexionType, conexion, listaColas[colaConexion]);
            }
            catch (Exception e)
            {
                MresourceApi.Log.Error(e.Message);
            }
            return managerQueue;
        }


        private static AMQPManager CargarConfiguracionAMQPDesdeVariablesEntorno(string colaConexion)
        {
            AMQPManager managerQueue = null;
            string conexion = string.Empty;
            AMQPClientTypes conexionType = AMQPClientTypes.AzureServiceBus;
            string nombreCola = string.Empty;
            if (Environment.GetEnvironmentVariable("AMQP_connectionType") != null)
            {
                string conexionTypeString = Environment.GetEnvironmentVariable("AMQP_connectionType");
                if (conexionTypeString == "Rabbit")
                {
                    conexionType = AMQPClientTypes.RabbitMQ;
                }
            }
            else
            {
                MresourceApi.Log.Error("The environment variable 'AMQP_connectionType' doesn't exist");
            }

            if (Environment.GetEnvironmentVariable("AMQP_connectionString") != null)
            {
                conexion = Environment.GetEnvironmentVariable("AMQP_connectionString");
            }
            else
            {
                MresourceApi.Log.Error("The environment variable 'AMQP_connectionString' doesn't exist");
            }

            if (Environment.GetEnvironmentVariable("AMQP_" + colaConexion) != null)
            {
                nombreCola = Environment.GetEnvironmentVariable("AMQP_" + colaConexion);
            }
            else
            {
                 MresourceApi.Log.Error("The environment variable 'AMQP_" + colaConexion + "' doesn't exist");
            }
            managerQueue = new AMQPManager(conexionType, conexion, nombreCola);
            return managerQueue;
        }

    }


}
