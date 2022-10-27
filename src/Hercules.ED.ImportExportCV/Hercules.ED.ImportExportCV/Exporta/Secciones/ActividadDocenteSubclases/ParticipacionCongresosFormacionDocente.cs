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
    public class ParticipacionCongresosFormacionDocente : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/teachingCongress", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "teachingcongress";

        public ParticipacionCongresosFormacionDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.090.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaParticipacionCongresos(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();

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
                    Code = "030.090.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoEvento),
                    "030.090.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoEventoOtros),
                    "030.090.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosNombreEvento),
                    "030.090.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPaisEvento),
                    "030.090.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosCCAAEvento),
                    "030.090.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosCiudadEvento),
                    "030.090.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPaisEntidadOrganizadora),
                    "030.090.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosCCAAEntidadOrganizadora),
                    "030.090.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosCiudadEntidadOrganizadora),
                    "030.090.000.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosObjetivosEvento),
                    "030.090.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPerfilDestinatarios),
                    "030.090.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosIdiomaPresentacion),
                    "030.090.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosFechaPresentacion),
                    "030.090.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoParticipacion),
                    "030.090.000.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoParticipacionOtros),
                    "030.090.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoPublicacion),
                    "030.090.000.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTituloPublicacion),
                    "030.090.000.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosNombrePublicacion),
                    "030.090.000.330", keyValue.Value);

                Dictionary<string, string> propiedadesVolNum = new Dictionary<string, string>();
                propiedadesVolNum.Add("Volumen", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosVolumenPublicacion));
                propiedadesVolNum.Add("Numero", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosNumeroPublicacion));
                UtilityExportar.AddCvnItemBeanCvnVolumeBean(itemBean, propiedadesVolNum,
                    "030.090.000.240", keyValue.Value);

                Dictionary<string, string> propiedadesPagIniPagFin = new Dictionary<string, string>();
                propiedadesPagIniPagFin.Add("PaginaInicial", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPagIniPublicacion));
                propiedadesPagIniPagFin.Add("PaginaFinal", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPagFinalPublicacion));
                UtilityExportar.AddCvnItemBeanCvnPageBean(itemBean, propiedadesPagIniPagFin,
                    "030.090.000.250", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosEditorialPublicacion),
                    "030.090.000.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosPaisPublicacion),
                    "030.090.000.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosCCAAPublicacion),
                    "030.090.000.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosFechaPublicacion),
                    "030.090.000.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosURLPublicacion),
                    "030.090.000.310", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosDepositoLegalPublicacion),
                   "030.090.000.320", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDurationHours(itemBean,
                    "030.090.000.340", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosFechaInicioPublicacion),
                    "030.090.000.350", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosFechaFinalPublicacion),
                    "030.090.000.360", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosAutorCorrespondencia),
                    "030.090.000.400", keyValue.Value);

                //Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosEntidadOrganizadoraNombre),
                    "030.090.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadora),
                    "030.090.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadoraOtros),
                    "030.090.000.110", keyValue.Value);

                //ISBN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosISBNPublicacion),
                   "030.090.000.180", keyValue.Value);

                //ISSN
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosISSNPublicacion),
                   "030.090.000.180", keyValue.Value);

                //ID publicacion
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosIDPubDigitalHandle),
                    "030.090.000.370", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosIDPubDigitalDOI),
                    "030.090.000.370", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosIDPubDigitalPMID),
                    "030.090.000.370", keyValue.Value);

                Dictionary<string, string> dicNombreID = new Dictionary<string, string>();
                dicNombreID.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosNombreOtroIDPubDigital));
                dicNombreID.Add("ID", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participaCongresosIDOtroPubDigital));
                UtilityExportar.AddCvnItemBeanCvnExternalPKBeanOthers(itemBean, dicNombreID, "030.090.000.370", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
