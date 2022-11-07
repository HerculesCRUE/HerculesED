using Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class PremiosMenciones : SeccionBase
    {
        private readonly List<string> propiedadesItem = new()
        {
            "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/prizes",
            "http://vivoweb.org/ontology/core#relatedBy"
        };
        private readonly string graph = "accreditation";
        public PremiosMenciones(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.030.050.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaPremiosMenciones(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresPreMen = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresPreMen, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesPreMen = GetListLoadedEntity(listadoIdentificadoresPreMen, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesPreMen)
            {
                CvnItemBean itemBean = new()
                {
                    Code = "060.030.050.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesDescripcion),
                    "060.030.050.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesPais),
                    "060.030.050.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesCCAA),
                    "060.030.050.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesCiudad),
                    "060.030.050.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesReconocimientosLigados),
                    "060.030.050.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesFechaConcesion),
                    "060.030.050.100", keyValue.Value);

                // Entidad Premios Menciones
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesEntidadNombre),
                   "060.030.050.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidad),
                    "060.030.050.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidadOtros),
                    "060.030.050.080", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
