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
    public class IndicadoresGenerales : SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/generalQualityIndicators", "http://w3id.org/roh/generalQualityIndicatorCV" };
        string graph = "curriculumvitae";

        public IndicadoresGenerales(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.010.060.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaIndicadoresGenerales(Entity entity, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                listadoIdentificadores = listadoIdentificadores.Where(x => listaId.Contains(x.Item1)).ToList();
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
                    Code = "060.010.060.000",
                    Items = new List<CVNObject>()
                };

                string propIndicadores = UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.indicadoresGeneralesCalidad);
                string texto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propIndicadores))) ?
                    keyValue.Value.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propIndicadores)).Select(x => x.values).First().First().Split("@@@").Last()
                    : null;

                UtilityExportar.AddCvnItemBeanCvnRichText(itemBean, texto, "060.010.060.010");

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
