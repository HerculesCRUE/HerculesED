using Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class PremiosInnovacionDocente : SeccionBase
    {
        private readonly List<string> propiedadesItem = new()
        {
            "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/teachingInnovationAwardsReceived",
            "http://vivoweb.org/ontology/core#relatedBy"
        };
        private readonly string graph = "accreditation";

        public PremiosInnovacionDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.030.080.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaPremiosInnovacionDocente(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresPreInnDoc = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresPreInnDoc, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesPreInnDoc = GetListLoadedEntity(listadoIdentificadoresPreInnDoc, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesPreInnDoc)
            {
                CvnItemBean itemBean = new()
                {
                    Code = "060.030.080.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaNombre),
                    "060.030.080.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaPaisEntidadConcesionaria),
                    "060.030.080.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaCCAAEntidadConcesionaria),
                    "060.030.080.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaCiudadEntidadConcesionaria),
                    "060.030.080.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaPropuestaDe),
                    "060.030.080.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaFechaConcesion),
                    "060.030.080.100", keyValue.Value);

                //Entidad concesionaria
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre),
                    "060.030.080.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionaria),
                    "060.030.080.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionariaOtros),
                    "060.030.080.080", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }


    }
}
