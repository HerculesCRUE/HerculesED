using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases
{
    public class SituacionProfesionalActual : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/currentProfessionalSituation", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "position";

        public SituacionProfesionalActual(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                //listado.add(item)
            }


            //Añado en el cvnRootResultBean los items que forman parte del listado
            //UtilityExportar.AniadirItems(mCvn, listaEntidadesSP.Select(x=>x.Value));
        }
    }
}
