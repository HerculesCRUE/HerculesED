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
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;
using WoSConnect.Controllers.autores;



namespace WoSConnect.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("WoS/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        public Dictionary<string, string> ds = LeerDatosExcel(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-ED_Taxonomía-Unificada_Scopus-WoS_v1.2.xlsx");
       // public Dictionary<string, Tuple<string, string, string, string, string, string>> autores_orcid;// = LeerDatosExcel_autores(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx");

        public APIController(ILogger<APIController> logger)
        {
            _logger = logger;
        }
        public static Dictionary<string, string> LeerDatosExcel(string pRuta)
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


        // public static Dictionary<string, Tuple<string, string, string, string, string, string>> LeerDatosExcel_autores(string pRuta)
        // {
        //     DataSet ds = new DataSet();

        //     Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //     //FileStream uploadFileStream = System.IO.File.OpenRead(pRuta);

        //     using (var stream = System.IO.File.Open(pRuta, FileMode.Open, FileAccess.Read))
        //     {
        //         using (var reader = ExcelReaderFactory.CreateReader(stream))
        //         {
        //             ds = reader.AsDataSet(new ExcelDataSetConfiguration()
        //             {
        //                 ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
        //                 {
        //                     UseHeaderRow = true,
        //                 }
        //             });
        //         }
        //     }
        //     Dictionary<string, Tuple<string, string, string, string, string, string>> autores = new Dictionary<string, Tuple<string, string, string, string, string, string>>();
        //     foreach (DataRow fila in ds.Tables["Orcids"].Rows)
        //     {
        //         if (fila["id"].ToString() != "")
        //         {
        //             Tuple<string, string, string, string, string, string> tupla = new Tuple<string, string, string, string, string, string>(
        //                 fila["ORCID"].ToString(), fila["Name"].ToString(), fila["Apellido"].ToString(),
        //                 fila["Nombres"].ToString(),
        //                  fila["Ids"].ToString(), fila["Links"].ToString());
        //             autores[fila["id"].ToString()] = tupla;
        //         }
        //     }

        //     return autores;
        // }
        // Person persona_con_orcid= new Person();
        // persona_con_orcid.ORCID = fila["ORCID"].ToString();
        // string[] names_persona = fila["Name"].ToString().Split(";");
        // Name nombre_persona_orcid = new Name();
        // List<string> given = new List<string>();
        // foreach(string name_concreto in names_persona){
        //     given.Add(name_concreto);
        // }
        // nombre_persona_orcid.given=given;
        // string[] apellido_persona = fila["Apellido"].ToString().Split(";");

        // List<string> apellidos = new List<string>();
        // foreach(string apellido_concreto in apellido_persona){
        //     apellidos.Add(apellido_concreto);
        // }
        // nombre_persona_orcid.familia=apellidos; 

        //  string[] nombre_completo_persona = fila["Nombres"].ToString().Split(";");

        // List<string> nombles_completos = new List<string>();
        // foreach(string nombre_completo_concreto in nombre_completo_persona){
        //     nombles_completos.Add(nombre_completo_concreto);
        // }
        // nombre_persona_orcid.nombre_completo=nombles_completos; 

        //todo! los links y los ids


        // Tuple<string, string, string, string> tupla = new Tuple<string, string, string, string>(fila["Level 0"].ToString(), fila["Level 1"].ToString(), fila["Level 2"].ToString(), fila["Level 3"].ToString());
        // autores[fila["ORCID"].ToString()] = persona_con_orcid;
        // }
        // }


        // public Boolean escribri_datos_autores(Dictionary<string, Person> autores_orcid)
        // {


        // }

        // public Boolean guardar_info_autores()
        // {
        //     //DataSet ds = new DataSet();
        //     //el nombre que le des aqui es el de la hoja de excel! 
        //     DataTable dt = new DataTable("orcids");
        //     DataRow row;

        //     // Create third column.
        //     DataColumn column = new DataColumn();
        //     column.ColumnName = "id";
        //     column.Unique = true;
        //     dt.Columns.Add(column);
           
        //     column = new DataColumn();

        //     //column.DataType = System.Type.GetType("System.string");
        //     column.ColumnName = "ORCID";
        //     dt.Columns.Add(column);

        //     column = new DataColumn();
        //     column.ColumnName = "Name";
        //     dt.Columns.Add(column);

        //     column = new DataColumn();
        //     column.ColumnName = "Apellido";
        //     dt.Columns.Add(column);

        //     column = new DataColumn();
        //     column.ColumnName = "Nombres";
        //     dt.Columns.Add(column);

        //     column = new DataColumn();
        //     column.ColumnName = "ids";
        //     dt.Columns.Add(column);

        //     column = new DataColumn();
        //     column.ColumnName = "Links";
        //     dt.Columns.Add(column);

        //     foreach (string key in this.autores_orcid.Keys)
        //     {
        //         row = dt.NewRow();
        //         row["id"] = key;
        //         row["ORCID"] = this.autores_orcid[key].Item1;
        //         row["Name"] = this.autores_orcid[key].Item2;
        //         row["Apellido"] = this.autores_orcid[key].Item3;
        //         row["Nombres"] = this.autores_orcid[key].Item4;
        //         row["ids"] = this.autores_orcid[key].Item5;
        //         row["Links"] = this.autores_orcid[key].Item6;
        //         dt.Rows.Add(row);
        //     }

        //     //ds.Tables.Add(dt);
        //     string pRuta = @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx";

        //     using (var workbook = new XLWorkbook())
        //     {
        //         //var worksheet = workbook.Worksheets.Add("Sample Sheet");
        //         workbook.Worksheets.Add(dt);
        //         workbook.SaveAs(pRuta);
        //     }
        //     return true;
        // }

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
            almacenamiento_autores almacenamiento = new almacenamiento_autores();
            ROWoSController WoSObject = new ROWoSController("https://wos-api.clarivate.com/", "10e8a3a2417b7ae1d864b5558136c56b78ed3eb8", this.ds, almacenamiento.autores_orcid);//, @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hércules-ED_Taxonomías_v1.2.xlsx");//"adf94bebeeba8c3042ad5193455740e2");
            List<Publication> publication = WoSObject.getPublications(orcid, date);
            //guardar_info_autores();
            almacenamiento.guardar_info_autores();
            return publication;
        }


    }
}

