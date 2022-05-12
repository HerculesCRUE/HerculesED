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
    public class OtrosModosColaboracion:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherCollaborations", 
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "collaboration";
        public OtrosModosColaboracion(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        public void ExportaOtrosModosColaboracion(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
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
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,"060.020.020.130" ,keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion),
                    "060.020.020.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabResultadosRelevantes),
                    "060.020.020.150", keyValue.Value);


                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabFirmaInvestigador));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabNombreInvestigador));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPrimApellInvestigador));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabSegApellInvestigador));
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean,listadoPropiedadesAutor, "060.020.020.070", keyValue.Value);
                
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.otrasColabPalabrasClave),
                    "060.020.020.160", keyValue.Value);

                listado.Add(itemBean);

            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}

