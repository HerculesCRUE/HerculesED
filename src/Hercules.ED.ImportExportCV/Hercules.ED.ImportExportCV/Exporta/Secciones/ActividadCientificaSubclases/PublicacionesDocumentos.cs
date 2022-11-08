using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class PublicacionesDocumentos : SeccionBase
    {
        private readonly List<string> propiedadesItem = new()
        {
            "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/scientificPublications",
            "http://w3id.org/roh/relatedScientificPublicationCV",
            "http://vivoweb.org/ontology/core#relatedBy"
        };
        private readonly string graph = "document";

        public PublicacionesDocumentos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "060.010.010.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="seccion"></param>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaPublicacionesDocumentos(string seccion, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new();

            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string, string>> listadoIdentificadoresPubDoc = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadoresPubDoc != null)
            {
                listadoIdentificadoresPubDoc = listadoIdentificadoresPubDoc.Where(x => listaId.Contains(x.Item3)).ToList();
                if (listadoIdentificadoresPubDoc.Count == 0)
                {
                    return;
                }
            }
            Dictionary<string, Entity> listaEntidadesPubDoc = GetListLoadedEntityCV(listadoIdentificadoresPubDoc, graph, MultilangProp, new List<string>() { "maindocument" });
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesPubDoc)
            {
                CvnItemBean itemBean = new();
                itemBean.Code = "060.010.010.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProd),
                    "060.010.010.010", keyValue.Value);
                if (UtilityExportar.CheckCvnString(UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProdOtros), keyValue.Value))
                {
                    UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProdOtros),
                    "060.010.010.020", keyValue.Value);
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo),
                    "060.010.010.030", keyValue.Value);

                Dictionary<string, string> propVolNum = new Dictionary<string, string>();
                propVolNum.Add("Volumen", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubVolumen));
                propVolNum.Add("Numero", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubNumero));
                UtilityExportar.AddCvnItemBeanCvnVolumeBean(itemBean, propVolNum, "060.010.010.080", keyValue.Value);

                Dictionary<string, string> propiedadesPagIniPagFin = new Dictionary<string, string>();
                propiedadesPagIniPagFin.Add("PaginaInicial", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagIni));
                propiedadesPagIniPagFin.Add("PaginaFinal", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagFin));
                UtilityExportar.AddCvnItemBeanCvnPageBean(itemBean, propiedadesPagIniPagFin, "060.010.010.090", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubPais),
                    "060.010.010.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubCCAA),
                    "060.010.010.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha),
                    "060.010.010.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubURL),
                    "060.010.010.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubDepositoLegal),
                    "060.010.010.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubCiudad),
                    "060.010.010.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosColeccion),
                    "060.010.010.270", keyValue.Value);
                string reseniasRevista = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.pubDocumentosReseniaRevista))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.pubDocumentosReseniaRevista)).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(reseniasRevista))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.010.010.340", reseniasRevista);
                }

                // Properties_cv
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosGradoContribucion),
                    "060.010.010.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosResultadosDestacados),
                    "060.010.010.290", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubRelevante),
                    "060.010.010.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosAutorCorrespondencia),
                    "060.010.010.390", keyValue.Value);

                // Soporte
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte),
                    "060.010.010.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubEditorial),
                    "060.010.010.100", keyValue.Value);

                //Compruebo si hay algun tipo de soporte
                if (itemBean.Items.Any(x => x.Code.Equals("060.010.010.070")))
                {
                    //Compruebo si el soporte es una revista
                    CvnItemBeanCvnString itemBeanCvnString = (CvnItemBeanCvnString)itemBean.Items.First(x => x.Code.Equals("060.010.010.070"));
                    if (itemBeanCvnString.Value.Equals("057"))
                    {
                        UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosNombreRevista),
                        "060.010.010.210", keyValue.Value);
                    }
                    else
                    {
                        UtilityExportar.AddCvnItemBeanPubNombre(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubNombre),
                        "060.010.010.210", keyValue.Value);
                    }
                }
                else
                {
                    UtilityExportar.AddCvnItemBeanPubNombre(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubNombre),
                    "060.010.010.210", keyValue.Value);
                }


                // Autores 
                //Selecciono las firmas de las personas
                Dictionary<string, string> dicFirmas = UtilityExportar.GetFirmasAutores(Variables.ActividadCientificaTecnologica.pubDocumentosAutoresFirma, keyValue.Value);

                //Selecciono los autores
                List<Tuple<string, string, string>> autorNombreApellido = UtilityExportar.GetNombreApellidoAutor(Variables.ActividadCientificaTecnologica.pubDocumentosAutores, keyValue.Value, mResourceApi);
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanListSimple(itemBean, autorNombreApellido, dicFirmas,
                    "060.010.010.040");

                // Traducciones
                UtilityExportar.AddLanguage(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion),
                    "060.010.010.350", keyValue.Value);

                // ID publicación
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalHandle),
                    "060.010.010.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalDOI),
                    "060.010.010.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalPMID),
                    "060.010.010.400", keyValue.Value);
                Dictionary<string, string> dicNombreID = new Dictionary<string, string>();
                dicNombreID.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosNombreOtroPubDigital));
                dicNombreID.Add("ID", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosIDOtroPubDigital));
                UtilityExportar.AddCvnItemBeanCvnExternalPKBeanOthers(itemBean, dicNombreID, "060.010.010.400", keyValue.Value);

                // ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubISBN),
                    "060.010.010.160", keyValue.Value);

                // Citas 
                List<Tuple<string, string, string>> dicCodigosWOS = new();
                dicCodigosWOS.Add(new Tuple<string, string, string>("Double", "060.010.010.310", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasWOS)));
                dicCodigosWOS.Add(new Tuple<string, string, string>("String", "060.010.010.320", "WOS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosWOS,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosScopus = new();
                dicCodigosScopus.Add(new Tuple<string, string, string>("Double", "060.010.010.310", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasScopus)));
                dicCodigosScopus.Add(new Tuple<string, string, string>("String", "060.010.010.320", "SCOPUS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosScopus,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosInrecs = new();
                dicCodigosInrecs.Add(new Tuple<string, string, string>("Double", "060.010.010.310", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasInrecs)));
                dicCodigosInrecs.Add(new Tuple<string, string, string>("String", "060.010.010.320", "INRECS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosInrecs,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosGoogleScholar = new();
                dicCodigosGoogleScholar.Add(new Tuple<string, string, string>("Double", "060.010.010.310", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasGoogleScholar)));
                dicCodigosGoogleScholar.Add(new Tuple<string, string, string>("String", "060.010.010.320", "GOOGLE"));
                dicCodigosGoogleScholar.Add(new Tuple<string, string, string>("String", "060.010.010.370", "Google Scholar"));
                UtilityExportar.AddCitas(itemBean, dicCodigosGoogleScholar,
                    "060.010.010.310", keyValue.Value);


                List<Tuple<string, string, string>> dicCodigosScholar = new();
                dicCodigosScholar.Add(new Tuple<string, string, string>("Double", "060.010.010.310", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasScholar)));
                dicCodigosScholar.Add(new Tuple<string, string, string>("String", "060.010.010.320", "SCHOLAR"));
                dicCodigosScholar.Add(new Tuple<string, string, string>("String", "060.010.010.370", "Semantic Scholar"));
                UtilityExportar.AddCitas(itemBean, dicCodigosScholar,
                    "060.010.010.310", keyValue.Value);

                //Índice de i
                List<Tuple<string, string, string, string, string, string, string>> impactIndex = UtilityExportar.GetImpactIndex(keyValue.Value, mResourceApi);
                foreach (Tuple<string, string, string, string, string, string, string> impactindex in impactIndex)
                {
                    //Source
                    //SourceOther
                    //Categoria
                    //ImpactIndex
                    //PublicationPosition
                    //JournalNumberInCat
                    //Cuartil
                    UtilityExportar.AddImpactIndex(itemBean, impactindex.Item1, impactindex.Item2, impactindex.Item3, impactindex.Item4, impactindex.Item5, impactindex.Item6, impactindex.Item7);
                }

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
