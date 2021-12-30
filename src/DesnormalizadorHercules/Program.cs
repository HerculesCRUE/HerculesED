using DesnormalizadorHercules.Models;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace DesnormalizadorHercules
{
    class Program
    {
        static void Main()
        {
            ActualizadorEDMA.DesnormalizarDatosPersona("http://gnoss.com/items/Person_e883a5d8-f7fa-4f9b-aba4-ec6570646b6e_1b1099f0-930c-458a-9ae0-e9c77495d25b");
            bool eliminarDatos = false;
            if (eliminarDatos)
            {
                ActualizadorEDMA.EliminarDatosDesnormalizados();
            }
            while (true)
            {
                try
                {
                    
                    ActualizadorEDMA.DesnormalizarTodo();
                }
                catch (Exception)
                {

                }
                Thread.Sleep(10000);
            }
        }
    }
}
