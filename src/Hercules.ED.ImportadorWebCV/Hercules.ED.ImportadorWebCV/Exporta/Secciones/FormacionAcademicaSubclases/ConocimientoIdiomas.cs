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
    public class ConocimientoIdiomas : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/languageSkills", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "languagecertificate";

        public ConocimientoIdiomas(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        /// <summary>
        /// Exporta los datos de la sección "020.060.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaConocimientoIdiomas(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp,[Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listadoIdentificadores != null)
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
                    Code = "020.060.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddLanguage(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasIdioma),
                    "020.060.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasComprensionAuditiva),
                    "020.060.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasComprensionLectura),
                    "020.060.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasInteraccionOral),
                    "020.060.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasExpresionOral),
                    "020.060.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.conocimientoIdiomasExpresionEscrita),
                    "020.060.000.160", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
