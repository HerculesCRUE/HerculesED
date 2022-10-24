using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WoSConnect.ROs.WoS.Controllers;
using WoSConnect.ROs.WoS.Models;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;

namespace WoSConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("WoS/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        public Dictionary<string, string> ds = LeerDatosExcel();
        const string pRuta = @"Files/Taxonomy.xlsx";

        //Resource API.
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        
        public APIController(ILogger<APIController> logger)
        {
            _logger = logger;
        }

        public static Dictionary<string, string> LeerDatosExcel()
        {
            DataSet ds = new DataSet();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //FileStream uploadFileStream = System.IO.File.OpenRead(pRuta);

            using (var stream = System.IO.File.Open(pRuta, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    });
                }
            }
            Dictionary<string, string> name = new Dictionary<string, string>();
            foreach (DataRow fila in ds.Tables["Hércules-KA-Scopus-WoS"].Rows)
            {
                if (fila["WoS-JCR descriptor"].ToString() != "")
                {
                    // Tuple<string, string, string, string> tupla = new Tuple<string, string, string, string>(fila["Level 0"].ToString(), fila["Level 1"].ToString(), fila["Level 2"].ToString(), fila["Level 3"].ToString());
                    name[fila["WoS-JCR descriptor"].ToString()] = fila["Hércules-KA"].ToString();
                }
            }
            return name;
        }
        

        /// <summary>
        /// Get all repositories from a specified user account and RO
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     GET /WoS/GetROs?orcid=--
        ///     GET http://localhost:5000/WoS/GetROs?orcid=
        /// </remarks>
        /// <param orcid="orcid">Orcid</param>
        /// <param date="Year-Month-Day">Orcid</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Invalid app</response> 
        /// <response code="500">Oops! Something went wrong</response> 

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetROs([FromQuery][Required] string orcid, string date = "1500-01-01")
        {
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8", this.ds, mResourceApi);//, @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-CommonsEDMA_Taxonomías_v1.2.xlsx");//"adf94bebeeba8c3042ad5193455740e2");
            List<Publication> publication = WoSObject.getPublications(orcid, date);
            return publication;
        }

        /// <summary>
        /// Permite obtener la información de una publicación mediante el identificador de WoS (Web Of Science).
        /// </summary>
        /// <param name="pIdWos">ID de la publicación.</param>
        /// <returns>Objeto con los datos recuperados.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication GetRoByWosId([FromQuery][Required] string pIdWos)
        {
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8", this.ds, mResourceApi);//, @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-CommonsEDMA_Taxonomías_v1.2.xlsx");//"adf94bebeeba8c3042ad5193455740e2");
            Publication publication = WoSObject.getPublicationWos(pIdWos);
            return publication;
        }

        /// <summary>
        /// Permite obtener la información de una publicación mediante el identificador de WoS (Web Of Science).
        /// </summary>
        /// <param name="pDoi">DOI de la publicación.</param>
        /// <returns>Objeto con los datos recuperados.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Publication GetRoByDoi([FromQuery][Required] string pDoi)
        {
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8", this.ds, mResourceApi);//, @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-CommonsEDMA_Taxonomías_v1.2.xlsx");//"adf94bebeeba8c3042ad5193455740e2");
            Publication publication = WoSObject.getPublicationDoi(pDoi);
            return publication;
        }

        /// <summary>
        /// Permite obtener las citas de una publicación mediante el DOI.
        /// </summary>
        /// <param name="pDoi">DOI de la publicación.</param>
        /// <returns>Objeto con los datos recuperados.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public List<Publication> GetCitesByWosId([FromQuery][Required] string pWosId)
        {
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8", this.ds, mResourceApi);//, @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-CommonsEDMA_Taxonomías_v1.2.xlsx");//"adf94bebeeba8c3042ad5193455740e2");
            List<Publication> publication = WoSObject.getCitingByWosId(pWosId);
            return publication;
        }
    }
}

