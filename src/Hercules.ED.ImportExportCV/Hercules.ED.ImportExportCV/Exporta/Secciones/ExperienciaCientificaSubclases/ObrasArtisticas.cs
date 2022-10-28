using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases
{
    public class ObrasArtisticas:SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificExperience", 
            "http://w3id.org/roh/supervisedArtisticProjects", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "supervisedartisticproject";
        public ObrasArtisticas(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "050.020.030.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaObrasArtisticas(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresObrArt = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresObrArt, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesObrArt = GetListLoadedEntity(listadoIdentificadoresObrArt, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesObrArt)
            {
                CvnItemBean itemBean = new ()
                {
                    Code = "050.020.030.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasDescripcion), 
                    "050.020.030.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasNombreExpo),
                    "050.020.030.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasPaisExpo), 
                    "050.020.030.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCCAAExpo),
                    "050.020.030.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCiudadExpo), 
                    "050.020.030.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasForoExpo),
                    "050.020.030.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasMonografica), 
                    "050.020.030.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCatalogo),
                    "050.020.030.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasComisario),
                    "050.020.030.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasFechaInicio),
                    "050.020.030.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCatalogacion),
                    "050.020.030.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasPremio),
                    "050.020.030.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasTituloPublicacion),
                    "050.020.030.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasOtros),
                    "050.020.030.160",keyValue.Value);

                // Autores
                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Orden", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresOrden));
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresFirma));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresNombre));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresPrimerApellido));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresSegundoApellido));
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoPropiedadesAutor, "050.020.030.030", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}

