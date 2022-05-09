﻿using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        public void ExportaEstudiosCiclos(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
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

            }
        }
    }
}
