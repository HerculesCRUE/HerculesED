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
    public class Contratos : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience",
            "http://w3id.org/roh/nonCompetitiveProjects", "http://w3id.org/roh/relatedNonCompetitiveProjectCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "project";
        public Contratos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "050.020.020.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaContratos(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
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
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "050.020.020.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosNombreProyecto),
                    "050.020.020.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosModalidadProyecto),
                    "050.020.020.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosAmbitoProyecto),
                    "050.020.020.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosAmbitoProyectoOtros),
                    "050.020.020.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosCodEntidadFinanciadora),
                    "050.020.020.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosTipoProyecto),
                    "050.020.020.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosNombrePrograma),
                    "050.020.020.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosFechaInicio),
                    "050.020.020.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "050.020.020.190", keyValue.Value);

                string numCuantiaTotal = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosCuantiaTotal))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosCuantiaTotal)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numCuantiaTotal))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.200", numCuantiaTotal);
                }

                string numInvestigadores = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosNumInvestigadores))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosNumInvestigadores)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numInvestigadores))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.260", numInvestigadores);
                }

                string numPersonasAnio = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosNumPersonasAnio))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosNumPersonasAnio)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numPersonasAnio))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.270", numPersonasAnio);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosResultadosRelevantes),
                    "050.020.020.300", keyValue.Value);

                // propiedades_cv
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosGradoContribucion),
                    "050.020.020.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosGradoContribucionOtros),
                    "050.020.020.290", keyValue.Value);

                // Financiacion
                string numCuantiaSubproyecto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosCuantiaSubproyecto))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosCuantiaSubproyecto)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numCuantiaSubproyecto))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.210", numCuantiaSubproyecto);
                }

                string numPorcentajeSubvencion = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeSubvencion))) ?
                   keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeSubvencion)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                   : null;
                if (!string.IsNullOrEmpty(numPorcentajeSubvencion))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.220", numPorcentajeSubvencion);
                }

                string numPorcentajeCredito = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeCredito))) ?
                   keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeCredito)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                   : null;
                if (!string.IsNullOrEmpty(numPorcentajeCredito))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.230", numPorcentajeCredito);
                }

                string numPorcentajeMixto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeMixto))) ?
                   keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeMixto)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                   : null;
                if (!string.IsNullOrEmpty(numPorcentajeMixto))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.020.240", numPorcentajeMixto);
                }

                // Entidad Realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosEntidadRealizacionNombre),
                    "050.020.020.370", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadRealizacion),
                    "050.020.020.330", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadRealizacionOtros),
                    "050.020.020.380", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosPaisEntidadRealizacion),
                    "050.020.020.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosCCAAEntidadRealizacion),
                    "050.020.020.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosCiudadEntidadRealizacion),
                    "050.020.020.090", keyValue.Value);

                // Entidad Participante
                UtilityExportar.AddCvnItemBeanCvnEntityBeanList(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosEntidadParticipanteNombre),
                    "050.020.020.320", keyValue.Value);

                // Autores
                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Orden", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosIPOrden));
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosIPFirma));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosIPNombre));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosIPPrimerApellido));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosIPSegundoApellido));
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoPropiedadesAutor, "050.020.020.250", keyValue.Value);

                // Entidad Financiadora
                List<Tuple<string, string, string>> dicCodigos = new List<Tuple<string, string, string>>();
                dicCodigos.Add(new Tuple<string, string, string>("EntityBean", "050.020.020.120",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosEntidadFinanciadoraNombre)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "050.020.020.140",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadFinanciadora)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "050.020.020.150",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadFinanciadoraOtros)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "050.020.020.350",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosPaisEntidadFinanciadora)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "050.020.020.360",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosCCAAEntidadFinanciadora)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "050.020.020.340",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.contratosCiudadEntidadFinanciadora)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicCodigos,
                   "050.020.020.120", keyValue.Value);


                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
