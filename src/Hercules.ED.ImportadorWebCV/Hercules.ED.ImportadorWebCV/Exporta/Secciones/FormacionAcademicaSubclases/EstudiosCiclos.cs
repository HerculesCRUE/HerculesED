﻿using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class EstudiosCiclos : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/firstSecondCycles", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "academicdegree";

        public EstudiosCiclos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        /// <summary>
        /// Exporta los datos de la sección "020.010.010.000" a cvn.cvnRootResultBean
        /// </summary>
        public void ExportaEstudiosCiclos(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null)
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
                    Code = "020.010.010.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloPaisEntidadTitulacion),
                    "020.010.010.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloCCAAEntidadTitulacion),
                    "020.010.010.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloCiudadEntidadTitulacion),
                    "020.010.010.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloFechaTitulacion),
                    "020.010.010.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloNotaMedia),
                    "020.010.010.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloFechaHomologacion),
                    "020.010.010.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTituloHomologado),
                    "020.010.010.180", keyValue.Value);

                //Tipo titulacion
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTipoTitulacion),
                    "020.010.010.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTipoTitulacionOtros),
                    "020.010.010.020", keyValue.Value);

                //Premio
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloPremio),
                    "020.010.010.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloPremioOtros),
                    "020.010.010.200", keyValue.Value);

                //Titulacion
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTitulo),
                    UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloNombreTitulo), "020.010.010.030", keyValue.Value);

                //Titulo extrajero
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTituloExtranjero),
                    UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTituloExtranjeroNombre), "020.010.010.150", keyValue.Value);

                //Entidad titulacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloEntidadTitulacionNombre)
                    , "020.010.010.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacion),
                    "020.010.010.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacionOtros),
                    "020.010.010.120", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
