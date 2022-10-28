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
    public class RedesCooperacion :SeccionBase
    {

        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/networks", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "network";
        public RedesCooperacion(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "060.030.040.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaRedesCooperacion( Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresRedCoop = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresRedCoop, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesRedCoop = GetListLoadedEntity(listadoIdentificadoresRedCoop, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesRedCoop)
            {
                CvnItemBean itemBean = new ()
                {
                    Code = "060.030.040.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopNombre),
                    "060.030.040.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopIdentificacion),
                    "060.030.040.020", keyValue.Value);

                string numInvestigadores = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.redesCoopNumInvestigadores))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.redesCoopNumInvestigadores)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numInvestigadores))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.030.040.030", numInvestigadores);

                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopPaisRadicacion),
                    "060.030.040.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopCCAARadicacion),
                    "060.030.040.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopCiudadRadicacion),
                    "060.030.040.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopPaisEntidadSeleccion),
                    "060.030.040.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopCCAAEntidadSeleccion),
                    "060.030.040.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopCiudadEntidadSeleccion),
                    "060.030.040.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTareas),
                    "060.030.040.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopFechaInicio),
                    "060.030.040.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, 
                    "060.030.040.170", keyValue.Value);

                // Entidad Seleccion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopEntidadSeleccionNombre),
                    "060.030.040.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccion),
                    "060.030.040.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccionOtros),
                    "060.030.040.140", keyValue.Value);

                // Entidades Participantes
                List<Tuple<string, string, string>> dicCodigos = new ();
                dicCodigos.Add(new Tuple<string, string, string>("EntityBean", "060.030.040.070",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipanteNombre)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.030.040.090",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipante)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.030.040.100",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipanteOtros)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicCodigos,
                   "060.030.040.070", keyValue.Value,
                   UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipante),
                   UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipanteOtros));

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
