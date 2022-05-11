using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases
{
    public class GrupoIDI:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience",
            "http://w3id.org/roh/groups","http://w3id.org/roh/relatedGroupCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "group";
        public GrupoIDI(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        public void ExportaGrupoIDI(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<Tuple<string,string>> listadoIdentificadores = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntityCV(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "050.010.000.000",
                    Items = new List<CVNObject>()
                };

            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
}
