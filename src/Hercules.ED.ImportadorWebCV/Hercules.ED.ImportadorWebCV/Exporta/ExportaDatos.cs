using ExportadorWebCV.Utils;
using ImportadorWebCV.Exporta.Secciones;
using ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases;
using ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases;
using ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases;
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
            //List<CvnItemBean> listado = new List<CvnItemBean>();

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

            EstudiosCiclos estudiosCiclos = new EstudiosCiclos(cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(entity, seccion);

            Doctorados doctorados = new Doctorados(cvn, cvID);

            OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado(cvn, cvID);

            FormacionEspecializada formacionEspecializada = new FormacionEspecializada(cvn, cvID);

            CursosMejoraDocente cursosMejora = new CursosMejoraDocente(cvn, cvID);

            ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas(cvn, cvID);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();

            AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes(cvn, cvID);

            CursosSeminarios cursosSeminarios = new CursosSeminarios(cvn, cvID);

            DireccionTesis direccionTesis = new DireccionTesis(cvn, cvID);

            FormacionAcademicaSubclase formacionAcademica = new FormacionAcademicaSubclase(cvn, cvID);

            OtrasActividades otrasActividades = new OtrasActividades(cvn, cvID);

            ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente(cvn, cvID);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente(cvn, cvID);

            PremiosInnovacionDocente premiosInnovacionDocente = new PremiosInnovacionDocente(cvn, cvID);

            PublicacionesDocentes publicacionesDocentes = new PublicacionesDocentes(cvn, cvID);

            TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas(cvn, cvID);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();

            Contratos contratos = new Contratos(cvn, cvID);

            GrupoIDI grupoIDI = new GrupoIDI(cvn, cvID);

            ObrasArtisticas obrasArtisticas = new ObrasArtisticas(cvn, cvID);

            PropiedadIndustrialIntelectual propII = new PropiedadIndustrialIntelectual(cvn, cvID);

            ProyectosIDI proyectosIDI = new ProyectosIDI(cvn, cvID);

            ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos(cvn, cvID);

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
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);
            textoLibre.ExportaTextoLibre(entity);

        }
    }
}
