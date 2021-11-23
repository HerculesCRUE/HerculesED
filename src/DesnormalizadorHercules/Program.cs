using DesnormalizadorHercules.Models;
using System;
using System.Threading;

namespace DesnormalizadorHercules
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                ActualizadorPerson actualizadorPersonas = new ActualizadorPerson();
                actualizadorPersonas.ActualizarPertenenciaLineas();
                actualizadorPersonas.ActualizarPertenenciaGrupos();
                actualizadorPersonas.ActualizarNumeroPublicaciones();
                actualizadorPersonas.ActualizarNumeroProyectos();                
                ActualizadorGroup actualizadorGrupos = new ActualizadorGroup();
                actualizadorGrupos.ActualizarNumeroMiembros();
                ActualizadorDocument actualizadorDocument = new ActualizadorDocument();
                actualizadorDocument.ActualizarPertenenciaPersonas();
                ActualizadorProject actualizadorProject = new ActualizadorProject();
                actualizadorProject.ActualizarPertenenciaPersonas();
                Thread.Sleep(10000);
            }
        }
    }
}
