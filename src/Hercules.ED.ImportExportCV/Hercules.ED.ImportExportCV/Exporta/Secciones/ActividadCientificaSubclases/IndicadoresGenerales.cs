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
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/generalQualityIndicators", "http://w3id.org/roh/generalQualityIndicatorCV" };
        private readonly string graph = "curriculumvitae";

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
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadores, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new ()
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
