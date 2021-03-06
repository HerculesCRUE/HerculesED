using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using Import;
using ImportadorWebCV.Sincro.Secciones;
using Microsoft.AspNetCore.Http;
using Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using Utils;
using System.Xml;

namespace ImportadorWebCV.Sincro
{
    public class SincroDatos
    {
        readonly ConfigService mConfiguracion;
        protected cvnRootResultBean cvn;
        private string cvID;
        private string personID;
        public FormFile CVFileAsXML;

        public int GetNumItems()
        {
            return cvn.cvnRootBean.Length;
        }

        private List<string> listadoSecciones = new List<string>()
        {
            //Datos identificacion
            "000.010.000.000",
            //Situación profesional
            "010.010.000.000","010.020.000.000",
            //Formacion academica
            "020.010.010.000","020.010.020.000","020.010.030.000","020.020.000.000","020.050.000.000","020.060.000.000",
            //Actividad docente
            "030.040.000.000","030.010.000.000","030.050.000.000","030.060.000.000","030.070.000.000","030.080.000.000",
            "030.090.000.000","060.030.080.000","030.100.000.000","030.110.000.000",
            //Experiencia cientifica tecnologica
            "050.020.010.000","050.020.020.000","050.030.010.000","050.010.000.000","050.020.030.000","050.030.020.000",
            //Actividad cientifica tecnologica
            "060.010.000.000", "060.010.060.000", "060.010.060.010","060.010.010.000","060.010.020.000", "060.010.030.000", "060.010.040.000", "060.020.010.000",
            "060.020.030.000", "060.020.040.000", "060.020.050.000", "060.020.060.000", "060.010.050.000", "060.030.010.000", "060.020.020.000",
            "060.030.020.000", "060.030.030.000", "060.030.040.000", "060.030.050.000", "060.030.060.000", "060.030.070.000", "060.030.090.000",
            "060.030.100.000",
            //Texto libre
            "070.010.000.000"
        };

