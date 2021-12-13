using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SemanticScholarConnect.ROs.SemanticScholar.Controllers;
using SemanticScholarConnect.ROs.SemanticScholar.Models;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;


namespace SemanticScholarConnect.Controllers.autores{

    public class almacenamiento_autores{

    public Dictionary<string, Tuple<string, string, string, string, string, string>> autores_orcid ;

        public almacenamiento_autores(){
            this.autores_orcid=LeerDatosExcel_autores(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx");
        }
        public Boolean guardar_info_autores()
        {
            //DataSet ds = new DataSet();
            //el nombre que le des aqui es el de la hoja de excel! 
            DataTable dt = new DataTable("orcids");
            DataRow row;

            // Create third column.
            DataColumn column = new DataColumn();
            column.ColumnName = "id";
            column.Unique = true;
            dt.Columns.Add(column);
           
            column = new DataColumn();

            //column.DataType = System.Type.GetType("System.string");
            column.ColumnName = "ORCID";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Name";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Apellido";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Nombres";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "ids";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Links";
            dt.Columns.Add(column);

            foreach (string key in this.autores_orcid.Keys)
            {
                row = dt.NewRow();
                row["id"] = key;
                row["ORCID"] = this.autores_orcid[key].Item1;
                row["Name"] = this.autores_orcid[key].Item2;
                row["Apellido"] = this.autores_orcid[key].Item3;
                row["Nombres"] = this.autores_orcid[key].Item4;
                row["ids"] = this.autores_orcid[key].Item5;
                row["Links"] = this.autores_orcid[key].Item6;
                dt.Rows.Add(row);
            }

            //ds.Tables.Add(dt);
            string pRuta = @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx";

            using (var workbook = new XLWorkbook())
            {
                //var worksheet = workbook.Worksheets.Add("Sample Sheet");
                workbook.Worksheets.Add(dt);
                workbook.SaveAs(pRuta);
            }
            return true;
        }

        
        public static Dictionary<string, Tuple<string, string, string, string, string, string>> LeerDatosExcel_autores(string pRuta)
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
            Dictionary<string, Tuple<string, string, string, string, string, string>> autores = new Dictionary<string, Tuple<string, string, string, string, string, string>>();
            foreach (DataRow fila in ds.Tables["Orcids"].Rows)
            {
                if (fila["id"].ToString() != "")
                {
                    Tuple<string, string, string, string, string, string> tupla = new Tuple<string, string, string, string, string, string>(
                        fila["ORCID"].ToString(), fila["Name"].ToString(), fila["Apellido"].ToString(),
                        fila["Nombres"].ToString(),
                         fila["Ids"].ToString(), fila["Links"].ToString());
                    autores[fila["id"].ToString()] = tupla;
                }
            }

            return autores;
        }


    }

}