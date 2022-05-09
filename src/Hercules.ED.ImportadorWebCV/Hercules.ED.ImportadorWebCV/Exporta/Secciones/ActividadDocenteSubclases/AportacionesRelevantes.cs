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
    public class AportacionesRelevantes:SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", 
            "http://w3id.org/roh/mostRelevantContributions", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "activity";

        public AportacionesRelevantes(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        public void ExportaAportacionesRelevantes(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.110.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVDescripcion),
                    "030.110.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVPaisRealizacion),
                    "030.110.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVCCAARealizacion),
                    "030.110.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVCiudadRealizacion),
                    "030.110.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVFechaFinalizacion),
                    "030.110.000.110", keyValue.Value);

                //Palabras clave TODO

                //Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean,UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre),
                    "030.110.000.070",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadora),
                    "030.110.000.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadoraOtros),
                    "030.110.000.100", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
