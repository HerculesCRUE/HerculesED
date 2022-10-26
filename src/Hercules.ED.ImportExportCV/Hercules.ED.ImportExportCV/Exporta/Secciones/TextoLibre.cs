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
        readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/freeTextSummary",
            "http://w3id.org/roh/freeTextSummaryValues", "http://w3id.org/roh/freeTextSummaryValuesCV" };

        public TextoLibre(cvnRootResultBean mCvn, string cvID) : base(mCvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "070.010.000.000" a cvn.cvnRootResultBean.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="listaId"></param>
        public void ExportaTextoLibre(Entity entity, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> opciones = new List<string>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                string cv = listadoIdentificadores.First().Item1;
                if (!listaId.Any(x => x.StartsWith(cv)))
                {
                    return;
                }
                else
                {
                    opciones.AddRange(listaId.Where(x => x.StartsWith(cv)).Select(x => x.Split("|||").Last()));
                }
            }
            string propResumenLibre = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.resumenLibre)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFG = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b1DescripcionTFG)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFM = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b2DescripcionTFM)).Select(x => x.prop).FirstOrDefault());

            CvnItemBean itemBean = new ()
            {
                Code = "070.010.000.000",
                Items = new List<CVNObject>()
            };

            //Selecciono el ultimo valor que se corresponde a la propiedad en caso de que esta exista.
            string resumenLibre = null;
            string resumenTFG = null;
            string resumenTFM = null;

            //Si listaId es nulo compruebo todos los valores, en caso contrario compruebo solo los que estén en opciones
            if (listaId == null)
            {
                resumenLibre = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                   entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).First().First().Split("@@@").Last()
                   : null;
                resumenTFG = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).First().First().Split("@@@").Last()
                    : null;
                resumenTFM = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).First().First().Split("@@@").Last()
                    : null;
            }
            else
            {
                if (opciones.Contains(Variables.TextoLibre.resumenLibre))
                {
                    resumenLibre = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                        entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).First().First().Split("@@@").Last()
                        : null;
                }
                if (opciones.Contains(Variables.TextoLibre.b1DescripcionTFG))
                {
                    resumenTFG = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                        entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).First().First().Split("@@@").Last()
                        : null;
                }
                if (opciones.Contains(Variables.TextoLibre.b2DescripcionTFM))
                {
                    resumenTFM = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                        entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).First().First().Split("@@@").Last()
                        : null;
                }
            }            

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
