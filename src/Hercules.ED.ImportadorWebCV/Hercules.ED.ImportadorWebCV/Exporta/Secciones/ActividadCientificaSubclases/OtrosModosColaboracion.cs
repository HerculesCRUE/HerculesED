using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class OtrosModosColaboracion:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherCollaborations", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "collaboration";
        public OtrosModosColaboracion(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        public void ExportaOtrosModosColaboracion(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "060.020.020.000",
                    Items = new List<CVNObject>()
                };


            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}