        /// <summary>
        /// Construyo el cvnRootResultBean a partir de un archivo PDF o XML, en el caso del PDF lo transformo a XML.
        /// </summary>
        /// <param name="Configuracion"></param>
        /// <param name="cvID"></param>
        /// <param name="CVFile"></param>
        public SincroDatos(ConfigService Configuracion, string cvID, IFormFile CVFile)
        {
            mConfiguracion = Configuracion;
            string extensionFile = Path.GetExtension(CVFile.FileName);

            //Si no es un XML o un PDF. No hago nada
            if (!extensionFile.Equals(".xml") && !extensionFile.Equals(".pdf"))
            {
                throw new FileLoadException("Extensión de archivo invalida");
            }
            //Si es un PDF lo convierto a XML y lo inserto.
            if (extensionFile.Equals(".pdf"))
            {
                try
                {
                    CVFileAsXML = GenerarRootBean(mConfiguracion, CVFile);

                    XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                    using (StreamReader reader = new StreamReader(CVFileAsXML.OpenReadStream()))
                    {
                        cvn = (cvnRootResultBean)ser.Deserialize(reader);
                    }
                    this.cvID = cvID;
                    this.personID = Utility.PersonaCV(cvID);
                }
                catch (Exception e)
                {
                    
                }
            }
            else
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                    CVFileAsXML = (FormFile)CVFile;
                    using (StreamReader reader = new StreamReader(CVFile.OpenReadStream()))
                    {
                        cvn = (cvnRootResultBean)ser.Deserialize(reader);
                    }
                    this.cvID = cvID;
                    this.personID = Utility.PersonaCV(cvID);
                }
                catch (Exception e)
                {

                }
            }
        }

        public SincroDatos(ConfigService Configuracion, string cvID, string data)
        {
            mConfiguracion = Configuracion;
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            using (StringReader reader = new StringReader(data))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            this.cvID = cvID;
            this.personID = Utility.PersonaCV(cvID);
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
            //Aumento el tiempo de espera a 2 hora como máximo
            cvnRootBeanClient.Endpoint.Binding.CloseTimeout = new TimeSpan(2, 0, 0);
            cvnRootBeanClient.Endpoint.Binding.SendTimeout = new TimeSpan(2, 0, 0);
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

        public byte[] GuardarXMLFiltrado()
        {
            string xml = "";
            XmlSerializer serializer = new XmlSerializer(typeof(cvnRootResultBean));
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    serializer.Serialize(writer, cvn);
                    xml = sww.ToString();
                }
            }

            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            return bytes;
        }

        public void ComprobarSecciones()
        {
            List<CvnItemBean> listCvnRootBean = cvn.cvnRootBean.ToList();
            foreach (CvnItemBean itemBean in new List<CvnItemBean>(listCvnRootBean))
            {
                if (!listadoSecciones.Contains(itemBean.Code))
                {
                    listCvnRootBean.Remove(itemBean);
                }
            }
            cvn.cvnRootBean = listCvnRootBean.ToArray();
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Datos de identificacion y contacto.
        /// Con el codigo identificativo 000.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosIdentificacion([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD)
        {
            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("000.000.000.000", datosIdentificacion.SincroDatosIdentificacion(UtilitySecciones.CheckSecciones(secciones, "000.000.000.000"), preimportar, listadoIdBBDD)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Situación profesional.
        /// Con el codigo identificativo 010.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosSituacionProfesional([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            SituacionProfesional situacionProfesional = new SituacionProfesional(cvn, cvID, personID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("010.010.000.000", situacionProfesional.SincroSituacionProfesionalActual(UtilitySecciones.CheckSecciones(secciones, "010.010.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("010.020.000.000", situacionProfesional.SincroCargosActividades(UtilitySecciones.CheckSecciones(secciones, "010.020.000.000"), preimportar, listadoIdBBDD, petitionStatus)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Formación académica recibida.
        /// Con el codigo identificativo 020.000.000.000
        /// </summary>
        public List<Subseccion> SincroFormacionAcademica([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            FormacionAcademica formacionAcademica = new FormacionAcademica(cvn, cvID, personID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("020.010.010.000", formacionAcademica.SincroEstudiosCiclos(UtilitySecciones.CheckSecciones(secciones, "020.010.010.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("020.010.020.000", formacionAcademica.SincroDoctorados(UtilitySecciones.CheckSecciones(secciones, "020.010.020.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("020.010.030.000", formacionAcademica.SincroOtraFormacionPosgrado(UtilitySecciones.CheckSecciones(secciones, "020.010.030.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("020.020.000.000", formacionAcademica.SincroFormacionEspecializada(UtilitySecciones.CheckSecciones(secciones, "020.020.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("020.050.000.000", formacionAcademica.SincroCursosMejoraDocente(UtilitySecciones.CheckSecciones(secciones, "020.050.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("020.060.000.000", formacionAcademica.SincroConocimientoIdiomas(UtilitySecciones.CheckSecciones(secciones, "020.060.000.000"), preimportar, listadoIdBBDD, petitionStatus)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad docente.
        /// Con el codigo identificativo 030.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadDocente([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            ActividadDocente actividadDocente = new ActividadDocente(cvn, cvID, personID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("030.040.000.000", actividadDocente.SincroDireccionTesis(UtilitySecciones.CheckSecciones(secciones, "030.040.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.010.000.000", actividadDocente.SincroFormacionAcademica(UtilitySecciones.CheckSecciones(secciones, "030.010.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.050.000.000", actividadDocente.SincroTutoriasAcademicas(UtilitySecciones.CheckSecciones(secciones, "030.050.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.060.000.000", actividadDocente.SincroCursosSeminarios(UtilitySecciones.CheckSecciones(secciones, "030.060.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.070.000.000", actividadDocente.SincroPublicacionDocentes(UtilitySecciones.CheckSecciones(secciones, "030.070.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.080.000.000", actividadDocente.SincroParticipacionProyectosInnovacionDocente(UtilitySecciones.CheckSecciones(secciones, "030.080.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.090.000.000", actividadDocente.SincroParticipacionCongresosFormacionDocente(UtilitySecciones.CheckSecciones(secciones, "030.090.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.080.000", actividadDocente.SincroPremiosInovacionDocente(UtilitySecciones.CheckSecciones(secciones, "060.030.080.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.100.000.000", actividadDocente.SincroOtrasActividades(UtilitySecciones.CheckSecciones(secciones, "030.100.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("030.110.000.000", actividadDocente.SincroAportacionesRelevantes(UtilitySecciones.CheckSecciones(secciones, "030.110.000.000"), preimportar, listadoIdBBDD, petitionStatus)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Experiencia científica y tecnológica.
        /// Con el codigo identificativo 050.000.000.000
        /// </summary>
        public List<Subseccion> SincroExperienciaCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            ExperienciaCientificaTecnologica experienciaCientificaTecnologica = new ExperienciaCientificaTecnologica(cvn, cvID, personID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("050.020.010.000", experienciaCientificaTecnologica.SincroProyectosIDI(UtilitySecciones.CheckSecciones(secciones, "050.020.010.000"), preimportar, listadoIdBBDD, petitionStatus)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.020.020.000", experienciaCientificaTecnologica.SincroContratos(UtilitySecciones.CheckSecciones(secciones, "050.020.020.000"), preimportar, listadoIdBBDD, petitionStatus)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.030.010.000", experienciaCientificaTecnologica.SincroPropiedadIndustrialIntelectual(UtilitySecciones.CheckSecciones(secciones, "050.030.010.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("050.010.000.000", experienciaCientificaTecnologica.SincroGrupoIDI(UtilitySecciones.CheckSecciones(secciones, "050.010.000.000"), preimportar, listadoIdBBDD, petitionStatus)));//TODO - tesauro palabras clave->areas tematicas
            listadoSecciones.Add(new Subseccion("050.020.030.000", experienciaCientificaTecnologica.SincroObrasArtisticas(UtilitySecciones.CheckSecciones(secciones, "050.020.030.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("050.030.020.000", experienciaCientificaTecnologica.SincroResultadosTecnologicos(UtilitySecciones.CheckSecciones(secciones, "050.030.020.000"), preimportar, listadoIdBBDD, petitionStatus)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad científica y tecnológica.
        /// Con el codigo identificativo 060.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            ActividadCientificaTecnologica actividadCientificaTecnologica = new ActividadCientificaTecnologica(cvn, cvID, personID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("060.010.000.000", actividadCientificaTecnologica.SincroProduccionCientifica(UtilitySecciones.CheckSecciones(secciones, "060.010.000.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.060.010", actividadCientificaTecnologica.SincroIndicadoresGenerales(UtilitySecciones.CheckSecciones(secciones, "060.010.060.010"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.010.000", actividadCientificaTecnologica.SincroPublicacionesDocumentos(mConfiguracion, UtilitySecciones.CheckSecciones(secciones, "060.010.010.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.020.000", actividadCientificaTecnologica.SincroTrabajosCongresos(mConfiguracion,UtilitySecciones.CheckSecciones(secciones, "060.010.020.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.030.000", actividadCientificaTecnologica.SincroTrabajosJornadasSeminarios(mConfiguracion,UtilitySecciones.CheckSecciones(secciones, "060.010.030.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.040.000", actividadCientificaTecnologica.SincroOtrasActividadesDivulgacion(UtilitySecciones.CheckSecciones(secciones, "060.010.040.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.010.000", actividadCientificaTecnologica.SincroComitesCTA(UtilitySecciones.CheckSecciones(secciones, "060.020.010.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.030.000", actividadCientificaTecnologica.SincroOrganizacionIDI(UtilitySecciones.CheckSecciones(secciones, "060.020.030.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.040.000", actividadCientificaTecnologica.SincroGestionIDI(UtilitySecciones.CheckSecciones(secciones, "060.020.040.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.050.000", actividadCientificaTecnologica.SincroForosComites(UtilitySecciones.CheckSecciones(secciones, "060.020.050.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.060.000", actividadCientificaTecnologica.SincroEvalRevIDI(UtilitySecciones.CheckSecciones(secciones, "060.020.060.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.010.050.000", actividadCientificaTecnologica.SincroEstanciasIDI(UtilitySecciones.CheckSecciones(secciones, "060.010.050.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.010.000", actividadCientificaTecnologica.SincroAyudasBecas(UtilitySecciones.CheckSecciones(secciones, "060.030.010.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.020.020.000", actividadCientificaTecnologica.SincroOtrosModosColaboracion(UtilitySecciones.CheckSecciones(secciones, "060.020.020.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.020.000", actividadCientificaTecnologica.SincroSociedadesAsociaciones(UtilitySecciones.CheckSecciones(secciones, "060.030.020.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.030.000", actividadCientificaTecnologica.SincroConsejos(UtilitySecciones.CheckSecciones(secciones, "060.030.030.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.040.000", actividadCientificaTecnologica.SincroRedesCooperacion(UtilitySecciones.CheckSecciones(secciones, "060.030.040.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.050.000", actividadCientificaTecnologica.SincroPremiosMenciones(UtilitySecciones.CheckSecciones(secciones, "060.030.050.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.060.000", actividadCientificaTecnologica.SincroOtrasDistinciones(UtilitySecciones.CheckSecciones(secciones, "060.030.060.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.070.000", actividadCientificaTecnologica.SincroPeriodosActividad(UtilitySecciones.CheckSecciones(secciones, "060.030.070.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.090.000", actividadCientificaTecnologica.SincroAcreditacionesObtenidas(UtilitySecciones.CheckSecciones(secciones, "060.030.090.000"), preimportar, listadoIdBBDD, petitionStatus)));
            listadoSecciones.Add(new Subseccion("060.030.100.000", actividadCientificaTecnologica.SincroResumenOtrosMeritos(UtilitySecciones.CheckSecciones(secciones, "060.030.100.000"), preimportar, listadoIdBBDD, petitionStatus)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Resumen Texto Libre.
        /// Con el codigo identificativo 070.010.000.000
        /// </summary>
        /// <param name="preimportar"></param>
        /// <returns></returns>
        public List<Subseccion> SincroTextoLibre([Optional] List<string> secciones, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID, mConfiguracion);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("070.010.000.000", textoLibre.SincroTextoLibre(UtilitySecciones.CheckSecciones(secciones, "070.010.000.000"), preimportar, listadoIdBBDD)));

            return listadoSecciones;
        }
    }
}
