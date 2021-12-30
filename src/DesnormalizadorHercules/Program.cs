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
