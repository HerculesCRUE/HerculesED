using ExportadorWebCV.Utils;
using ImportadorWebCV.Exporta.Secciones;
using ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImportadorWebCV.Exporta
{
    public class ExportaDatos : SeccionBase
    {
        private string cvID;
        private cvnRootResultBean cvn;

        public ExportaDatos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            this.cvID = cvID;
            this.cvn = cvn;
        }

        public void ExportaDatosIdentificacion(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/personalData";
            List<CvnItemBean> listado = new List<CvnItemBean>();

            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn,cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/professionalSituation";
            //List<CvnItemBean> listado = new List<CvnItemBean>();

            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn,cvID);
            situacionProfesional.ExportaSituacionProfesional(entity, seccion);

            CargosActividades cargosActividades = new CargosActividades(cvn,cvID);
            cargosActividades.ExportaCargosActividades(entity, seccion);

            ////Añado en el cvnRootResultBean los items que forman parte del listado
            //UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/qualifications";
            List<CvnItemBean> listado = new List<CvnItemBean>();



            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();




            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();


            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaActividadCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            List<CvnItemBean> listado = new List<CvnItemBean>();


            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string propResumenLibre = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.resumenLibre)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFG = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b1DescripcionTFG)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFM = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b2DescripcionTFM)).Select(x => x.prop).FirstOrDefault());

            List<CvnItemBean> listado = new List<CvnItemBean>();
            CvnItemBean itemBean = new CvnItemBean()
            {
                Code = "070.000.000.000"
            };

            if (itemBean.Items == null)
            {
                itemBean.Items = new List<CVNObject>();
            }

            //Selecciono el ultimo valor que se corresponde a la propiedad en caso de que esta exista.
            string resumenLibre = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFG = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFM = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;

            //Separación de los diferentes apartados por los titulos del FECYT. 
            string resumen = resumenLibre + " B.1. Breve descripción del Trabajo de Fin de Grado (TFG) y puntuación obtenida"
                + resumenTFG + " B.2. Breve descripción del Trabajo de Fin de Máster (TFM) y puntuación obtenida" + resumenTFM;

            UtilityExportar.AddCvnItemBeanCvnRichText(itemBean, resumen, "070.010.000.000");

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
    }
}
