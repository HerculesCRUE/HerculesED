using Hercules.ED.ImportadorWebCV.Models;
using ImportadorWebCV.Sincro;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Gnoss.ApiWrapper.Model;
using Models;
using ImportadorWebCV.Exporta.Secciones;
using Gnoss.ApiWrapper;

namespace Hercules.ED.ImportadorWebCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportadorCVController : ControllerBase
    {
        private readonly ILogger<ImportadorCVController> _logger;
        readonly ConfigService _Configuracion;

        public ImportadorCVController(ILogger<ImportadorCVController> logger, ConfigService pConfig)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _logger = logger;
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Importa los datos del <paramref name="File"/> al curriculum vitae, que corresponde a <paramref name="pCVID"/>
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <returns></returns>
        [HttpPost("Importar")]
        public ActionResult Importar([FromHeader][Required] string pCVID, [Required] IFormFile File, [FromHeader][Optional] List<string> Secciones)
        {
            try
            {
                SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);

                sincro.SincroDatosIdentificacion(Secciones);
                sincro.SincroDatosSituacionProfesional(Secciones);
                sincro.SincroFormacionAcademica(Secciones);
                sincro.SincroActividadDocente(Secciones);
                sincro.SincroExperienciaCientificaTecnologica(Secciones);
                sincro.SincroActividadCientificaTecnologica(Secciones);
                sincro.SincroTextoLibre(Secciones);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Devuelve los datos del <paramref name="File"/> , que corresponden a <paramref name="pCVID"/>,
        /// para elegir cuales importar.
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <returns>Json con los datos a preimportar</returns>
        [HttpPost("Preimportar")]
        public ActionResult Preimportar([FromForm][Required] string pCVID, [Required] IFormFile File, [FromForm][Optional] List<string> Secciones)
        {
            try
            {
                SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);
                Preimport preimportar = new Preimport();

                preimportar.secciones.AddRange(sincro.SincroDatosIdentificacion(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroDatosSituacionProfesional(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroFormacionAcademica(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroActividadDocente(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroExperienciaCientificaTecnologica(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroActividadCientificaTecnologica(Secciones, true));
                preimportar.secciones.AddRange(sincro.SincroTextoLibre(Secciones, true));

                string xml = "";
                XmlSerializer serializer = new XmlSerializer(typeof(Preimport));
                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        serializer.Serialize(writer, preimportar);
                        xml = sww.ToString();
                    }
                }
                preimportar.cvn_xml = xml;

                return Ok(preimportar);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Postimportar")]
        public ActionResult PostImportar([FromForm][Required] string pCVID, [FromForm] string fileData, [FromForm] List<string> listaId, [FromForm][Optional] List<string> listaOpciones)
        {
            try
            {
                SincroDatos sincroDatos = new SincroDatos(_Configuracion, pCVID, fileData);

                AccionesImportacion accionesImportacion = new AccionesImportacion();
                accionesImportacion.ImportacionTriples(sincroDatos, pCVID, listaId, listaOpciones);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
