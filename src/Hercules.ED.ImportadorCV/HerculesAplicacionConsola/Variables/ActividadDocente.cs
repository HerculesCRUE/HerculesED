using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Variables
{
    class ActividadDocente
    {
        /// <summary>
        /// Dirección de tesis doctorales y/o proyectos fin de carrera - 030.040.000.000
        /// </summary>
        public const string direccionTesisTipoProyecto = "";
        public const string direccionTesisTipoProyectoOtros = "";
        public const string direccionTesisTituloTrabajo = "";
        public const string direccionTesisCodirectorTesis = "";
        public const string direccionTesisPaisEntidadRealizacion = "";
        public const string direccionTesisCCAAEntidadRealizacion = "";
        public const string direccionTesisCiudadEntidadRealizacion = "";
        public const string direccionTesisEntidadRealizacion = "";
        public const string direccionTesisTipoEntidadRealizacion = "";
        public const string direccionTesisTipoEntidadRealizacionOtros = "";
        public const string direccionTesisAlumno = "";
        public const string direccionTesisPalabrasClave = "";
        public const string direccionTesisFechaDefensa = "";
        public const string direccionTesisCalificacionObtenida = "";
        public const string direccionTesisFechaMencionDoctUE = "";
        public const string direccionTesisMencionCalidad = "";
        public const string direccionTesisDoctoradoUE = "";
        public const string direccionTesisFechaMencionCalidad = "";

        /// <summary>
        /// Formación académica impartida - 030.010.000.000
        /// </summary>
        public const string formacionAcademicaTipoDocenciaOficialidad = "";
        public const string formacionAcademicaTitulacionUniversitaria = "";
        public const string formacionAcademicaPaisEntidadRealizacion = "";
        public const string formacionAcademicaCCAAEntidadRealizacion = "";
        public const string formacionAcademicaCiudadEntidadRealizacion = "";
        public const string formacionAcademicaEntidadRealizacion = "";
        public const string formacionAcademicaTipoEntidadRealizacion = "";
        public const string formacionAcademicaTipoEntidadRealizacionOtros = "";
        public const string formacionAcademicaDepartamento = "";
        public const string formacionAcademicaTipoPrograma = "";
        public const string formacionAcademicaTipoProgramaOtros = "";
        public const string formacionAcademicaNombreAsignatura = "";
        public const string formacionAcademicaTipoDocenciaModalidad = "";
        public const string formacionAcademicaTipoDocenciaModalidadOtros = "";
        public const string formacionAcademicaTipoAsignatura = "";
        public const string formacionAcademicaTipoAsignaturaOtros = "";
        public const string formacionAcademicaCursoTitulacion = "";
        public const string formacionAcademicaTipoECTS = "";
        public const string formacionAcademicaNumeroECTS = "";
        public const string formacionAcademicaIdiomaAsignatura = "";
        public const string formacionAcademicaFrecuenciaAsignatura = "";
        public const string formacionAcademicaFechaFinalizacion = "";//TODO - revisar
        public const string formacionAcademicaCompetenciasRelacionadas = "";
        public const string formacionAcademicaCategoriaProfesional = "";
        public const string formacionAcademicaCalificacionObtenida = "";
        public const string formacionAcademicaCalificacionMax = "";
        public const string formacionAcademicaPaisEntidadEvaluacion = "";
        public const string formacionAcademicaCCAAEntidadEvaluacion = "";
        public const string formacionAcademicaCiudadEntidadEvaluacion = "";
        public const string formacionAcademicaEntidadEvaluacion = "";
        public const string formacionAcademicaTipoEntidadEvaluacion = "";
        public const string formacionAcademicaTipoEntidadEvaluacionOtros = "";
        public const string formacionAcademicaTipoEvaluacion = "";
        public const string formacionAcademicaTipoEvaluacionOtros = "";
        public const string formacionAcademicaPaisEntidadFinanciadora = "";
        public const string formacionAcademicaCCAAEntidadFinanciadora = "";
        public const string formacionAcademicaCiudadEntidadFinanciadora = "";
        public const string formacionAcademicaEntidadFinanciadora = "";
        public const string formacionAcademicaTipoEntidadFinanciadora = "";
        public const string formacionAcademicaTipoEntidadFinanciadoraOtros = "";
        public const string formacionAcademicaEntFinanTipoConvocatoria = "";
        public const string formacionAcademicaEntFinanTipoConvocatoriaOtros = "";
        public const string formacionAcademicaEntFinanAmbitoGeo = "";
        public const string formacionAcademicaEntFinanAmbitoGeoOtros = "";
        public const string formacionAcademicaFacultadEscuela = "";
        public const string formacionAcademicaFechaInicio = "";
        public const string formacionAcademicaFechaFinalizacion_ = "";//TODO -revisar

        /// <summary>
        /// Formación sanitaria especializada impartida - 030.020.000.000
        /// </summary>
        public const string formacionSaniEspeTituloEspe = "";
        public const string formacionSaniEspeTituloSubespe = "";
        public const string formacionSaniEspeTipoParticipacion = "";
        public const string formacionSaniEspeTipoParticipacionOtros = "";
        public const string formacionSaniEspePaisEntidadTitulacion = "";
        public const string formacionSaniEspeCCAAEntidadTitulacion = "";
        public const string formacionSaniEspeCiudadEntidadTitulacion = "";
        public const string formacionSaniEspeEntidadRealizacion = "";
        public const string formacionSaniEspeTipoEntidadRealizacion = "";
        public const string formacionSaniEspeTipoEntidadRealizacionOtros = "";
        public const string formacionSaniEspePaisEntidadRealizacion = "";
        public const string formacionSaniEspeCCAAEntidadRealizacion = "";
        public const string formacionSaniEspeCiudadEntidadRealizacion = "";
        public const string formacionSaniEspeEntidadTitulacion = "";
        public const string formacionSaniEspeTipoEntidadTitulacion = "";
        public const string formacionSaniEspeTipoEntidadTitulacionOtros = "";
        public const string formacionSaniEspeDepartamento = "";
        public const string formacionSaniEspeServicio = "";
        public const string formacionSaniEspeSeccion = "";
        public const string formacionSaniEspeUnidad = "";
        public const string formacionSaniEspeFechaInicio = "";
        public const string formacionSaniEspeFechaFinal = "";
        public const string formacionSaniEspeDuracionAnio = "";
        public const string formacionSaniEspeDuracionMes = "";
        public const string formacionSaniEspeDuracionDia = "";
        public const string formacionSaniEspePerfilDestinatario = "";

        /// <summary>
        /// Formación sanitaria en I+D, y/o posformación sanitaria especializada en I+D impartida - 030.030.000.000
        /// </summary>
        public const string formacionPosformacionTitulo = "";
        public const string formacionPosformacionPaisEntidadTitulacion = "";
        public const string formacionPosformacionCCAAEntidadTitulacion = "";
        public const string formacionPosformacionCiudadEntidadTitulacion = "";
        public const string formacionPosformacionEntidadRealizacion = "";
        public const string formacionPosformacionTipoEntidadRealizacion = "";
        public const string formacionPosformacionTipoEntidadRealizacionOtros = "";
        public const string formacionPosformacionPaisEntidadRealizacion = "";
        public const string formacionPosformacionCCAAEntidadRealizacion = "";
        public const string formacionPosformacionCiudadEntidadRealizacion = "";
        public const string formacionPosformacionEntidadTitulacion = "";
        public const string formacionPosformacionTipoEntidadTitulacion = "";
        public const string formacionPosformacionTipoEntidadTitulacionOtros = "";
        public const string formacionPosformacionDepartamento = "";
        public const string formacionPosformacionServicio = "";
        public const string formacionPosformacionSeccion = "";
        public const string formacionPosformacionUnidad = "";
        public const string formacionPosformacionFechaInicio = "";
        public const string formacionPosformacionFechaFinal = "";
        public const string formacionPosformacionDuracionAnio = "";
        public const string formacionPosformacionDuracionMes = "";
        public const string formacionPosformacionDuracionDia = "";
        public const string formacionPosformacionPerfilDestinatario = "";

        /// <summary>
        /// Tutorias académicas de estudiantes - 030.050.000.000
        /// </summary>
        public const string tutoAcademicaNombrePrograma = "";
        public const string tutoAcademicaNombreProgramaOtros = "";
        public const string tutoAcademicaPaisEntidadRealizacion = "";
        public const string tutoAcademicaCCAAEntidadRealizacion = "";
        public const string tutoAcademicaCiudadEntidadRealizacion = "";
        public const string tutoAcademicaEntidadRealizacion = "";
        public const string tutoAcademicaTipoEntidadRealizacion = "";
        public const string tutoAcademicaTipoEntidadRealizacionOtros = "";
        public const string tutoAcademicaNumAlumnosTutelados = "";
        public const string tutoAcademicaFrecuenciaActividad = "";
        public const string tutoAcademicaNumHorasECTS = "";

        /// <summary>
        /// Cursos y seminarios impartidos orientados a la formación docente universitaria - 030.060.000.000
        /// </summary>
        public const string cursosSeminariosTipoEvento = "";
        public const string cursosSeminariosTipoEventoOtros = "";
        public const string cursosSeminariosNombreEvento = "";
        public const string cursosSeminariosPaisEntidadOrganizadora = "";
        public const string cursosSeminariosCCAAEntidadOrganizadora = "";
        public const string cursosSeminariosCiudadEntidadOrganizadora = "";
        public const string cursosSeminariosEntidadOrganizadora = "";
        public const string cursosSeminariosTipoEntidadOrganizadora = "";
        public const string cursosSeminariosTipoEntidadOrganizadoraOtros = "";
        public const string cursosSeminariosObjetivosCurso = "";
        public const string cursosSeminariosPerfilDestinatarios = "";
        public const string cursosSeminariosIdiomaImpartio = "";
        public const string cursosSeminariosFechaImparticion = "";
        public const string cursosSeminariosHorasImpartidas = "";
        public const string cursosSeminariosTipoParticipacion = "";
        public const string cursosSeminariosTipoParticipacionOtros = "";
        public const string cursosSeminariosISBN = "";
        public const string cursosSeminariosAutorCorrespondencia = "";
        public const string cursosSeminariosIdentificadorPublicacion = "";
        public const string cursosSeminariosTipoIdentificadorPublicacion = "";
        public const string cursosSeminariosTipoIdentificadorPublicacionOtros = "";


        /// <summary>
        /// Publicaciones docentes o de carácter pedagógico, libros, articulos, etc. - 030.070.000.000
        /// </summary>
        public const string publicacionDocenteNombre = "";
        public const string publicacionDocentePerfilDestinatario = "";
        public const string publicacionDocenteAutores = "";
        public const string publicacionDocentePosicionFirma = "";
        public const string publicacionDocenteFechaElaboracion = "";
        public const string publicacionDocenteTipologiaSoporte = "";
        public const string publicacionDocenteTipologiaSoporteOtros = "";
        public const string publicacionDocenteTituloPublicacion = "";
        public const string publicacionDocenteNombrePublicacion = "";
        public const string publicacionDocenteVolumenPublicacion = "";
        public const string publicacionDocentePagIniPublicacion = "";
        public const string publicacionDocentePagFinalPublicacion = "";
        public const string publicacionDocenteEditorialPublicacion = "";
        public const string publicacionDocentePaisPublicacion = "";
        public const string publicacionDocenteCCAAPublicacion = "";
        public const string publicacionDocenteFechaPublicacion = "";
        public const string publicacionDocenteURLPublicacion = "";
        public const string publicacionDocenteISBNPublicacion = "";
        public const string publicacionDocenteDepositoLegal = "";
        public const string publicacionDocenteJustificacionMaterial = "";
        public const string publicacionDocenteGradoContribucion = "";
        public const string publicacionDocenteAutorCorrespondencia = "";
        public const string publicacionDocenteIdentificadorPublicacion = "";
        public const string publicacionDocenteTipoIdentificadorPublicacion = "";
        public const string publicacionDocenteTipoIdentificadorPublicacionOtros = "";

        /// <summary>
        /// Participación en proyectos de innovacion docente - 030.080.000.000
        /// </summary>
        public const string participacionInnovaTitulo = "";
        public const string participacionInnovaPaisEntidadRealizacion = "";
        public const string participacionInnovaCCAAEntidadRealizacion = "";
        public const string participacionInnovaCiudadEntidadRealizacion = "";
        public const string participacionInnovaTipoParticipacion = "";
        public const string participacionInnovaTipoParticipacionOtros = "";
        public const string participacionInnovaAportacionProyecto = "";
        public const string participacionInnovaRegimenDedicacion = "";
        public const string participacionInnovaEntidadFinanciadora = "";
        public const string participacionInnovaTipoEntidadFinanciadora = "";
        public const string participacionInnovaTipoEntidadFinanciadoraOtros = "";
        public const string participacionInnovaTipoConvocatoria = "";
        public const string participacionInnovaTipoConvocatoriaOtros = "";
        public const string participacionInnovaEntidadParticipante = "";
        public const string participacionInnovaTipoEntidadParticipante = "";
        public const string participacionInnovaTipoEntidadParticipanteOtros = "";
        public const string participacionInnovaTipoDuracionRelacionLaboral = "";
        public const string participacionInnovaDuracionParticipacionAnio = "";
        public const string participacionInnovaDuracionParticipacionMes = "";
        public const string participacionInnovaDuracionParticipacionDia = "";
        public const string participacionInnovaFechaFinalizacionParticipacion = "";
        public const string participacionInnovaNombreIP = "";
        public const string participacionInnovaNumParticipantes = "";
        public const string participacionInnovaImporteConcedido = "";
        public const string participacionInnovaAmbitoProyecto = "";
        public const string participacionInnovaAmbitoProyectoOtros = "";
        public const string participacionInnovaFechaInicio = "";

        /// <summary>
        /// Participación en congresos con ponencias orientadas a la formación docente - 030.090.000.000
        /// </summary>
        public const string participaCongresosTipoEvento = "";
        public const string participaCongresosTipoEventoOtros = "";
        public const string participaCongresosNombreEvento = "";
        public const string participaCongresosPaisEvento = "";
        public const string participaCongresosCCAAEvento = "";
        public const string participaCongresosCiudadEvento = "";
        public const string participaCongresosEntidadOrganizadora = "";
        public const string participaCongresosTipoEntidadOrganizadora = "";
        public const string participaCongresosTipoEntidadOrganizadoraOtros = "";
        public const string participaCongresosPaisEntidadOrganizadora = "";
        public const string participaCongresosCCAAEntidadOrganizadora = "";
        public const string participaCongresosCiudadEntidadOrganizadora = "";
        public const string participaCongresosObjetivosEvento = "";
        public const string participaCongresosPerfilDestinatarios = "";
        public const string participaCongresosIdiomaPresentacion = "";
        public const string participaCongresosFechaPresentacion = "";
        public const string participaCongresosTipoParticipacion = "";
        public const string participaCongresosTipoParticipacionOtros = "";
        public const string participaCongresosTipoPublicacion = "";
        public const string participaCongresosTituloPublicacion = "";
        public const string participaCongresosNombrePublicacion = "";
        public const string participaCongresosVolumenPublicacion = "";
        public const string participaCongresosPagIniPublicacion = "";
        public const string participaCongresosPagFinalPublicacion = "";
        public const string participaCongresosEditorialPublicacion = "";
        public const string participaCongresosPaisPublicacion = "";
        public const string participaCongresosCCAAPublicacion = "";
        public const string participaCongresosFechaPublicacion = "";
        public const string participaCongresosURLPublicacion = "";
        public const string participaCongresosISBNPublicacion = "";
        public const string participaCongresosDepositoLegalPublicacion = "";
        public const string participaCongresosNumHorasPublicacion = "";
        public const string participaCongresosFechaInicioPublicacion = "";
        public const string participaCongresosFechaFinalPublicacion = "";
        public const string participaCongresosIdentificadorPublicacion = "";
        public const string participaCongresosTipoIDPublicacion = "";
        public const string participaCongresosTipoIDSPublicacionOtros = "";
        public const string participaCongresosAutorCorrespondencia = "";


        /// <summary>
        /// Premios de innovación docente recibidos - 060.030.080.000
        /// </summary>
        public const string premiosInnovaNombre = "";
        public const string premiosInnovaPaisEntidadConcesionaria = "";
        public const string premiosInnovaCCAAEntidadConcesionaria = "";
        public const string premiosInnovaCiudadEntidadConcesionaria = "";
        public const string premiosInnovaEntidadConcesionaria = "";
        public const string premiosInnovaTipoEntidadConcesionaria = "";
        public const string premiosInnovaTipoEntidadConcesionariaOtros = "";
        public const string premiosInnovaPropuestaDe = "";
        public const string premiosInnovaFechaConcesion = "";


        /// <summary>
        /// Otras actividades/méritos no incluidos en la relación anterior - 030.100.000.000
        /// </summary>
        public const string otrasActividadesDescripcion = "";
        public const string otrasActividadesPalabrasClave = "";
        public const string otrasActividadesPaisRealizacion = "";
        public const string otrasActividadesCCAARealizacion = "";
        public const string otrasActividadesCiudadRealizacion = "";
        public const string otrasActividadesEntidadOrganizadora = "";
        public const string otrasActividadesTipoEntidadOrganizadora = "";
        public const string otrasActividadesTipoEntidadOrganizadoraOtros = "";
        public const string otrasActividadesFechaFinalizacion = "";


        /// <summary>
        /// Aportaciones más relevantes de su CV de docencia - 030.110.000.000
        /// </summary>
        public const string aportacionesCVDescripcion = "";
        public const string aportacionesCVPalabrasClave = "";
        public const string aportacionesCVPaisRealizacion = "";
        public const string aportacionesCVCCAARealizacion = "";
        public const string aportacionesCVCiudadRealizacion = "";
        public const string aportacionesCVEntidadOrganizadora = "";
        public const string aportacionesCVTipoEntidadOrganizadora = "";
        public const string aportacionesCVTipoEntidadOrganizadoraOtros = "";
        public const string aportacionesCVFechaFinalizacion = "";

    }
}
