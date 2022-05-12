using ExportadorWebCV.Utils;
using ImportadorWebCV.Exporta.Secciones;
using ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases;
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

            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn,cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/professionalSituation";

            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn,cvID);
            situacionProfesional.ExportaSituacionProfesional(entity, seccion);

            CargosActividades cargosActividades = new CargosActividades(cvn, cvID);
            cargosActividades.ExportaCargosActividades(entity, seccion);

        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/qualifications";

            EstudiosCiclos estudiosCiclos = new EstudiosCiclos(cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(entity, seccion);

            Doctorados doctorados = new Doctorados(cvn, cvID);
            doctorados.ExportaDoctorados(entity, seccion);

            OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado(cvn, cvID);
            otraFormacionPosgrado.ExportaOtraFormacionPosgrado(entity, seccion);

            FormacionEspecializada formacionEspecializada = new FormacionEspecializada(cvn, cvID);
            formacionEspecializada.ExportaFormacionEspecializada(entity, seccion);

            CursosMejoraDocente cursosMejora = new CursosMejoraDocente(cvn, cvID);
            cursosMejora.ExportaCursosMejoraDocente(entity, seccion);

            ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas(cvn, cvID);
            conocimientoIdiomas.ExportaConocimientoIdiomas(entity, seccion);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";

            AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes(cvn, cvID);
            aportacionesRelevantes.ExportaAportacionesRelevantes(entity,seccion);

            CursosSeminarios cursosSeminarios = new CursosSeminarios(cvn, cvID);
            cursosSeminarios.ExportaCursosSeminarios(entity, seccion);

            DireccionTesis direccionTesis = new DireccionTesis(cvn, cvID);
            direccionTesis.ExportaDireccionTesis(entity, seccion);

            FormacionAcademicaSubclase formacionAcademica = new FormacionAcademicaSubclase(cvn, cvID);
            formacionAcademica.ExportaFormacionAcademica(entity, seccion);

            OtrasActividades otrasActividades = new OtrasActividades(cvn, cvID);
            otrasActividades.ExportaOtrasActividades(entity, seccion);

            ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente(cvn, cvID);
            participacionCongresos.ExportaParticipacionCongresos(entity, seccion);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente(cvn, cvID);
            participacionProyectos.ExportaParticipacionProyectos(entity, seccion);

            PremiosInnovacionDocente premiosInnovacionDocente = new PremiosInnovacionDocente(cvn, cvID);
            premiosInnovacionDocente.ExportaPremiosInnovacionDocente(entity, seccion);

            PublicacionesDocentes publicacionesDocentes = new PublicacionesDocentes(cvn, cvID);
            publicacionesDocentes.ExportaPublicacionesDocentes(entity, seccion);

            TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas(cvn, cvID);
            tutoriasAcademicas.ExportaTutoriasAcademicas(entity, seccion);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";

            Contratos contratos = new Contratos(cvn, cvID);

            GrupoIDI grupoIDI = new GrupoIDI(cvn, cvID);
            grupoIDI.ExportaGrupoIDI(entity, seccion);

            ObrasArtisticas obrasArtisticas = new ObrasArtisticas(cvn, cvID);
            obrasArtisticas.ExportaObrasArtisticas(entity, seccion);

            PropiedadIndustrialIntelectual propII = new PropiedadIndustrialIntelectual(cvn, cvID);
            propII.ExportaPropiedadII(entity, seccion);

            ProyectosIDI proyectosIDI = new ProyectosIDI(cvn, cvID);
            proyectosIDI.ExportaProyectosIDI(entity, seccion);

            ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos(cvn, cvID);
            resultadosTecnologicos.ExportaResultadosTecnologicos(entity, seccion);

        }
        public void ExportaActividadCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion(cvn, cvID);
            otrasActividadesDivulgacion.ExportaOtrasActividadesDivulgacion(entity, seccion);

            ForosComites forosComites = new ForosComites(cvn, cvID);
            forosComites.ExportaForosComites(entity, seccion);

            OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI(cvn, cvID);
            organizacionesIDI.ExportaOrganizacionesIDI(entity, seccion);

            TrabajosCongresos trabajosCongresos = new TrabajosCongresos(cvn, cvID);
            trabajosCongresos.ExportaTrabajosCongresos(entity, seccion);

            RedesCooperacion redesCooperacion = new RedesCooperacion(cvn, cvID);
            redesCooperacion.ExportaRedesCooperacion(entity, seccion);

            PremiosMenciones premiosMenciones = new PremiosMenciones(cvn, cvID);
            premiosMenciones.ExportaPremiosMenciones(entity, seccion);

            AyudaBecas ayudaBecas = new AyudaBecas(cvn, cvID);
            ayudaBecas.ExportaAyudaBecas(entity, seccion);

            OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion(cvn, cvID);
            otrosModosColaboracion.ExportaOtrosModosColaboracion(entity, seccion);

            AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos(cvn, cvID);
            acreditacionesReconocimientos.ExportaAcreditacionesReconocimientos(entity, seccion);

            EstanciasIDI estanciasIDI = new EstanciasIDI(cvn, cvID);
            estanciasIDI.ExportaEstanciasIDI(entity, seccion);

            GestionIDI gestionIDI = new GestionIDI(cvn, cvID);
            gestionIDI.ExportaGestionIDI(entity, seccion);

            EvalRevIDI evalRevIDI = new EvalRevIDI(cvn, cvID);
            evalRevIDI.ExportaEvalRevIDI(entity, seccion);

            PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos(cvn, cvID);
            publicacionesDocumentos.ExportaPublicacionesDocumentos(entity, seccion);

            ProduccionCientifica produccionCientifica = new ProduccionCientifica(cvn, cvID);
            produccionCientifica.ExportaProduccionCientifica(entity, seccion);

            ComitesCTA comitesCTA = new ComitesCTA(cvn, cvID);
            comitesCTA.ExportaComitesCTA(entity, seccion);

            SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones(cvn, cvID);
            sociedadesAsociaciones.ExportaSociedadesAsociaciones(entity, seccion);

            Consejos consejos = new Consejos(cvn, cvID);
            consejos.ExportaConsejos(entity, seccion);

            TrabajosJornadasSeminarios trabajosJornadasSeminarios = new TrabajosJornadasSeminarios(cvn, cvID);
            trabajosJornadasSeminarios.ExportaTrabajosJornadasSeminarios(entity, seccion);

            OtrasDistinciones otrasDistinciones = new OtrasDistinciones(cvn, cvID);
            otrasDistinciones.ExportaOtrasDistinciones(entity, seccion);

            PeriodosActividad periodosActividad = new PeriodosActividad(cvn, cvID);
            periodosActividad.ExportaPeriodosActividad(entity, seccion);

            OtrosMeritos otrosMeritos = new OtrosMeritos(cvn, cvID);
            otrosMeritos.ExportaOtrosMeritos(entity, seccion);

        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);
            textoLibre.ExportaTextoLibre(entity);
        }
    }
}
