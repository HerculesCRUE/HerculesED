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
    public class TrabajosJornadasSeminarios:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/worksSubmittedSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "document";
        public TrabajosJornadasSeminarios(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        public void ExportaTrabajosJornadasSeminarios(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean();
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

                // Rango
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeo),
                    "060.010.030.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeoOtros),
                    "060.010.030.390", keyValue.Value);

                // Autores 060.010.030.310 TODO

                // IDPublicación 060.010.030.400 TODO

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
