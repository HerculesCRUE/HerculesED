using System.Collections.Generic;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.DatosIdentificacion
{
    public class IdentificacionCurriculum : SeccionBase
    {
        public IdentificacionCurriculum(cvnRootResultBean mCvn, string cvID) : base(mCvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "000.020.000.000" a cvn.cvnRootResultBean.
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="version"></param>
        public void ExportaIdentificacionCurriculum(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, string version)
        {
            CvnItemBean itemBean = new()
            {
                Code = "000.020.000.000",
                Items = new List<CVNObject>()
            };

            //Idioma del curriculum
            UtilityExportar.AddCvnItemBeanCvnStringSimple(itemBean, "000.020.000.070", "spa");
            //Version
            UtilityExportar.AddCvnItemBeanCvnStringSimple(itemBean, "000.020.000.080", version.Replace("_", "."));
        }
    }
}
