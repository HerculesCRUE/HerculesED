using DesnormalizadorHercules.Models;
using Gnoss.ApiWrapper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace DesnormalizadorHercules
{
    class Program
    {
        private static string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
        //private static ResourceApi resourceApi = new ResourceApi(rutaOauth);


        static void Main(string[] args)
        {

            //new Thread(new ThreadStart(CVActualizarDocumentos)).Start();           
            //new Thread(new ThreadStart(CVActualizarGrupos)).Start();
            //new Thread(new ThreadStart(PersonActualizarPertenenciaLineas)).Start();
            //new Thread(new ThreadStart(PersonActualizarPertenenciaGrupos)).Start();
            //new Thread(new ThreadStart(PersonActualizarNumeroPublicaciones)).Start();
            //new Thread(new ThreadStart(PersonActualizarNumeroProyectos)).Start();
            //new Thread(new ThreadStart(PersonActualizarPersonasPublicas)).Start();
            //new Thread(new ThreadStart(PersonActualizarAreasPersonas)).Start();
            //new Thread(new ThreadStart(PersonEliminarPersonasNoReferenciadas)).Start();
            //new Thread(new ThreadStart(GroupActualizarNumeroMiembros)).Start();
            //new Thread(new ThreadStart(GroupActualizarNumeroProyectos)).Start();
            //new Thread(new ThreadStart(GroupActualizarNumeroPublicaciones)).Start();
            //new Thread(new ThreadStart(GroupActualizarPertenenciaLineas)).Start();
            //new Thread(new ThreadStart(GroupActualizarGruposPublicos)).Start();
            //new Thread(new ThreadStart(DocumentActualizarPertenenciaPersonas)).Start();
            //new Thread(new ThreadStart(DocumentActualizarPertenenciaGrupos)).Start();
            //new Thread(new ThreadStart(DocumentActualizarNumeroCitasMaximas)).Start();
            //new Thread(new ThreadStart(DocumentActualizarNumeroCitasCargadas)).Start();
            //new Thread(new ThreadStart(DocumentActualizarNumeroReferenciasCargadas)).Start();
            //new Thread(new ThreadStart(DocumentActualizarDocumentosPublicos)).Start();
            //new Thread(new ThreadStart(ProjectActualizarPertenenciaPersonas)).Start();
            //new Thread(new ThreadStart(ProjectActualizarPertenenciaGrupos)).Start();
            //new Thread(new ThreadStart(ProjectActualizarProyectosPublicos)).Start();
            //while (true)
            //{
            //    Thread.Sleep(60000);
            //}
            while (true)
            {
                try
                {
                    ResourceApi resourceApi = new ResourceApi(rutaOauth);
                    ActualizadorCV actualizadorCV = new ActualizadorCV(resourceApi);
                    actualizadorCV.ActualizarDocumentos();
                    actualizadorCV.ActualizarGrupos();
                    //TODO otro vinculo entre persona y proyecto
                    actualizadorCV.ActualizarProyectos();

                    //TODO eliminar ususarios


                    ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
                    actualizadorPersonas.ActualizarPertenenciaLineas();
                    actualizadorPersonas.ActualizarPertenenciaGrupos();
                    actualizadorPersonas.ActualizarNumeroPublicaciones();
                    actualizadorPersonas.ActualizarNumeroProyectos();
                    actualizadorPersonas.ActualizarPersonasPublicas();
                    actualizadorPersonas.ActualizarAreasPersonas();
                    actualizadorPersonas.EliminarPersonasNoReferenciadas();
                    //TODO
                    actualizadorPersonas.ActualizarNumeroAreasTematicas();
                    actualizadorPersonas.ActualizarNumeroColaboradores();

                    ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
                    actualizadorGrupos.ActualizarGruposPublicos();
                    actualizadorGrupos.ActualizarNumeroMiembros();
                    actualizadorGrupos.ActualizarNumeroColaboradores();
                    actualizadorGrupos.ActualizarNumeroProyectos();
                    actualizadorGrupos.ActualizarNumeroPublicaciones();
                    actualizadorGrupos.ActualizarNumeroAreasTematicas();
                    actualizadorGrupos.ActualizarPertenenciaLineas();
                    
                    actualizadorGrupos.ActualizarAreasGrupos();

                    ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
                    actualizadorDocument.ActualizarPertenenciaPersonas();
                    actualizadorDocument.ActualizarPertenenciaGrupos();
                    actualizadorDocument.ActualizarNumeroCitasMaximas();
                    actualizadorDocument.ActualizarNumeroCitasCargadas();
                    actualizadorDocument.ActualizarNumeroReferenciasCargadas();
                    actualizadorDocument.ActualizarDocumentosPublicos();
                    actualizadorDocument.ActualizarAreasDocumentos();
                    actualizadorDocument.ActualizarTagsDocumentos();

                    ActualizadorProject actualizadorProject = new ActualizadorProject(resourceApi);
                    actualizadorProject.ActualizarPertenenciaPersonas();
                    actualizadorProject.ActualizarPertenenciaGrupos();
                    actualizadorProject.ActualizarProyectosPublicos();
                    actualizadorProject.ActualizarNumeroAreasTematicas();
                    actualizadorProject.ActualizarNumeroPublicaciones();
                    actualizadorProject.ActualizarNumeroColaboradores();
                    actualizadorProject.ActualizarNumeroMiembros();






                }
                catch (Exception ex)
                {

                }

                Thread.Sleep(10000);
            }
        }

        //public static void CVActualizarDocumentos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorCV actualizadorCV = new ActualizadorCV(resourceApi);
        //            actualizadorCV.ActualizarDocumentos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}

        //public static void CVActualizarGrupos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorCV actualizadorCV = new ActualizadorCV(resourceApi);
        //            actualizadorCV.ActualizarDocumentos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}

        ////actualizadorCV.ActualizarProyectosCV();

        //public static void PersonActualizarPertenenciaLineas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarPertenenciaLineas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonActualizarPertenenciaGrupos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarPertenenciaGrupos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonActualizarNumeroPublicaciones()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarNumeroPublicaciones();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonActualizarNumeroProyectos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarNumeroProyectos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonActualizarPersonasPublicas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarPersonasPublicas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonActualizarAreasPersonas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.ActualizarAreasPersonas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void PersonEliminarPersonasNoReferenciadas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorPerson actualizadorPersonas = new ActualizadorPerson(resourceApi);
        //            actualizadorPersonas.EliminarPersonasNoReferenciadas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void GroupActualizarNumeroMiembros()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
        //            actualizadorGrupos.ActualizarNumeroMiembros();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void GroupActualizarNumeroProyectos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
        //            actualizadorGrupos.ActualizarNumeroProyectos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void GroupActualizarNumeroPublicaciones()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
        //            actualizadorGrupos.ActualizarNumeroPublicaciones();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void GroupActualizarPertenenciaLineas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
        //            actualizadorGrupos.ActualizarPertenenciaLineas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void GroupActualizarGruposPublicos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorGroup actualizadorGrupos = new ActualizadorGroup(resourceApi);
        //            actualizadorGrupos.ActualizarGruposPublicos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarPertenenciaPersonas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarPertenenciaPersonas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarPertenenciaGrupos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarPertenenciaGrupos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarNumeroCitasMaximas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarNumeroCitasMaximas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarNumeroCitasCargadas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarNumeroCitasCargadas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarNumeroReferenciasCargadas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarNumeroReferenciasCargadas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void DocumentActualizarDocumentosPublicos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);
        //            actualizadorDocument.ActualizarDocumentosPublicos();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void ProjectActualizarPertenenciaPersonas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorProject actualizadorProject = new ActualizadorProject(resourceApi);
        //            actualizadorProject.ActualizarPertenenciaPersonas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void ProjectActualizarPertenenciaGrupos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorProject actualizadorProject = new ActualizadorProject(resourceApi);
        //            actualizadorProject.ActualizarPertenenciaPersonas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}
        //public static void ProjectActualizarProyectosPublicos()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            //ResourceApi resourceApi = new ResourceApi(rutaOauth);
        //            ActualizadorProject actualizadorProject = new ActualizadorProject(resourceApi);
        //            actualizadorProject.ActualizarPertenenciaPersonas();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Thread.Sleep(300000);
        //        }
        //    }
        //}

    }
}
