using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;

namespace ImportadorWebCV.Exporta.Secciones
{
    public class TextoLibre : SeccionBase
    {

        public TextoLibre(cvnRootResultBean mCvn, string cvID) : base(mCvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "070.010.000.000" a cvn.cvnRootResultBean.
        /// </summary>
        /// <param name="entity"></param>
        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones)
        {
            if (!UtilitySecciones.CheckSecciones(secciones, "070.010.000.000"))
            {
                return;
            }
            string propResumenLibre = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.resumenLibre)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFG = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b1DescripcionTFG)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFM = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b2DescripcionTFM)).Select(x => x.prop).FirstOrDefault());

            List<CvnItemBean> listado = new List<CvnItemBean>();
            CvnItemBean itemBean = new CvnItemBean()
            {
                Code = "070.010.000.000",
                Items = new List<CVNObject>()
            };

            //Selecciono el ultimo valor que se corresponde a la propiedad en caso de que esta exista.
            string resumenLibre = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFG = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFM = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;

            //Separación de los diferentes apartados por los titulos del FECYT. 
            string resumen = resumenLibre 
                + " B.1. Breve descripción del Trabajo de Fin de Grado (TFG) y puntuación obtenida" + resumenTFG 
                + " B.2. Breve descripción del Trabajo de Fin de Máster (TFM) y puntuación obtenida" + resumenTFM;

            UtilityExportar.AddCvnItemBeanCvnRichText(itemBean, resumen, "070.010.000.010");

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
