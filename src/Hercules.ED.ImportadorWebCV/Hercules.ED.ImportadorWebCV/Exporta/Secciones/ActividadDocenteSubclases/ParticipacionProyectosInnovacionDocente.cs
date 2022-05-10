﻿using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class ParticipacionProyectosInnovacionDocente : SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/teachingProjects", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "teachingproject";
        public ParticipacionProyectosInnovacionDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        public void ExportaParticipacionProyectos(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.080.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTitulo),
                    "030.080.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaPaisEntidadRealizacion),
                    "030.080.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaCCAAEntidadRealizacion),
                    "030.080.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaCiudadEntidadRealizacion),
                    "030.080.000.250", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoParticipacion),
                    "030.080.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoParticipacionOtros),
                    "030.080.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAportacionProyecto),
                    "030.080.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaRegimenDedicacion),
                    "030.080.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoConvocatoria),
                    "030.080.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoConvocatoriaOtros),
                    "030.080.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoDuracionRelacionLaboral),
                    "030.080.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,
                    "030.080.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaFechaFinalizacionParticipacion),
                    "030.080.000.210", keyValue.Value);

                string numParticipantes = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaNumParticipantes))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaNumParticipantes)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numParticipantes))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.080.000.230", numParticipantes);
                }

                string importeConcedido = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaImporteConcedido))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaImporteConcedido)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(importeConcedido))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.080.000.230", importeConcedido);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAmbitoProyecto),
                    "030.080.000.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAmbitoProyectoOtros),
                    "030.080.000.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaFechaInicio),
                    "030.080.000.280", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
