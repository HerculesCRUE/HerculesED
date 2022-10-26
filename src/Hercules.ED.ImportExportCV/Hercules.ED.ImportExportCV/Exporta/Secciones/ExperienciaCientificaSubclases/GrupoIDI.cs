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
    public class GrupoIDI:SeccionBase
    {
        readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience",
            "http://w3id.org/roh/groups","http://w3id.org/roh/relatedGroupCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        readonly string graph = "group";
        public GrupoIDI(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "050.010.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaGrupoIDI(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
            foreach(KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "050.010.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIObjetoGrupo),
                    "050.010.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDINombreGrupo),
                    "050.010.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDICodNormalizado),
                    "050.010.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIPaisRadicacion), 
                    "050.010.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDICCAARadicacion),
                    "050.010.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDICiudadRadicacion),
                    "050.010.000.070", keyValue.Value);

                string numComponentes = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumComponentes))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumComponentes)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numComponentes))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.010.000.130", numComponentes);

                }

                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIFechaInicio),
                    "050.010.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,"050.010.000.150", keyValue.Value);
               
                string numTesisDirigidas = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumTesisDirigidas))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumTesisDirigidas)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numTesisDirigidas))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.010.000.170", numTesisDirigidas);

                }
                string numPosDocDirigidos = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumPosDocDirigidos))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.grupoIDINumPosDocDirigidos)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numPosDocDirigidos))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.010.000.180", numPosDocDirigidos);

                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIResultadosOtros),
                    "050.010.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIResultadosRelevantes),
                    "050.010.000.200", keyValue.Value);

                // propiedades_cv 
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIClaseColaboracion),
                    "050.010.000.160", keyValue.Value);

                // Autor
                Dictionary<string, string> listadoPropiedadesAutor = new Dictionary<string, string>();
                listadoPropiedadesAutor.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIFirmaIP));
                listadoPropiedadesAutor.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDINombreIP));
                listadoPropiedadesAutor.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIPrimerApellidoIP));
                listadoPropiedadesAutor.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDISegundoapellidoIP));
                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedadesAutor, "050.010.000.080", keyValue.Value);

                // Entidad
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDIEntidadAfiliacionNombre),
                    "050.010.000.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDITipoEntidadAfiliacion),
                    "050.010.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.grupoIDITipoEntidadAfiliacionOtros),
                    "050.010.000.120", keyValue.Value);


                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}

