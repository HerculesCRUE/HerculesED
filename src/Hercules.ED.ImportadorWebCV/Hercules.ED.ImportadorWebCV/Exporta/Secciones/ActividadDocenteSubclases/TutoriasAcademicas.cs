﻿using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class TutoriasAcademicas:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/academicTutorials", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "tutorship";

        public TutoriasAcademicas(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.050.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaTutoriasAcademicas(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                listadoIdentificadores = listadoIdentificadores.Where(x => listaId.Contains(x.Item2)).ToList();
                if (listadoIdentificadores.Count == 0)
                {
                    return;
                }
            }
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.050.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaPaisEntidadRealizacion),
                    "030.050.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaCCAAEntidadRealizacion),
                    "030.050.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaCiudadEntidadRealizacion),
                    "030.050.000.060", keyValue.Value);

                string numAlumnos = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaNumAlumnosTutelados))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaNumAlumnosTutelados)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numAlumnos))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.050.000.110", numAlumnos);
                }

                string frecuenciaActividad = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaFrecuenciaActividad))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaFrecuenciaActividad)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(frecuenciaActividad))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.050.000.120", frecuenciaActividad);
                }

                string numHorasECTS = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaNumHorasECTS))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.tutoAcademicaNumHorasECTS)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numHorasECTS))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.050.000.130", numHorasECTS);
                }

                //Nombre programa
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaNombrePrograma),
                    "030.050.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros),
                    "030.050.000.020", keyValue.Value);

                //Entidad realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean,UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaEntidadRealizacionNombre),
                    "030.050.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacion),
                    "030.050.000.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacionOtros),
                    "030.050.000.100", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
