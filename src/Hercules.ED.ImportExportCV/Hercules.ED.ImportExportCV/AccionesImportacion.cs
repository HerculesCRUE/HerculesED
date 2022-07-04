using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using ImportadorWebCV;
using ImportadorWebCV.Sincro;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hercules.ED.ImportExportCV
{
    public class AccionesImportacion : SincroDatos
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public AccionesImportacion(ConfigService Configuracion, string cvID, string fileData) : base(Configuracion, cvID, fileData)
        {

        }

        private List<string> listadoSecciones = new List<string>()
        {
            //Datos identificacion
            "000.010.000.000", "000.020.000.000",
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
        /// Importa los datos de <paramref name="filePreimport"/>, filtrando los items y eliminando los que no sean pertenecientes a <paramref name="listaId"/>
        /// y tratandolos dependiendo de la opciones marcadas en <paramref name="listaOpciones"/>.
        /// </summary>
        /// <param name="pCVID"></param>
        /// <param name="filePreimport"></param>
        /// <param name="listaId"></param>
        /// <param name="listaOpciones"></param>
        public void ImportacionTriples(string pCVID, string filePreimport, List<string> listaId, List<string> listaOpciones)
        {
            Dictionary<string, string> dicOpciones = new Dictionary<string, string>();
            List<Tuple<string, string>> filtrador = new List<Tuple<string, string>>();
            foreach (string str in listaId)
            {
                filtrador.Add(new Tuple<string, string>(str.Split("_").First(), str.Split("_").Last()));
            }
            Preimport preimport = new Preimport();

            XmlSerializer serializer = new XmlSerializer(typeof(Preimport));
            using (TextReader reader = new StringReader(filePreimport))
            {
                preimport = (Preimport)serializer.Deserialize(reader);
            }

            //Fichero preimport
            List<SubseccionItem> listadoSubsetionItems = new List<SubseccionItem>();
            foreach (Subseccion subseccion in preimport.secciones)
            {
                foreach (SubseccionItem subseccionItem in subseccion.subsecciones)
                {
                    listadoSubsetionItems.Add(subseccionItem);
                }
            }

            string idOpcion;
            string valueOpcion;
            if (listaOpciones != null && listaOpciones.Count > 0)
            {
                foreach (string opcion in listaOpciones)
                {
                    if (opcion == null)
                    {
                        continue;
                    }
                    idOpcion = opcion.Split("|||").First().Split("_").Last();
                    valueOpcion = opcion.Split("|||").Last();
                    dicOpciones.Add(idOpcion, valueOpcion);
                }
            }

            //El primer item debe ser la sección "000.020.000.000"
            List<CvnItemBean> listadoItems = base.cvn.cvnRootBean.ToList();
            if (listadoItems.Count == 0 || !listadoItems.ElementAt(0).Code.Equals("000.020.000.000"))
            {
                return;
            }
            //Elimino la sección de información del fichero.
            if (listadoItems.ElementAt(0).Code.Equals("000.020.000.000"))
            {
                listadoItems.RemoveAt(0);
            }

            //Elimino las secciones no deseadas
            foreach (CvnItemBean itemBean in new List<CvnItemBean>(listadoItems))
            {
                if (!listadoSecciones.Contains(itemBean.Code))
                {
                    listadoItems.Remove(itemBean);
                }
            }

            //En el caso de que la seccion de Texto libre no esté en listadoSubsectionItems tambien la elimino.
            if (!listadoSubsetionItems.Any(x => x.propiedades.Any(x => x.prop.Contains("http://w3id.org/roh/summary"))))
            {
                if (listadoItems.Last().Code.Equals("070.010.000.000"))
                {
                    listadoItems.RemoveAt(listadoItems.Count - 1);
                }
            }


            int contadorEliminados = 1;
            string opcionSeleccionada = "";

            List<CvnItemBean> listadoDuplicar = new List<CvnItemBean>();
            List<CvnItemBean> listadoFusionar = new List<CvnItemBean>();
            List<CvnItemBean> listadoSobrescribir = new List<CvnItemBean>();
            List<string> listadoDuplicarBBDD = new List<string>();
            List<string> listadoFusionarBBDD = new List<string>();
            List<string> listadoSobrescribirBBDD = new List<string>();
            List<string> listadoTextoLibreBBDD = new List<string>();


            for (int i = 0; i < listadoItems.Count; i++)
            {
                //Si el elemento no trae propiedades, no lo inserto y continuo con el siguiente del listado.
                if (listadoSubsetionItems.Count == i)
                {
                    break;
                }
                if (listadoSubsetionItems.ElementAt(i).propiedades.Count == 0)
                {
                    if (listadoItems.ElementAt(i).Items.Count == 0)
                    {
                        listadoItems.RemoveAt(i);
                    }
                    listadoSubsetionItems.RemoveAt(i);
                    i--;
                    continue;
                }
                //Si alguno de los datos no está marcado continuo con el siguiente del listado.
                if (!filtrador.Any(x => x.Item2.Equals((i + contadorEliminados).ToString())))
                {
                    continue;
                }

                //Compruebo si es la sección de Indicadores Generales de producción cientifica.
                if (listadoItems.ElementAt(i).Code.Equals("060.010.060.000"))
                {
                    if (!listadoSobrescribir.Exists(x => x.Code.Equals("060.010.060.000")))
                    {
                        listadoSobrescribir.Add(listadoItems.ElementAt(i));
                    }
                    if (listadoSubsetionItems.ElementAt(i).idBBDD.StartsWith("http://gnoss.com/items/GeneralQualityIndicatorCV_"))
                    {
                        listadoSobrescribirBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@so");
                    }
                    continue;
                }

                //El texto libre siempre debe llegar en la última posición
                if (i.Equals(listadoItems.Count() - 1))
                {
                    //Recorro resumenLibre(0), resumenTFG(1) y resumenTFM(2) para comprobar si alguno de ellos está marcado,
                    // e indicando cual de ellos para posteriormente cargar ese dato unicamente.
                    for (int contadorTexto = 0; contadorTexto < 3; contadorTexto++)
                    {
                        if (!listadoItems.Last().Code.Equals("070.010.000.000"))
                        {
                            break;
                        }
                        if (filtrador.Any(x => x.Item2.Equals((i + contadorEliminados + contadorTexto).ToString())))
                        {
                            //Si no existe el CvnItemBean de la sección "070.010.000.000" lo añado, en otro caso sigo.
                            if (!listadoSobrescribir.Exists(x => x.Code.Equals("070.010.000.000")))
                            {
                                listadoSobrescribir.Add(listadoItems.Last());
                            }

                            if (listadoSubsetionItems.Last().idBBDD != null && listadoSubsetionItems.Last().idBBDD.StartsWith("http://gnoss.com/items/FreeTextSummaryValuesCV_"))
                            {
                                listadoTextoLibreBBDD.Add(listadoSubsetionItems.Last().idBBDD + "@@@" + contadorTexto);
                            }
                        }
                    }
                }

                //La opción por defecto será la de duplicar el objeto, en caso de que no traiga nada
                // para que si no hay opción seleccionada carge un recurso nuevo.
                opcionSeleccionada = "du";
                if (dicOpciones.ContainsKey((i + contadorEliminados).ToString()))
                {
                    opcionSeleccionada = dicOpciones[(i + contadorEliminados).ToString()];
                }
                if (opcionSeleccionada.Equals("du"))
                {
                    listadoDuplicar.Add(listadoItems.ElementAt(i));
                    listadoDuplicarBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@du");
                }
                if (opcionSeleccionada.Equals("fu"))
                {
                    listadoFusionar.Add(listadoItems.ElementAt(i));
                    listadoFusionarBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@fu");
                }
                if (opcionSeleccionada.Equals("so"))
                {
                    listadoSobrescribir.Add(listadoItems.ElementAt(i));
                    listadoSobrescribirBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@so");
                }
            }

            //Asigno los cvnRoot dependiendo de cada tipo de acción.
            cvnRootResultBean duplicadosResultBean = new cvnRootResultBean() { cvnRootBean = listadoDuplicar.ToArray() };
            cvnRootResultBean fusionResultBean = new cvnRootResultBean() { cvnRootBean = listadoFusionar.ToArray() };
            cvnRootResultBean sobrescribirResultBean = new cvnRootResultBean() { cvnRootBean = listadoSobrescribir.ToArray() };


            //Duplicar
            base.cvn = duplicadosResultBean;
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);

            //Fusionar
            base.cvn = fusionResultBean;
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);

            //Sobrescribir
            base.cvn = sobrescribirResultBean;
            base.SincroDatosIdentificacion(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroTextoLibre(preimportar: false, listadoIdBBDD: listadoTextoLibreBBDD);
        }
    }
}

