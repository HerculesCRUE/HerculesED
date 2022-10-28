using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class OtrasDistinciones : SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/otherDistinctions", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "accreditation";
        public OtrasDistinciones(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "060.030.060.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaOtrasDistinciones(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresOtrDis = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresOtrDis, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesOtrDis = GetListLoadedEntity(listadoIdentificadoresOtrDis, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesOtrDis)
            {
                CvnItemBean itemBean = new ();
                itemBean.Code = "060.030.060.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesDescripcion),
                    "060.030.060.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesPais),
                    "060.030.060.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesCCAA),
                    "060.030.060.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesCiudad),
                    "060.030.060.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbito),
                    "060.030.060.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbitoOtros),
                    "060.030.060.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesFechaConcesion),
                    "060.030.060.110", keyValue.Value);
                // Entidad afiliación
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesEntidadNombre),
                    "060.030.060.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidad),
                    "060.030.060.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidadOtros),
                    "060.030.060.080", keyValue.Value);
                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
