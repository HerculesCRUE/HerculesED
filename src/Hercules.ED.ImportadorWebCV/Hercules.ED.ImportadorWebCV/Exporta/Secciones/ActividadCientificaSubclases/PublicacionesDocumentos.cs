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
    public class PublicacionesDocumentos:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/scientificProduction", "http://w3id.org/roh/scientificProductionCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "scientificproduction";
        public PublicacionesDocumentos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
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
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha),
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

                // Autores TODO
                //Dictionary<string, string> listadoAutores = new Dictionary<string, string>();
                //listadoAutores.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubdocumentos));
                //listadoAutores.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.direccionTesisCodirectorTesisNombre));
                //listadoAutores.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.direccionTesisCodirectorTesisPrimerApellido));
                //listadoAutores.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisSegundoApellido));
                //UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoAutores, "060.010.010.040", keyValue.Value);

                // Traducciones
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion),
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

                // TODO quedan apartados por completar en la sección

                // ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.pubDocumentosPubISBN),
                    "060.010.010.160", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
