using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class TrabajosJornadasSeminarios:SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/worksSubmittedSeminars", "http://w3id.org/roh/relatedWorkSubmittedSeminarsCV", 
            "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "document";
        public TrabajosJornadasSeminarios(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.010.030.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaTrabajosJornadasSeminarios(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
       {
            List<CvnItemBean> listado = new ();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                listadoIdentificadores = listadoIdentificadores.Where(x => listaId.Contains(x.Item3)).ToList();
                if (listadoIdentificadores.Count == 0)
                {
                    return;
                }
            }

            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntityCV(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new ();
                itemBean.Code = "060.010.030.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo),
                    "060.010.030.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongreso),
                    "060.010.030.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTipo),
                    "060.010.030.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTitulo),
                    "060.010.030.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNombre),
                    "060.010.030.350", keyValue.Value);

                Dictionary<string, string> propVolNum = new Dictionary<string, string>();
                propVolNum.Add("Volumen", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubVolumen));
                propVolNum.Add("Numero", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNumero));
                UtilityExportar.AddCvnItemBeanCvnVolumeBean(itemBean, propVolNum, "060.010.030.210", keyValue.Value);

                Dictionary<string, string> propiedadesPagIniPagFin = new Dictionary<string, string>();
                propiedadesPagIniPagFin.Add("PaginaInicial", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagIni));
                propiedadesPagIniPagFin.Add("PaginaFinal", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagFin));
                UtilityExportar.AddCvnItemBeanCvnPageBean(itemBean, propiedadesPagIniPagFin, "060.010.030.220", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPais),
                    "060.010.030.240", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubEditorial),
                    "060.010.030.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubCCAA),
                    "060.010.030.250", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFecha),
                    "060.010.030.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubURL),
                    "060.010.030.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubDepositoLegal),
                    "060.010.030.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemNombreEvento),
                    "060.010.030.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemFechaCelebracion),
                    "060.010.030.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFechaFinCelebracion),
                    "060.010.030.370", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadCelebracion),
                    "060.010.030.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisCelebracion),
                    "060.010.030.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAACelebracion),
                    "060.010.030.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEvento),
                    "060.010.030.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEventoOtros),
                    "060.010.030.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongresoExterno),
                    "060.010.030.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeo),
                    "060.010.030.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeoOtros),
                    "060.010.030.060", keyValue.Value);

                // properties_cv
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemIntervencion),
                    "060.010.030.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorCorrespondencia),
                    "060.010.030.390", keyValue.Value);

                // Autores
                //Selecciono las firmas de las personas
                Dictionary<string, string> dicFirmas = UtilityExportar.GetFirmasAutores(Variables.ActividadCientificaTecnologica.trabajosJornSemAutoresFirma, keyValue.Value);

                //Selecciono los autores
                List<Tuple<string, string, string>> autorNombreApellido = UtilityExportar.GetNombreApellidoAutor(Variables.ActividadCientificaTecnologica.trabajosJornSemAutores, keyValue.Value, mResourceApi);
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanListSimple(itemBean, autorNombreApellido, dicFirmas,
                    "060.010.030.310");

                // IDPublicación
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalHandle),
                    "060.010.030.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalDOI),
                    "060.010.030.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalPMID),
                    "060.010.030.400", keyValue.Value);
                Dictionary<string, string> dicNombreID = new Dictionary<string, string>();
                dicNombreID.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemNombreOtroPubDigital));
                dicNombreID.Add("ID", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemIDOtroPubDigital));
                UtilityExportar.AddCvnItemBeanCvnExternalPKBeanOthers(itemBean, dicNombreID, "060.010.030.400", keyValue.Value);

                // ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubISBN),
                    "060.010.030.290", keyValue.Value);

                // ISSN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPubISSN),
                    "060.010.030.290", keyValue.Value);

                // Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemEntidadOrganizadoraNombre),
                    "060.010.030.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadora),
                    "060.010.030.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadoraOtros),
                    "060.010.030.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisEntidadOrganizadora),
                    "060.010.030.320", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAAEntidadOrganizadora),
                    "060.010.030.330", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadEntidadOrganizadora),
                    "060.010.030.340", keyValue.Value);
                listado.Add(itemBean);

            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
