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
    public class OtrosModosColaboracion:SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/otherCollaborations", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "collaboration";
        public OtrosModosColaboracion(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.020.020.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaOtrosModosColaboracion(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "060.020.020.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabModoRelacion),
                    "060.020.020.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabModoRelacionOtros),
                    "060.020.020.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPaisRadicacion),
                    "060.020.020.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabCCAARadicacion),
                    "060.020.020.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabCiudadRadicacion),
                    "060.020.020.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabFechaInicio),
                    "060.020.020.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,
                    "060.020.020.130" ,keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion),
                    "060.020.020.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabResultadosRelevantes),
                    "060.020.020.150", keyValue.Value);

                // Autores
                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Orden", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabOrdenInvestigador));
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabFirmaInvestigador));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabNombreInvestigador));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPrimApellInvestigador));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabSegApellInvestigador));
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean,listadoPropiedadesAutor, "060.020.020.070", keyValue.Value);

                // Entidades Participantes
                List<Tuple<string, string, string>> dicCodigos = new List<Tuple<string, string, string>>();
                dicCodigos.Add(new Tuple<string, string, string>("EntityBean", "060.020.020.080",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantesNombre)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.020.020.170",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPaisEntidadParticipante)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.020.020.180",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabCCAAEntidadParticipante)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.020.020.190",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabCiudadEntidadParticipante)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.020.020.100",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad)));
                dicCodigos.Add(new Tuple<string, string, string>("String", "060.020.020.110",
                    UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicCodigos,
                   "060.020.020.080", keyValue.Value);

                // Palabras Clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPalabrasClave),
                    "060.020.020.160", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}