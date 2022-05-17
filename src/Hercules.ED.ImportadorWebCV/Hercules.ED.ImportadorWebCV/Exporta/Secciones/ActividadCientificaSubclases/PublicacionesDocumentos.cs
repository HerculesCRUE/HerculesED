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
    public class PublicacionesDocumentos : SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/scientificPublications", "http://w3id.org/roh/relatedScientificPublicationCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "document";
        public PublicacionesDocumentos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        /// <summary>
        /// Exporta los datos de la sección "060.010.010.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaPublicacionesDocumentos(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntityCV(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean();
                itemBean.Code = "060.010.010.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProd),
                    "060.010.010.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProdOtros),
                    "060.010.010.020", keyValue.Value);
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
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubNombre),
                    "060.010.010.210", keyValue.Value);

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
                List<Tuple<string, string, string>> dicCodigosWOS = new List<Tuple<string, string, string>>();
                dicCodigosWOS.Add(new Tuple<string, string, string>("Double", "060.010.010.310",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasWOS)));
                dicCodigosWOS.Add(new Tuple<string, string, string>("String", "060.010.010.320", "WOS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosWOS,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosScopus = new List<Tuple<string, string, string>>();
                dicCodigosScopus.Add(new Tuple<string, string, string>("Double", "060.010.010.310",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasScopus)));
                dicCodigosScopus.Add(new Tuple<string, string, string>("String", "060.010.010.320", "SCOPUS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosScopus,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosInrecs = new List<Tuple<string, string, string>>();
                dicCodigosInrecs.Add(new Tuple<string, string, string>("Double", "060.010.010.310",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasInrecs)));
                dicCodigosInrecs.Add(new Tuple<string, string, string>("String", "060.010.010.320", "INRECS"));
                UtilityExportar.AddCitas(itemBean, dicCodigosInrecs,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosScholar = new List<Tuple<string, string, string>>();
                dicCodigosInrecs.Add(new Tuple<string, string, string>("Double", "060.010.010.310",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosCitasScholar)));
                dicCodigosInrecs.Add(new Tuple<string, string, string>("String", "060.010.010.320", "OTHERS"));
                dicCodigosInrecs.Add(new Tuple<string, string, string>("String", "060.010.010.370", "Semantic Scholar"));
                UtilityExportar.AddCitas(itemBean, dicCodigosInrecs,
                    "060.010.010.310", keyValue.Value);

                List<Tuple<string, string, string>> dicCodigosOther = new List<Tuple<string, string, string>>();
                dicCodigosOther.Add(new Tuple<string, string, string>("Double", "060.010.010.310",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosOtrasCitas)));
                dicCodigosOther.Add(new Tuple<string, string, string>("String", "060.010.010.320", "OTHERS"));
                dicCodigosOther.Add(new Tuple<string, string, string>("String", "060.010.010.370",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosOtrasCitasNombre)));
                UtilityExportar.AddCitas(itemBean, dicCodigosOther,
                    "060.010.010.310", keyValue.Value);
                //TODO otras citas



                // TODO Indice de impacto


                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
