using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases
{
    public class ResultadosTecnologicos:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience",
            "http://w3id.org/roh/technologicalResults", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "technologicalresult";
        public ResultadosTecnologicos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        public void ExportaResultadosTecnologicos(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
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

            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
