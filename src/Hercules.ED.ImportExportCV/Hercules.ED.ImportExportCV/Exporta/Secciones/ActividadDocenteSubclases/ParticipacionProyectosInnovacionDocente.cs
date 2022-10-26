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
    public class ParticipacionProyectosInnovacionDocente : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/teachingProjects", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "teachingproject";
        public ParticipacionProyectosInnovacionDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "030.080.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaParticipacionProyectos(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                listadoIdentificadores = listadoIdentificadores.Where(x => listaId.Contains(x.Item2)).ToList();
                if (listadoIdentificadores.Count == 0)
                {
                    return;
                }
            }

            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.080.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTitulo),
                    "030.080.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaPaisEntidadRealizacion),
                    "030.080.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaCCAAEntidadRealizacion),
                    "030.080.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaCiudadEntidadRealizacion),
                    "030.080.000.250", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoParticipacion),
                    "030.080.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoParticipacionOtros),
                    "030.080.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAportacionProyecto),
                    "030.080.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaRegimenDedicacion),
                    "030.080.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoConvocatoria),
                    "030.080.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoConvocatoriaOtros),
                    "030.080.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoDuracionRelacionLaboral),
                    "030.080.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,
                    "030.080.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaFechaFinalizacionParticipacion),
                    "030.080.000.210", keyValue.Value);

                string numParticipantes = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaNumParticipantes))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaNumParticipantes)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numParticipantes))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.080.000.230", numParticipantes);
                }

                string importeConcedido = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaImporteConcedido))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.participacionInnovaImporteConcedido)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(importeConcedido))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.080.000.240", importeConcedido);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAmbitoProyecto),
                    "030.080.000.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaAmbitoProyectoOtros),
                    "030.080.000.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaFechaInicio),
                    "030.080.000.280", keyValue.Value);

                //Entidad financiadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaEntidadFinanciadoraNombre),
                    "030.080.000.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadora),
                    "030.080.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadoraOtros),
                    "030.080.000.120", keyValue.Value);

                //Entidad participante 
                List<Tuple<string, string, string>> dicCodigos = new List<Tuple<string, string, string>>();
                dicCodigos.Add(new Tuple<string, string, string>("EntityBean", "030.080.000.150",
                    UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaEntidadParticipanteNombre)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "030.080.000.170", 
                    UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "030.080.000.180", 
                    UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicCodigos,
                   "030.080.000.150", keyValue.Value,
                    UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante),
                    UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros));

                //Investigador principal
                Dictionary<string, string> listadoIP = new Dictionary<string, string>();
                listadoIP.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaFirmaIP));
                listadoIP.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaNombreIP));
                listadoIP.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaPrimerApellidoIP));
                listadoIP.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.participacionInnovaSegundoApellidoIP));

                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoIP,
                    "030.080.000.220",keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
