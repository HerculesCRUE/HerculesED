using Gnoss.ApiWrapper;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using ImportadorWebCV.Variables;
using Gnoss.ApiWrapper.ApiModel;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using ImportadorWebCV;
using System.Runtime.InteropServices;
using ExportadorWebCV.Utils;

namespace ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases
{
    public class CargosActividades : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/previousPositions", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "position";

        public CargosActividades(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        public void ExportaCargosActividades(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
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
