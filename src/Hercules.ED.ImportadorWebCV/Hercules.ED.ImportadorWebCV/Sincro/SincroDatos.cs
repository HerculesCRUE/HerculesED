using Hercules.ED.ImportadorWebCV.Controllers;
using Hercules.ED.ImportadorWebCV.Models;
using Import;
using ImportadorWebCV.Sincro.Secciones;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace ImportadorWebCV.Sincro
{
    public class SincroDatos
    {
        private cvnRootResultBean cvn;
        private string cvID;

        public SincroDatos()
        {
            cvn = new cvnRootResultBean();
        }

        /// <summary>
        /// Genero un archivo XML a partir del PDF.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pInput"></param>
        /// <returns></returns>
        private FormFile GenerarRootBean(ConfigService _Configuracion, IFormFile pInput)
        {
            long length = pInput.Length;
            using var fileStream = pInput.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)pInput.Length);

            Cvn2RootBeanClient cvnRootBeanClient = new Cvn2RootBeanClient();
            var x = cvnRootBeanClient.cvnPdf2CvnRootBeanAsync(_Configuracion.GetUsuarioPDF(), _Configuracion.GetContraseñaPDF(), bytes);
            Import.cvnRootResultBean cvnRootResultBean = x.Result.@return;

            XmlSerializer xmlSerializer = new XmlSerializer(cvnRootResultBean.GetType());
            MemoryStream memoryStream = new MemoryStream();

            xmlSerializer.Serialize(memoryStream, cvnRootResultBean);

            FormFile file = new FormFile(memoryStream, 0, memoryStream.Length, null, Path.GetFileName(pInput.FileName))
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/xml"
            };
            return file;
        }

        /// <summary>
        /// Construyo el cvnRootResultBean a partir de un archivo PDF o XML, en el caso del PDF lo transformo a XML.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="cvID"></param>
        /// <param name="CVFile"></param>
        public SincroDatos(ConfigService _Configuracion, string cvID, IFormFile CVFile)
        {
            string extensionFile = Path.GetExtension(CVFile.FileName);

            //Si no es un XML o un PDF. No hago nada
            if (!extensionFile.Equals(".xml") && !extensionFile.Equals(".pdf"))
            {
                throw new FileLoadException("Extensión de archivo invalida");
            }
            //Si es un PDF lo convierto a XML y lo inserto.
            if (extensionFile.Equals(".pdf"))
            {
                FormFile CVFileAsXML = GenerarRootBean(_Configuracion, CVFile);

                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFileAsXML.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFile.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
            }

        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Datos de identificacion y contacto.
        /// Con el codigo identificativo 000.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosIdentificacion([Optional] bool preimportar)
        {
            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("000.000.000.000", datosIdentificacion.SincroDatosIdentificacion(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Situación profesional.stop
        /// Con el codigo identificativo 010.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosSituacionProfesional([Optional] bool preimportar)
        {
            SituacionProfesional situacionProfesional = new SituacionProfesional(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("010.010.000.000", situacionProfesional.SincroSituacionProfesionalActual(preimportar)));
            listadoSecciones.Add(new Subseccion("010.020.000.000", situacionProfesional.SincroCargosActividades(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Formación académica recibida.
        /// Con el codigo identificativo 020.000.000.000
        /// </summary>
        public List<Subseccion> SincroFormacionAcademica([Optional] bool preimportar)
        {
            FormacionAcademica formacionAcademica = new FormacionAcademica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();

            listadoSecciones.Add(new Subseccion("020.010.010.000", formacionAcademica.SincroEstudiosCiclos(preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.020.000", formacionAcademica.SincroDoctorados(preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.030.000", formacionAcademica.SincroOtraFormacionPosgrado(preimportar)));
            listadoSecciones.Add(new Subseccion("020.020.010.000", formacionAcademica.SincroFormacionEspecializada(preimportar)));
            listadoSecciones.Add(new Subseccion("020.050.010.000", formacionAcademica.SincroCursosMejoraDocente(preimportar)));
            listadoSecciones.Add(new Subseccion("020.060.010.000", formacionAcademica.SincroConocimientoIdiomas(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad docente.
        /// Con el codigo identificativo 030.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadDocente([Optional] bool preimportar)
        {
            ActividadDocente actividadDocente = new ActividadDocente(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();

            listadoSecciones.Add(new Subseccion("030.040.010.000", actividadDocente.SincroDireccionTesis(preimportar)));
            listadoSecciones.Add(new Subseccion("030.010.000.000", actividadDocente.SincroFormacionAcademica(preimportar)));
            listadoSecciones.Add(new Subseccion("030.050.000.000", actividadDocente.SincroTutoriasAcademicas(preimportar)));
            listadoSecciones.Add(new Subseccion("030.060.000.000", actividadDocente.SincroCursosSeminarios(preimportar)));
            listadoSecciones.Add(new Subseccion("030.070.000.000", actividadDocente.SincroPublicacionDocentes(preimportar)));
            listadoSecciones.Add(new Subseccion("030.080.000.000", actividadDocente.SincroParticipacionProyectosInnovacionDocente(preimportar)));
            listadoSecciones.Add(new Subseccion("030.090.000.000", actividadDocente.SincroParticipacionCongresosFormacionDocente(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.080.000", actividadDocente.SincroPremiosInovacionDocente(preimportar)));
            listadoSecciones.Add(new Subseccion("030.100.000.000", actividadDocente.SincroOtrasActividades(preimportar)));
            listadoSecciones.Add(new Subseccion("030.110.000.000", actividadDocente.SincroAportacionesRelevantes(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Experiencia científica y tecnológica.
        /// Con el codigo identificativo 050.000.000.000
        /// </summary>
        public List<Subseccion> SincroExperienciaCientificaTecnologica([Optional] bool preimportar)
        {
            ExperienciaCientificaTecnologica experienciaCientificaTecnologica = new ExperienciaCientificaTecnologica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();

            listadoSecciones.Add(new Subseccion("050.020.010.000", experienciaCientificaTecnologica.SincroProyectosIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("050.020.020.000", experienciaCientificaTecnologica.SincroContratos(preimportar)));
            listadoSecciones.Add(new Subseccion("050.030.010.000", experienciaCientificaTecnologica.SincroPropiedadIndustrialIntelectual(preimportar)));
            listadoSecciones.Add(new Subseccion("050.010.000.000", experienciaCientificaTecnologica.SincroGrupoIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("050.020.030.000", experienciaCientificaTecnologica.SincroObrasArtisticas(preimportar)));
            listadoSecciones.Add(new Subseccion("050.030.020.000", experienciaCientificaTecnologica.SincroResultadosTecnologicos(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad científica y tecnológica.
        /// Con el codigo identificativo 060.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadCientificaTecnologica([Optional] bool preimportar)
        {
            ActividadCientificaTecnologica actividadCientificaTecnologica = new ActividadCientificaTecnologica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();

            listadoSecciones.Add(new Subseccion("060.010.000.000", actividadCientificaTecnologica.SincroProduccionCientifica(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.060.010", actividadCientificaTecnologica.SincroIndicadoresGenerales(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.010.000", actividadCientificaTecnologica.SincroPublicacionesDocumentos(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.020.000", actividadCientificaTecnologica.SincroTrabajosCongresos(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.030.000", actividadCientificaTecnologica.SincroTrabajosJornadasSeminarios(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.040.000", actividadCientificaTecnologica.SincroOtrasActividadesDivulgacion(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.010.000", actividadCientificaTecnologica.SincroComitesCTA(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.030.000", actividadCientificaTecnologica.SincroOrganizacionIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.040.000", actividadCientificaTecnologica.SincroGestionIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.050.000", actividadCientificaTecnologica.SincroForosComites(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.060.000", actividadCientificaTecnologica.SincroEvalRevIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.050.000", actividadCientificaTecnologica.SincroEstanciasIDI(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.010.000", actividadCientificaTecnologica.SincroAyudasBecas(preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.020.000", actividadCientificaTecnologica.SincroOtrosModosColaboracion(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.020.000", actividadCientificaTecnologica.SincroSociedadesAsociaciones(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.030.000", actividadCientificaTecnologica.SincroConsejos(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.040.000", actividadCientificaTecnologica.SincroRedesCooperacion(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.050.000", actividadCientificaTecnologica.SincroPremiosMenciones(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.060.000", actividadCientificaTecnologica.SincroOtrasDistinciones(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.070.000", actividadCientificaTecnologica.SincroPeriodosActividad(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.090.000", actividadCientificaTecnologica.SincroAcreditacionesObtenidas(preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.100.000", actividadCientificaTecnologica.SincroResumenOtrosMeritos(preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Resumen Texto Libre.
        /// Con el codigo identificativo 070.010.000.000
        /// </summary>
        /// <param name="preimportar"></param>
        /// <returns></returns>
        public List<Subseccion> SincroTextoLibre([Optional] bool preimportar)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>
            {
                new Subseccion("070.010.000.000", textoLibre.SincroTextoLibre(preimportar))
            };

            return listadoSecciones;
        }
    }
}
