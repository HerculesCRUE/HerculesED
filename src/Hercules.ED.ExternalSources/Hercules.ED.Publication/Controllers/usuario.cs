using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Text;
using ExcelDataReader;
using System.Linq;


namespace PublicationConnect.Controllers.autores
{

    public class almacenamiento_autores
    {

        public Dictionary<string, Tuple<string, string, string, string, string, string>> autores_orcid;
        public Dictionary<string, Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>> usuarios_unicos;

        public almacenamiento_autores()
        {
            this.autores_orcid = LeerDatosExcel_autores(@"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores.xlsx");
            this.usuarios_unicos = new Dictionary<string, Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>>();
            //  id      ->  ids_antiguos, orcid,     name,        apellidos,        nombre   ids,       url 
        }


        public float GetNameSimilarity(string pFirma, string pTarget)
        {
            pFirma = ObtenerTextosFirmasNormalizadas(pFirma);
            pTarget = ObtenerTextosFirmasNormalizadas(pTarget);

            //Almacenamos los scores de cada una de las palabras
            List<float> scores = new List<float>();

            string[] pFirmaNormalizadoSplit = pFirma.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] pTargetNormalizadoSplit = pTarget.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] source = pFirmaNormalizadoSplit;
            string[] target = pTargetNormalizadoSplit;

