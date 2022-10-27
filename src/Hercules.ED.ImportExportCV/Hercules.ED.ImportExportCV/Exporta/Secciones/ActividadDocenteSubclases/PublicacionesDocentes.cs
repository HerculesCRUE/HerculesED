using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class PublicacionesDocentes : SeccionBase
    {
        private readonly List<string> propiedadesItem = new() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/teachingPublications", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "teachingpublication";

        public PublicacionesDocentes(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.070.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaPublicacionesDocentes( Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadores, listaId))
            {
                return;
            }


            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new ()
                {
                    Code = "030.070.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteNombre),
                    "030.070.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocentePerfilDestinatario),
                    "030.070.000.020", keyValue.Value);

                string posicionFirma = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.publicacionDocentePosicionFirma))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.publicacionDocentePosicionFirma)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(posicionFirma))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean,"030.070.000.040", posicionFirma);
                }
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteFechaElaboracion),
                    "030.070.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteTipologiaSoporte),
                    "030.070.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteTipologiaSoporteOtros),
                    "030.070.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteTituloPublicacion),
                    "030.070.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteNombrePublicacion),
                    "030.070.000.190", keyValue.Value);

                Dictionary<string, string> propVolNum = new Dictionary<string, string>();
                propVolNum.Add("Volumen", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteVolumenPublicacion));
                propVolNum.Add("Numero", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteNumeroPublicacion));
                UtilityExportar.AddCvnItemBeanCvnVolumeBean(itemBean, propVolNum, "030.070.000.090", keyValue.Value);

                Dictionary<string, string> propiedadesPagIniPagFin = new Dictionary<string, string>();
                propiedadesPagIniPagFin.Add("PaginaInicial", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocentePagIniPublicacion));
                propiedadesPagIniPagFin.Add("PaginaFinal", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocentePagFinalPublicacion));
                UtilityExportar.AddCvnItemBeanCvnPageBean(itemBean, propiedadesPagIniPagFin,
                    "030.070.000.100", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteEditorialPublicacion),
                    "030.070.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocentePaisPublicacion),
                    "030.070.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteCCAAPublicacion),
                    "030.070.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteFechaPublicacion),
                    "030.070.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteURLPublicacion),
                    "030.070.000.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteDepositoLegal),
                    "030.070.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteJustificacionMaterial),
                    "030.070.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteGradoContribucion),
                    "030.070.000.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorCorrespondencia),
                    "030.070.000.220", keyValue.Value);

                //ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteISBNPublicacion),
                    "030.070.000.170", keyValue.Value);

                //ISSN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteISSNPublicacion),
                    "030.070.000.170", keyValue.Value);

                //ID publicacion
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteIDPubDigitalHandle),
                    "030.070.000.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteIDPubDigitalDOI),
                    "030.070.000.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteIDPubDigitalPMID),
                    "030.070.000.230", keyValue.Value);

                Dictionary<string, string> dicNombreID = new Dictionary<string, string>();
                dicNombreID.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteNombreOtroIDPubDigital));
                dicNombreID.Add("ID", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteIDOtroPubDigital));
                UtilityExportar.AddCvnItemBeanCvnExternalPKBeanOthers(itemBean, dicNombreID, "030.070.000.230", keyValue.Value);

                //Autores
                Dictionary<string, string> listadoIP = new Dictionary<string, string>();
                listadoIP.Add("Orden", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorOrden));
                listadoIP.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorFirma));
                listadoIP.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorNombre));
                listadoIP.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorPrimerApellido));
                listadoIP.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.publicacionDocenteAutorSegundoApellido));

                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoIP,
                    "030.070.000.030", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
