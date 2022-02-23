using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Variables
{
    class FormacionAcademica
    {
        /// <summary>
        /// Estudios de 1º y 2º ciclo, y antiguos ciclos (Licenciados, Diplomados, Ingenieros Superiores, Ingenieros Técnicos, Arquitectos) - 020.010.010.000
        /// </summary>
        public const string estudiosCicloTitulacion = "";
        public const string estudiosCicloTitulacionOtros = "";
        public const string estudiosCicloNombreTitulo = "";
        public const string estudiosCicloPaisEntidadTitulacion = "";
        public const string estudiosCicloCCAAEntidadTitulacion = "";
        public const string estudiosCicloCiudadEntidadTitulacion = "";
        public const string estudiosCicloEntidadTitulacion = "";
        public const string estudiosCicloTipoEntidadTitulacion = "";
        public const string estudiosCicloTipoEntidadTitulacionOtros = "";
        public const string estudiosCicloFechaTitulacion = "";
        public const string estudiosCicloNotaMedia = "";
        public const string estudiosCicloTituloExtranjero = "";
        public const string estudiosCicloFechaHomologacion = "";
        public const string estudiosCicloTituloHomologado = "";
        public const string estudiosCicloPremio = "";
        public const string estudiosCicloPremioOtros = "";

        /// <summary>
        /// Doctorados - 020.010.020.000
        /// </summary>
        public const string doctoradosProgramaDoctorado = "";
        public const string doctoradosEntidadTitulacionDEA = "";
        public const string doctoradosFechaObtencionDEA = "";
        public const string doctoradosPaisEntidadTitulacion = "";
        public const string doctoradosCCAAEntidadTitulacion = "";
        public const string doctoradosCiudadEntidadTitulacion = "";
        public const string doctoradosEntidadTitulacion = "";
        public const string doctoradosTipoEntidadTitulacion = "";
        public const string doctoradosTipoEntidadTitulacionOtros = "";
        public const string doctoradosFechaTitulacion = "";
        public const string doctoradosFechaMencionDocUE = "";
        public const string doctoradosTituloTesis = "";
        public const string doctoradosDirectorTesis = "";
        public const string doctoradosCodirectorTesis = "";
        public const string doctoradosCalificacionObtenida = "";
        public const string doctoradosDoctoradoUE = "";
        public const string doctoradosMencionCalidad = "";
        public const string doctoradosPremioExtraordinario = "";
        public const string doctoradosFechaObtencion = "";
        public const string doctoradosTituloHomologado = "";
        public const string doctoradosFechaHomologacion = "";
        
        /// <summary>
        /// Otra formación de posgrado - 020.010.030.000
        /// </summary>
        public const string otraFormacionTipoFormacion = "";
        public const string otraFormacionTituloPosgrado = "";
        public const string otraFormacionPaisEntidadTitulacion = "";
        public const string otraFormacionCCAAEntidadTitulacion = "";
        public const string otraFormacionCiudadEntidadTitulacion = "";
        public const string otraFormacionEntidadTitulacion = "";
        public const string otraFormacionTipoEntidadTitulacion = "";
        public const string otraFormacionTipoEntidadTitulacionOtros = "";
        public const string otraFormacionFechaTitulacion = "";
        public const string otraFormacionCalificacionObtenida = "";
        public const string otraFormacionFacultadEscuela = "";
        public const string otraFormacionTituloHomologado = "";
        public const string otraFormacionFechaHomologacion = "";

        /// <summary>
        /// Formación especializada, continuada, técnica, profesionalizada, de reciclaje y actualización - 020.020.000.000
        /// </summary>
        public const string formacionEspeTipoFormacion = "";
        public const string formacionEspeTipoFormacionOtros = "";
        public const string formacionEspeTituloFormacion = "";
        public const string formacionEspePaisEntidadTitulacion = "";
        public const string formacionEspeCCAAEntidadTitulacion = "";
        public const string formacionEspeCiudadEntidadTitulacion = "";
        public const string formacionEspeEntidadTitulacion = "";
        public const string formacionEspeTipoEntidadTitulacion = "";
        public const string formacionEspeTipoEntidadTitulacionOtros = "";
        public const string formacionEspeObjetivosEntidad = "";
        public const string formacionEspeNombreResponsable = "";
        public const string formacionEspeDuracionHoras = "";
        public const string formacionEspeFechaFinalizacion = "";

        /// <summary>
        /// Formación sanitaria especializada - 020.030.000.000
        /// </summary>
        public const string formacionSanitariaEspeTituloEspe = "";
        public const string formacionSanitariaEspeTituloSubespe = "";
        public const string formacionSanitariaEspePaisEntidadRealizacion = "";
        public const string formacionSanitariaEspeCCAAEntidadRealizacion = "";
        public const string formacionSanitariaEspeCiudadEntidadRealizacion = "";
        public const string formacionSanitariaEspeEntidadRealizacion = "";
        public const string formacionSanitariaEspeTipoEntidadRealizacion = "";
        public const string formacionSanitariaEspeTipoEntidadRealizacionOtros = "";
        public const string formacionSanitariaEspeEntidadTitulacion = "";
        public const string formacionSanitariaEspePaisEntidadTitulacion = "";
        public const string formacionSanitariaEspeCCAAEntidadTitulacion = "";
        public const string formacionSanitariaEspeCiudadEntidadTitulacion = "";
        public const string formacionSanitariaEspeTipoEntidadTitulacion = "";
        public const string formacionSanitariaEspeTipoEntidadTitulacionOtros = "";
        public const string formacionSanitariaEspeFechaInicio = "";
        public const string formacionSanitariaEspeFechaFinal = "";
        public const string formacionSanitariaEspePermanenciaAnio = "";
        public const string formacionSanitariaEspePermanenciaMes = "";
        public const string formacionSanitariaEspePermanenciaDia = "";
        public const string formacionSanitariaEspeFechaConvalidacion = "";

        /// <summary>
        /// Formación sanitaria en I+D - 020.040.000.000
        /// </summary>
        public const string formacionSanitariaIDNombre = "";
        public const string formacionSanitariaIDPaisEntidadTitulacion = "";
        public const string formacionSanitariaIDCCAAEntidadTitulacion = "";
        public const string formacionSanitariaIDCiudadEntidadTitulacion = "";
        public const string formacionSanitariaIDEntidadRealizacion = "";
        public const string formacionSanitariaIDTipoEntidadRealizacion = "";
        public const string formacionSanitariaIDTipoEntidadRealizacionOtros = "";
        public const string formacionSanitariaIDPaisEntidadRealizacion = "";
        public const string formacionSanitariaIDCCAAEntidadRealizacion = "";
        public const string formacionSanitariaIDCiudadEntidadRealizacion = "";
        public const string formacionSanitariaIDEntidadTitulacion = "";
        public const string formacionSanitariaIDTipoEntidadTitulacion = "";
        public const string formacionSanitariaIDTipoEntidadTitulacionOtros = "";
        public const string formacionSanitariaIDDepartamento = "";
        public const string formacionSanitariaIDServicio = "";
        public const string formacionSanitariaIDSeccion = "";
        public const string formacionSanitariaIDUnidad = "";
        public const string formacionSanitariaIDDuracionAnio = "";
        public const string formacionSanitariaIDDuracionMes = "";
        public const string formacionSanitariaIDDuracionDia = "";
        public const string formacionSanitariaIDFechaInicio = "";
        public const string formacionSanitariaIDFechaFinal = "";
        public const string formacionSanitariaIDCalificacionObtenida = "";
        public const string formacionSanitariaIDInteresDocencia = "";
        public const string formacionSanitariaIDCategoriaProfesional = "";

        /// <summary>
        /// Cursos y seminarios recibidos de perfeccionamiento, innovación y mejora 
        /// docente, nuevas tecnologías, etc., cuyo objetivo sea la mejora de la docencia - 020.050.000.000
        /// </summary>
        public const string cursosSeminariosTitulo = "";
        public const string cursosSeminariosObjetivos = "";
        public const string cursosSeminariosPaisEntidadOrganizadora = "";
        public const string cursosSeminariosCCAAEntidadOrganizadora = "";
        public const string cursosSeminariosCiudadEntidadOrganizadora = "";
        public const string cursosSeminariosEntidadOrganizadora = "";
        public const string cursosSeminariosTipoEntidadOrganizadora = "";
        public const string cursosSeminariosTipoEntidadOrganizadoraOtros = "";
        public const string cursosSeminariosDuracionHoras = "";
        public const string cursosSeminariosFechaFinal = "";
        public const string cursosSeminariosPerfilDestinatarios = "";
        public const string cursosSeminariosFechaInicio = "";
        public const string cursosSeminariosFacultadEscuela = "";
        public const string cursosSeminariosMesesAnio = "";
        public const string cursosSeminariosMesesMes = "";
        public const string cursosSeminariosMesesDia = "";
        public const string cursosSeminariosProgramaFinanciacion = "";
        public const string cursosSeminariosTareasContrastables = "";
        public const string cursosSeminariosObjetivoEstancia = "";
        public const string cursosSeminariosObjetivoEstanciaOtros = "";

        /// <summary>
        /// Conocimiento de idiomas - 020.060.000.000
        /// </summary>
        public const string conocimientoIdiomasIdioma = "";
        public const string conocimientoIdiomasComprensionAuditiva = "";
        public const string conocimientoIdiomasComprensionLectura = "";
        public const string conocimientoIdiomasInteraccionOral = "";
        public const string conocimientoIdiomasExpresionOral = "";
        public const string conocimientoIdiomasExpresionEscrita = "";
    }
}