            int indexTarget = 0;
            for (int i = 0; i < source.Length; i++)
            {
                //Similitud real
                float score = 0;
                string wordSource = source[i];
                bool wordSourceInicial = wordSource.Length == 1;
                //int desplazamiento = 0;
                for (int j = indexTarget; j < target.Length; j++)
                {
                    string wordTarget = target[j];
                    bool wordTargetInicial = wordTarget.Length == 1;
                    //Alguna de las dos es inicial
                    if (wordSourceInicial || wordTargetInicial)
                    {
                        if (wordSourceInicial != wordTargetInicial)
                        {
                            //No son las dos iniciales
                            if (wordSource[0] == wordTarget[0])
                            {
                                score = 0.5f;
                                indexTarget = j + 1;
                                //desplazamiento = Math.Abs(j - i);
                                break;
                            }
                        }
                        else
                        {
                            //Son las dos iniciales
                            score = 0.75f;
                            indexTarget = j + 1;
                            //desplazamiento = Math.Abs(j - i);
                            break;
                        }
                    }
                    float scoreSingleName = CompareSingleName(wordSource, wordTarget);
                    if (scoreSingleName > 0)
                    {
                        score = scoreSingleName;
                        indexTarget = j + 1;
                        break;
                    }
                }
                scores.Add(score);
            }
            if (scores.Count > 0)
            {
                return scores.Sum() / source.Length;
            }
            return 0;
        }

        private string ObtenerTextosFirmasNormalizadas(string pText)
        {
            pText = pText.ToLower();
            pText = pText.Trim();
            if (pText.Contains(","))
            {
                pText = (pText.Substring(pText.IndexOf(",") + 1)).Trim() + " " + (pText.Substring(0, pText.IndexOf(","))).Trim();
            }
            pText = pText.Replace("-", " ");
            string textoNormalizado = pText.Normalize(NormalizationForm.FormD);
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^a-zA-Z ]");
            string textoSinAcentos = reg.Replace(textoNormalizado, "");
            while (textoSinAcentos.Contains(" del "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" del ", " ");
            }
            while (textoSinAcentos.Contains(" de "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" de ", " ");
            }
            while (textoSinAcentos.Contains(" la "))
            {
                textoSinAcentos = textoSinAcentos.Replace(" la ", " ");
            }
            while (textoSinAcentos.Contains("  "))
            {
                textoSinAcentos = textoSinAcentos.Replace("  ", " ");
            }

            return textoSinAcentos.Trim();
        }

        private float CompareSingleName(string pNameA, string pNameB)
        {
            HashSet<string> ngramsNameA = GetNGramas(pNameA, 2);
            HashSet<string> ngramsNameB = GetNGramas(pNameB, 2);
            float tokens_comunes = ngramsNameA.Intersect(ngramsNameB).Count();
            float union_tokens = ngramsNameA.Union(ngramsNameB).Count();
            float coeficiente_jackard = tokens_comunes / union_tokens;
            return coeficiente_jackard;
        }

        private HashSet<string> GetNGramas(string pText, int pNgramSize)
        {
            HashSet<string> ngramas = new HashSet<string>();
            int textLength = pText.Length;
            if (pNgramSize == 1)
            {
                for (int i = 0; i < textLength; i++)
                {
                    ngramas.Add(pText[i].ToString());
                }
                return ngramas;
            }

            HashSet<string> ngramasaux = new HashSet<string>();
            for (int i = 0; i < textLength; i++)
            {
                foreach (string ngram in ngramasaux.ToList())
                {
                    string ngamaux = ngram + pText[i];
                    if (ngamaux.Length == pNgramSize)
                    {
                        ngramas.Add(ngamaux);
                    }
                    else
                    {
                        ngramasaux.Add(ngamaux);
                    }
                    ngramasaux.Remove(ngram);
                }
                ngramasaux.Add(pText[i].ToString());
                if (i < pNgramSize)
                {
                    foreach (string ngrama in ngramasaux)
                    {
                        if (ngrama.Length == i + 1)
                        {
                            ngramas.Add(ngrama);
                        }
                    }
                }
            }
            for (int i = (textLength - pNgramSize) + 1; i < textLength; i++)
            {
                if (i >= pNgramSize)
                {
                    ngramas.Add(pText.Substring(i));
                }
            }
            return ngramas;
        }

        public Boolean guardar_info_autores()
        {
            DataSet ds = new DataSet();
            // el nombre que le des aqui es el de la hoja de excel! 
            DataTable dt = new DataTable("orcids");
            DataRow row;

            // Create third column.
            DataColumn column = new DataColumn();
            column.ColumnName = "id";
            column.Unique = true;
            dt.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "ids_antiguo";
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

            foreach (string key in this.usuarios_unicos.Keys)
            {
                row = dt.NewRow();
                row["id"] = key;

                row["ORCID"] = this.usuarios_unicos[key].Item2;
                string ids = "";
                foreach (string id_antiguo in this.usuarios_unicos[key].Item1)
                {
                    ids = ids + "*" + id_antiguo;
                }
                row["ids_antiguo"] = ids;

                string names = "";
                foreach (string name in this.usuarios_unicos[key].Item3)
                {
                    names = names + "*" + name;
                }
                row["ids_antiguo"] = ids;
                row["Name"] = names;

                string apellidos = "";
                foreach (string apellido in this.usuarios_unicos[key].Item4)
                {
                    apellidos = apellidos + "*" + apellido;
                }
                row["Apellido"] = apellidos;
                string nombres = "";
                foreach (string nombre in this.usuarios_unicos[key].Item5)
                {
                    nombres = nombres + "*" + nombre;
                }

                row["Nombres"] = nombres;
                string idss = "";
                foreach (string id in this.usuarios_unicos[key].Item6)
                {
                    idss = idss + "*" + id;
                }

                row["ids"] = idss;
                string links = "";
                foreach (string link in this.usuarios_unicos[key].Item7)
                {
                    links = links + "*" + link;
                }

                row["Links"] = links;
                dt.Rows.Add(row);
            }

            ds.Tables.Add(dt);
            string pRuta = @"C:\Users\mpuer\Documents\GitHub\HerculesED\src\Hercules.ED.ExternalSources\Hercules-ED_autores_2.xlsx";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sample Sheet");
                workbook.Worksheets.Add(dt);
                workbook.SaveAs(pRuta);
            }
            return true;
        }

        // leer datos del excel. 
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
        public static Dictionary<string, Tuple<string, string, string, string, string, string>> LeerDatosExcel_JIF(string pRuta)
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
        // 
        public Boolean unificar_personas()
        {
            int id_nuevos = 0;
            List<string> list_id = this.autores_orcid.Keys.ToList();
            // List<string> ids_orcid = new List<string>();

            foreach (string id in list_id)
            {
                Console.Write("Primer id: " + id + "\n");
                string orcid = this.autores_orcid[id].Item1;
                string name = this.autores_orcid[id].Item2;
                string familia = this.autores_orcid[id].Item3;
                string completo = this.autores_orcid[id].Item4;
                string ids = this.autores_orcid[id].Item5;
                string links = this.autores_orcid[id].Item6;


                //datos que tenemos del usuario unificados! Estos seran los que se use siempre que esta persona se ha detectado. 
                List<string> list_antiguos_id; //= new List<string>();
                string orcid_unificado;// = "";
                List<string> list_name;// = new List<string>();
                List<string> list_familia; //= new List<string>(); ;
                List<string> list_nombre_completo;// = new List<string>();
                List<string> list_ids;// = new List<string>();
                List<string> list_links;// = new List<string>();


                List<string> list_id_NUEVO = this.usuarios_unicos.Keys.ToList();
                List<string> id_nuevos_que_son_la_misma_persona = new List<string>(); ;

                if (list_id_NUEVO.Count == 0)
                {
                    list_antiguos_id = new List<string>();
                    orcid_unificado = "";
                    list_name = new List<string>();
                    list_familia = new List<string>(); ;
                    list_nombre_completo = new List<string>();
                    list_ids = new List<string>();
                    list_links = new List<string>();

                    Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>> tupla =
                        new Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>(
                            list_antiguos_id, orcid_unificado, list_name, list_familia, list_nombre_completo, list_ids, list_links);
                    this.usuarios_unicos[id_nuevos.ToString()] = tupla;
                    completar_informacion_unificada(id, id_nuevos.ToString());
                    id_nuevos = id_nuevos + 1;
                }
                else if (list_id_NUEVO.Count > 0)
                {

                    foreach (string id_2 in this.usuarios_unicos.Keys)
                    {
                        Console.Write("id_diccionario_nuevo: " + id_2 + "\n");
                        list_antiguos_id = this.usuarios_unicos[id_2].Item1;
                        orcid_unificado = this.usuarios_unicos[id_2].Item2;
                        list_name = this.usuarios_unicos[id_2].Item3;
                        list_familia = this.usuarios_unicos[id_2].Item4;
                        list_nombre_completo = this.usuarios_unicos[id_2].Item5;
                        list_ids = this.usuarios_unicos[id_2].Item6;
                        list_links = this.usuarios_unicos[id_2].Item7;

                        if (orcid_unificado != "" & orcid == orcid_unificado)
                        {

                            completar_informacion_unificada(id, id_2);
                            id_nuevos_que_son_la_misma_persona.Add(id_2);

                        }
                        else if (list_ids.Contains(ids))
                        {
                            completar_informacion_unificada(id, id_2);
                            id_nuevos_que_son_la_misma_persona.Add(id_2);
                        }
                        else
                        {
                            if ((orcid != "" & orcid_unificado != "") || (list_id.Count > 0 & ids != ""))
                            {
                                continue;
                            }
                            else
                            {
                                if (list_nombre_completo.Count > 0)
                                {
                                    // cuando en los datos de la persona unificada tenemos el nombre completo 
                                    //foreach (string nombre_completo in list_nombre_completo)
                                    for (int k = 0; k < list_nombre_completo.Count; k++)
                                    {
                                        string nombre_completo = list_nombre_completo[k];
                                        if (name != "" & familia != "")
                                        {
                                            if (GetNameSimilarity(name + " " + familia, nombre_completo) > 0.87 ||
                                                GetNameSimilarity(name.Substring(0, 1) + "." + " " + familia, nombre_completo) > 0.87 & name.Substring(0, 1) == nombre_completo.Substring(0, 1) ||
                                                GetNameSimilarity(familia + ", " + name.Substring(0, 1) + ".", nombre_completo) > 0.87 & nombre_completo.Substring(nombre_completo.Count() - 2, 2) == name.Substring(0, 1) + "." ||
                                                GetNameSimilarity(familia + " " + name.Substring(0, 1) + ".", nombre_completo) > 0.87)
                                            {
                                                if (ids != "" & list_id.Count > 0 & !list_id.Contains(ids))
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (!id_nuevos_que_son_la_misma_persona.Contains(id_2))
                                                    {
                                                        completar_informacion_unificada(id, id_2);

                                                        id_nuevos_que_son_la_misma_persona.Add(id_2);
                                                    }
                                                }
                                            }
                                        }
                                        if (completo != "")
                                        {
                                            if (GetNameSimilarity(completo, nombre_completo) > 0.9)
                                            {
                                                if (ids != "" & list_id.Count > 0 & !list_id.Contains(ids))
                                                {
                                                    Console.Write("------------------------------------------------------------------");
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (!id_nuevos_que_son_la_misma_persona.Contains(id_2))
                                                    {
                                                        completar_informacion_unificada(id, id_2);

                                                        id_nuevos_que_son_la_misma_persona.Add(id_2);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // cuando en los datos de la persona unificada tenemos el nombre completo 
                                else if (list_name.Count > 0 & list_familia.Count > 0)
                                {
                                    for (int j = 0; j < list_name.Count; j++)
                                    {
                                        string name_unificado = list_name[j];
                                        for (int i = 0; i < list_familia.Count; i++)
                                        {
                                            if (name != "" & familia != "")
                                            {
                                                if (GetNameSimilarity(name + " " + familia, name_unificado + " " + list_familia[i]) > 0.87)
                                                {
                                                    if (ids != "" & list_id.Count > 0 & !list_id.Contains(ids))
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        if (!id_nuevos_que_son_la_misma_persona.Contains(id_2))
                                                        {
                                                            completar_informacion_unificada(id, id_2);

                                                            id_nuevos_que_son_la_misma_persona.Add(id_2);
                                                        }
                                                    }


                                                }
                                            }
                                            if (completo != "")
                                            {

                                                if (GetNameSimilarity(name_unificado + " " + list_familia[i], completo) > 0.87 ||
                                                    GetNameSimilarity(name_unificado.Substring(0, 1) + ". " + familia, completo) > 0.87 & name_unificado.Substring(0, 1) == completo.Substring(0, 1) ||
                                                    GetNameSimilarity(list_familia[i] + ", " + name_unificado.Substring(0, 1) + ".", completo) > 0.87 & completo.Substring(completo.Count() - 2, 2) == name_unificado.Substring(0, 1) + "." ||
                                                    GetNameSimilarity(list_familia[i] + " " + name_unificado.Substring(0, 1) + ".", completo) > 0.87)
                                                {
                                                    if (ids != "" & list_id.Count > 0 & !list_id.Contains(ids))
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        if (!id_nuevos_que_son_la_misma_persona.Contains(id_2))
                                                        {
                                                            completar_informacion_unificada(id, id_2);

                                                            id_nuevos_que_son_la_misma_persona.Add(id_2);
                                                        }
                                                    }

                                                }
                                            }

                                        }
                                    }
                                }

                            }

                        }

                    }
                }

                if (id_nuevos_que_son_la_misma_persona.Count == 0)
                {
                    list_antiguos_id = new List<string>();
                    orcid_unificado = "";
                    list_name = new List<string>();
                    list_familia = new List<string>(); ;
                    list_nombre_completo = new List<string>();
                    list_ids = new List<string>();
                    list_links = new List<string>();

                    Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>> tupla =
                        new Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>(
                            list_antiguos_id, orcid_unificado, list_name, list_familia, list_nombre_completo, list_ids, list_links);
                    this.usuarios_unicos[id_nuevos.ToString()] = tupla;
                    completar_informacion_unificada(id, id_nuevos.ToString());
                    id_nuevos = id_nuevos + 1;

                }
                else if (id_nuevos_que_son_la_misma_persona.Count > 1)
                {
                    // tengo dos usuarios que creia que eran igual y no lo son... 
                    for (int i = 0; i < id_nuevos_que_son_la_misma_persona.Count; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        else
                        {
                            unir_dos_identidades_unicas(id_nuevos_que_son_la_misma_persona[0], id_nuevos_que_son_la_misma_persona[i]);
                        }
                    }


                }

            }
            return true;
        }
        public Boolean unir_dos_identidades_unicas(string id, string id_2)
        {
            List<string> list_antiguos_id = this.usuarios_unicos[id].Item1;
            string orcid_unificado = this.usuarios_unicos[id].Item2;
            List<string> list_name = this.usuarios_unicos[id].Item3;
            List<string> list_familia = this.usuarios_unicos[id].Item4;
            List<string> list_nombre_completo = this.usuarios_unicos[id].Item5;
            List<string> list_ids = this.usuarios_unicos[id].Item6;
            List<string> list_links = this.usuarios_unicos[id].Item7;

            List<string> list_antiguos_id2 = this.usuarios_unicos[id_2].Item1;
            List<string> list_name_id2 = this.usuarios_unicos[id_2].Item3;
            List<string> list_familia_id2 = this.usuarios_unicos[id_2].Item4;
            List<string> list_nombre_completo_id2 = this.usuarios_unicos[id_2].Item5;
            List<string> list_ids_id_2 = this.usuarios_unicos[id_2].Item6;
            List<string> list_links_id_2 = this.usuarios_unicos[id_2].Item7;

            if (list_antiguos_id2.Count > 0)
            {
                foreach (string ids_antiguos in list_antiguos_id2)
                {
                    if (!list_antiguos_id.Contains(ids_antiguos))
                    {
                        list_antiguos_id.Add(ids_antiguos);
                    }
                }
            }
            if (list_name_id2.Count > 0)
            {
                foreach (string name_ids2 in list_name_id2)
                {
                    if (!list_name_id2.Contains(name_ids2))
                    {
                        list_name_id2.Add(name_ids2);
                    }
                }
            }
            if (list_familia_id2.Count > 0)
            {
                foreach (string familia_ids2 in list_familia_id2)
                {
                    if (!list_familia.Contains(familia_ids2))
                    {
                        list_familia.Add(familia_ids2);
                    }
                }
            }
            if (list_nombre_completo_id2.Count > 0)
            {
                foreach (string completo_ids2 in list_nombre_completo_id2)
                {
                    if (!list_nombre_completo.Contains(completo_ids2))
                    {
                        list_nombre_completo.Add(completo_ids2);
                    }
                }
            }
            if (list_ids_id_2.Count > 0)
            {
                foreach (string ids2 in list_ids_id_2)
                {
                    if (!list_ids.Contains(ids2))
                    {
                        list_ids.Add(ids2);
                    }
                }
            }
            if (list_links_id_2.Count > 0)
            {
                foreach (string links2 in list_links_id_2)
                {
                    if (!list_links.Contains(links2))
                    {
                        list_links.Add(links2);
                    }
                }
            }
            Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>> tupla =
                            new Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>(
                                list_antiguos_id, orcid_unificado, list_name, list_familia, list_nombre_completo, list_ids, list_links);
            this.usuarios_unicos[id] = tupla;
            this.usuarios_unicos.Remove(id_2);
            return true;
        }





        public Boolean completar_informacion_unificada(string id, string id_2)
        {
            // datos de la fila del excel que estamos analizando! 
            string orcid = this.autores_orcid[id].Item1;
            string name = this.autores_orcid[id].Item2;
            string familia = this.autores_orcid[id].Item3;
            string completo = this.autores_orcid[id].Item4;
            string ids = this.autores_orcid[id].Item5;
            string links = this.autores_orcid[id].Item6;
            //Console.Write("Segundo id: " + id_2 + "\n");

            //datos que tenemos del usuario unificados! Estos seran los que se use siempre que esta persona se ha detectado. 
            List<string> list_antiguos_id = this.usuarios_unicos[id_2].Item1;
            string orcid_unificado = this.usuarios_unicos[id_2].Item2;
            List<string> list_name = this.usuarios_unicos[id_2].Item3;
            List<string> list_familia = this.usuarios_unicos[id_2].Item4;
            List<string> list_nombre_completo = this.usuarios_unicos[id_2].Item5;
            List<string> list_ids = this.usuarios_unicos[id_2].Item6;
            List<string> list_links = this.usuarios_unicos[id_2].Item7;

            list_antiguos_id.Add(id);
            if (orcid_unificado == "" & orcid != "")
            {
                orcid_unificado = orcid;
            }
            if (!list_name.Contains(name) && name != "")
            {
                list_name.Add(name);
            }
            if (!list_familia.Contains(familia) && familia != "")
            {
                list_familia.Add(familia);
            }
            if (!list_nombre_completo.Contains(completo) & completo != "")
            {
                list_nombre_completo.Add(completo);
            }
            if (!list_ids.Contains(ids) & ids != "")
            {
                list_ids.Add(ids);
            }
            if (!list_links.Contains(links) & links != "")
            {
                list_links.Add(links);
            }

            Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>> tupla =
             new Tuple<List<string>, string, List<string>, List<string>, List<string>, List<string>, List<string>>(
                list_antiguos_id, orcid_unificado, list_name, list_familia, list_nombre_completo, list_ids, list_links);
            this.usuarios_unicos[id_2] = tupla;
            return true;

        }



        // una vez las personas se han unificado simplemente las metemos en el json donde corresponda. 
        public Person obtener_persona_unidicada(string id)
        {
            // buscar el id del nuevo diccionario, en el que las personas no estan duplicadas. 
            //string id_persona = "0";
            List<string> list_id_nuevos = this.usuarios_unicos.Keys.ToList();
            foreach (string id_nuevo in list_id_nuevos)
            {
                foreach (string id_viejo in this.usuarios_unicos[id_nuevo].Item1)
                {
                    if (id_viejo == id)
                    {
                        Person person = new Person();

                        string orcid = this.usuarios_unicos[id_nuevo].Item2;
                        person.ORCID = orcid;

                        Name name_author = new Name();

                        List<string> name = this.usuarios_unicos[id_nuevo].Item3;
                        name_author.given = name;

                        List<string> familia = this.usuarios_unicos[id_nuevo].Item4;
                        name_author.familia = familia;

                        List<string> completo = this.usuarios_unicos[id_nuevo].Item5;
                        name_author.nombre_completo = completo;

                        person.name = name_author;

                        List<string> ids = this.usuarios_unicos[id_nuevo].Item6;
                        person.IDs = ids;

                        List<string> links = this.usuarios_unicos[id_nuevo].Item7;
                        person.links = links;

                        return person;
                    }


                }
            }
            return null;


        }

        // //cambiamos a los objetovos de una persona  -- id
        // List<string> list_id = new List<string>();
        // string[] ids_divididos = ids.Split('*');
        // foreach (var identificador in ids_divididos)
        // {
        //     list_id.Add(identificador.ToString());
        // }
        // person.IDs = list_id;
        // //cambiamos a los objetovos de una persona  -- links
        // List<string> list_links = new List<string>();
        // string[] links_divididos = links.Split('*');
        // foreach (var link in links_divididos)
        // {
        //     list_links.Add(link.ToString());
        // }
        // person.links = list_links;

        // //Damos la estructura apropiada con los datos obtenidos  -- orcid
        // person.ORCID = orcid;
        // //Damos la estructura apropiada con los datos obtenidos  -- Name 

        // Name name_author = new Name();
        // //given 
        // List<string> name_given = new List<string>();
        // string[] name_dividivido = name.Split('*');
        // foreach (var name_dividido_concreto in name_dividivido)
        // {
        //     name_given.Add(name_dividido_concreto.ToString());
        // }
        // name_author.given = name_given;
        // //apellico
        // List<string> name_familia = new List<string>();
        // string[] familia_dividivido = familia.Split('*');
        // foreach (var familia_dividido_concreto in name_dividivido)
        // {
        //     name_familia.Add(familia_dividido_concreto.ToString());
        // }
        // name_author.familia = name_familia;
        // //nombre_completo 
        // List<string> name_completo_familia = new List<string>();
        // string[] completo_dividivido = familia.Split('*');
        // foreach (var completo_dividido_concreto in completo_dividivido)
        // {
        //     name_completo_familia.Add(completo_dividido_concreto.ToString());
        // }
        // name_author.nombre_completo = name_completo_familia;

        // person.name = name_author;
        // person.id_persona = id;
        // return person;






        public List<Publication> poner_usuarios(List<Publication> publicaciones)
        {
            foreach (Publication pub_princiapl in publicaciones)
            {
                string id;
                Person corresponding_author;
                if (pub_princiapl.correspondingAuthor != null)
                {
                    //se pone el autor correspondiendo de la publicacion con todos los datos completos
                    id = pub_princiapl.correspondingAuthor.id_persona;
                    corresponding_author = obtener_persona_unidicada(id);
                    pub_princiapl.correspondingAuthor = corresponding_author;
                }
                //se pone los autores correspondientes de la publicacion con todos los datos completos
                List<Person> contribuidores = new List<Person>();
                if (pub_princiapl.seqOfAuthors != null)
                {
                    foreach (Person contribuidor in pub_princiapl.seqOfAuthors)
                    {
                        id = contribuidor.id_persona;
                        contribuidores.Add(obtener_persona_unidicada(id));
                    }
                    pub_princiapl.seqOfAuthors = contribuidores;
                }
                if (pub_princiapl.bibliografia != null)
                {
                    // Por cada articulo de la bibliografia hacemos lo mismo...
                    foreach (Publication pub_bibliigrafia in pub_princiapl.bibliografia)
                    {
                        if (pub_bibliigrafia.correspondingAuthor != null)
                        {
                            //se pone el autor correspondiendo de la publicacion con todos los datos completos
                            id = pub_bibliigrafia.correspondingAuthor.id_persona;
                            pub_bibliigrafia.correspondingAuthor = obtener_persona_unidicada(id);
                        }
                        //se pone los autores correspondientes de la publicacion con todos los datos completos
                        contribuidores = new List<Person>();
                        if (pub_bibliigrafia.seqOfAuthors != null)
                        {
                            foreach (Person contribuidor in pub_bibliigrafia.seqOfAuthors)
                            {
                                id = contribuidor.id_persona;
                                contribuidores.Add(obtener_persona_unidicada(id));
                            }
                            pub_princiapl.seqOfAuthors = contribuidores;
                        }
                    }
                }
                if (pub_princiapl.citas != null)
                {
                    // Por cada articulo de la bibliografia hacemos lo mismo...
                    foreach (Publication pub_bibliigrafia in pub_princiapl.citas)
                    {
                        if (pub_bibliigrafia.correspondingAuthor != null)
                        {
                            //se pone el autor correspondiendo de la publicacion con todos los datos completos
                            id = pub_bibliigrafia.correspondingAuthor.id_persona;
                            pub_bibliigrafia.correspondingAuthor = obtener_persona_unidicada(id);
                        }
                        //se pone los autores correspondientes de la publicacion con todos los datos completos
                        contribuidores = new List<Person>();
                        if (pub_bibliigrafia.seqOfAuthors != null)
                        {
                            foreach (Person contribuidor in pub_bibliigrafia.seqOfAuthors)
                            {
                                id = contribuidor.id_persona;
                                contribuidores.Add(obtener_persona_unidicada(id));
                            }
                            pub_princiapl.seqOfAuthors = contribuidores;
                        }
                    }
                }

            }
            return publicaciones;
        }


    }

}