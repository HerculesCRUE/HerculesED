using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;

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
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaIndicadoresGenerales(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            if (!UtilitySecciones.CheckSecciones(secciones, "060.010.060.000"))
            {
                return;
            }
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "060.010.060.000",
                    Items = new List<CVNObject>()
                };

                string propIndicadores = UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.indicadoresGeneralesCalidad);
                string texto = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propIndicadores))) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propIndicadores)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                    : null;

                UtilityExportar.AddCvnItemBeanCvnRichText(itemBean, texto, "060.010.060.010");

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
