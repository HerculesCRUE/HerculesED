using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using ImportadorWebCV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ExportadorWebCV.Utils
{
    public class UtilityExportar
    {
        public static List<Tuple<string,string>> GetDatos(ResourceApi pResourceApi, string pCVID)
        {
            string select = $@"select distinct ?prop ?o";
            string where = $@"
where {{
    ?cv <http://w3id.org/roh/personalData> ?personalData . 
    ?personalData ?prop ?o
    FILTER(?cv =<{pCVID}>)
}}";

            List<Tuple<string, string>> listaResultado = new List<Tuple<string, string>>();

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            if (resultData.results.bindings.Count == 0)
            {
                return null;
            }
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                listaResultado.Add(new Tuple<string,string>(fila["prop"].value, fila["o"].value));
            }

            return listaResultado;
        }
        public static void AniadirItems(cvnRootResultBean cvn, List<CvnItemBean> listado)
        {
            if (cvn.cvnRootBean == null)
            {
                cvn.cvnRootBean = listado.ToArray();
            }
            else
            {
                cvn.cvnRootBean = cvn.cvnRootBean.Union(listado).ToArray();
            }
        }

        public static string EliminarRDF(string cadena)
        {
            if (string.IsNullOrEmpty(cadena))
            {
                return "";
            }
            return string.Join("|", cadena.Split("|").Select(x => x.Split("@@@")[0]));
        }

    }
}
