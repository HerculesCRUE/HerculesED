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
    public class ResultadosTecnologicos:SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience",
            "http://w3id.org/roh/technologicalResults", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "technologicalresult";
        public ResultadosTecnologicos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "050.030.020.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaResultadosTecnologicos(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                    Code = "050.030.020.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDescripcion),
                    "050.030.020.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosGradoContribucion),
                    "050.030.020.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosGradoContribucionOtros),
                    "050.030.020.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosNuevasTecnicasEquip),
                    "050.030.020.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEmpresasSpinOff),
                    "050.030.020.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosResultadosMejoraProd),
                    "050.030.020.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosHomologos),
                    "050.030.020.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosExpertoTecnologico),
                    "050.030.020.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosConveniosColab),
                    "050.030.020.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosAmbitoActividad),
                    "050.030.020.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosAmbitoActividadOtros),
                    "050.030.020.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosFechaInicio),
                    "050.030.020.250", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "050.030.020.260", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosResultadosRelevantes),
                    "050.030.020.270", keyValue.Value);

                // Autores
                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPFirma));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPNombre));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPPrimerApellido));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPSegundoApellido));
                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedadesAutor, "050.030.020.050", keyValue.Value);

                // Corresponsables
                Dictionary<string, string> listadoPropiedadesCorresponsable = new Dictionary<string, string>();
                listadoPropiedadesCorresponsable.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPFirma));
                listadoPropiedadesCorresponsable.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPNombre));
                listadoPropiedadesCorresponsable.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPPrimerApellido));
                listadoPropiedadesCorresponsable.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPSegundoApellido));
                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedadesCorresponsable, "050.030.020.060", keyValue.Value);

                // Entidad colaboradora 
                List<Tuple<string, string, string>> dicColaboradora = new List<Tuple<string, string, string>>();
                dicColaboradora.Add(new Tuple<string, string, string>("EntityBean", "050.030.020.170",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadColaboradoraNombre)));
                dicColaboradora.Add(new Tuple<string, string, string>("String", "050.030.020.190",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosTipoEntidadColaboradora)));
                dicColaboradora.Add(new Tuple<string, string, string>("String", "050.030.020.200",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosTipoEntidadColaboradoraOtros)));
                dicColaboradora.Add(new Tuple<string, string, string>("String", "050.030.020.320",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosPaisEntidadColaboradora)));
                dicColaboradora.Add(new Tuple<string, string, string>("String", "050.030.020.330",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCCAAEntidadColaboradora)));
                dicColaboradora.Add(new Tuple<string, string, string>("String", "050.030.020.340",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCiudadEntidadColaboradora)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicColaboradora,
                   "050.030.020.170", keyValue.Value);

                // Entidad destinataria
                List<Tuple<string, string, string>> dicDestinataria = new List<Tuple<string, string, string>>();
                dicDestinataria.Add(new Tuple<string, string, string>("EntityBean", "050.030.020.210",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadDestinatariaNombre)));
                dicDestinataria.Add(new Tuple<string, string, string>("String", "050.030.020.230",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosTipoEntidadDestinataria)));
                dicDestinataria.Add(new Tuple<string, string, string>("String", "050.030.020.240",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosTipoEntidadDestinatariaOtros)));
                dicDestinataria.Add(new Tuple<string, string, string>("String", "050.030.020.290",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosPaisEntidadDestinataria)));
                dicDestinataria.Add(new Tuple<string, string, string>("String", "050.030.020.300",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCCAAEntidadDestinataria)));
                dicDestinataria.Add(new Tuple<string, string, string>("String", "050.030.020.310",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCiudadEntidadDestinataria)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicDestinataria,
                   "050.030.020.210", keyValue.Value);

                // Códigos Unesco
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoPrimaria),
                    "050.030.020.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoSecundaria),
                    "050.030.020.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoTerciaria),
                    "050.030.020.040", keyValue.Value);

                // Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosPalabrasClave),
                    "050.030.020.280", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
