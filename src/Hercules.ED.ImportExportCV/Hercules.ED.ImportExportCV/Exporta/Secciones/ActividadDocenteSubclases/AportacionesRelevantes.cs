﻿using Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class AportacionesRelevantes : SeccionBase
    {
        private readonly List<string> propiedadesItem = new()
        {
            "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/mostRelevantContributions",
            "http://vivoweb.org/ontology/core#relatedBy"
        };
        private readonly string graph = "activity";

        public AportacionesRelevantes(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.110.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaAportacionesRelevantes(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresApoRel = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresApoRel, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesApoRel = GetListLoadedEntity(listadoIdentificadoresApoRel, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesApoRel)
            {
                CvnItemBean itemBean = new()
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

                //Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVPalabrasClave),
                    "030.110.000.020", keyValue.Value);

                //Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre),
                    "030.110.000.070", keyValue.Value);
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
