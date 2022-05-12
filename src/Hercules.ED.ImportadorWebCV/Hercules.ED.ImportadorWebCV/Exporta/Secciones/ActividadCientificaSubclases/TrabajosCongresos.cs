﻿using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class TrabajosCongresos : SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/worksSubmittedConferences", "http://w3id.org/roh/relatedWorkSubmittedConferencesCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "document";

        public TrabajosCongresos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        public void ExportaTrabajosCongresos(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntityCV(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "060.010.020.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo),
                    "060.010.020.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubActa),
                    "060.010.020.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosFormaContribucion),
                    "060.010.020.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubTitulo),
                    "060.010.020.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNombre),
                    "060.010.020.370", keyValue.Value);

                Dictionary<string, string> propiedadesPubVol = new Dictionary<string, string>();
                propiedadesPubVol.Add("Volumen", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubVolumen));
                propiedadesPubVol.Add("Numero", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNumero));
                UtilityExportar.AddCvnItemBeanCvnVolumeBean(itemBean, propiedadesPubVol, "060.010.020.240", keyValue.Value);

                Dictionary<string, string> propiedadesPagIniPagFin = new Dictionary<string, string>();
                propiedadesPagIniPagFin.Add("PaginaInicial", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagIni));
                propiedadesPagIniPagFin.Add("PaginaFinal", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagFin));
                UtilityExportar.AddCvnItemBeanCvnPageBean(itemBean, propiedadesPagIniPagFin, "060.010.020.250", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPais),
                    "060.010.020.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubEditorial),
                    "060.010.020.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubCCAA),
                    "060.010.020.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubFecha),
                    "060.010.020.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubURL),
                    "060.010.020.310", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPubDepositoLegal),
                    "060.010.020.330", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosNombreCongreso),
                    "060.010.020.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaCelebracion),
                    "060.010.020.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaFin),
                    "060.010.020.380", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadCelebracion),
                    "060.010.020.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisCelebracion),
                    "060.010.020.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAACelebracion),
                    "060.010.020.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEvento),
                    "060.010.020.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEventoOtros),
                    "060.010.020.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosComiteExterno),
                    "060.010.020.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeo),
                    "060.010.020.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeoOtros),
                    "060.010.020.090", keyValue.Value);
                // Propiedades cv
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoParticipacion),
                    "060.010.020.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencion),
                    "060.010.020.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencionOtros),
                    "060.010.020.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosAutoCorrespondencia),
                    "060.010.020.390", keyValue.Value);

                // Lista de autores

                //Selecciono las firmas de las personas
                Dictionary<string, string> dicFirmas = UtilityExportar.GetFirmasAutores(Variables.ActividadCientificaTecnologica.trabajosCongresosMiembrosAutorFirma, keyValue.Value);

                //Selecciono los autores
                List<Tuple<string, string, string>> autorNombreApellido = UtilityExportar.GetNombreApellidoAutor(Variables.ActividadCientificaTecnologica.trabajosCongresosMiembrosAutor, keyValue.Value, mResourceApi);
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanListSimple(itemBean, autorNombreApellido, dicFirmas,
                    "060.010.020.040");

                // Trabajos Congresos ID Publicacion
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalHandle),
                    "060.010.020.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalDOI),
                    "060.010.020.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalPMID),
                    "060.010.020.400", keyValue.Value);

                Dictionary<string, string> dicNombreID = new Dictionary<string, string>();
                dicNombreID.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosIDOtroPubDigital));
                dicNombreID.Add("ID", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosNombreOtroPubDigital));
                UtilityExportar.AddCvnItemBeanCvnExternalPKBeanOthers(itemBean, dicNombreID, "060.010.020.400", keyValue.Value);

                //ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasActDivulPubISBN),
                    "060.010.020.320", keyValue.Value);

                //ISSN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasActDivulPubISSN),
                    "060.010.020.320", keyValue.Value);

                // Entidad Organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosEntidadOrganizadoraNombre),
                    "060.010.020.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadora),
                    "060.010.020.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadoraOtros),
                    "060.010.020.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadEntidadOrganizadora),
                    "060.010.020.360", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisEntidadOrganizadora),
                    "060.010.020.340", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAAEntidadOrganizadora),
                    "060.010.020.350", keyValue.Value);
                listado.Add(itemBean);
            }


            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
