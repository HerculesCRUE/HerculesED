using Gnoss.ApiWrapper;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using ImportadorWebCV;
using ImportadorWebCV.Sincro;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Hercules.ED.ImportExportCV
{
    public class AccionesImportacion : SincroDatos
    {
        private static readonly ResourceApi mResourceApi = new($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");
        readonly ConfigService mConfiguracion;

        public AccionesImportacion(ConfigService Configuracion, string cvID, string fileData) : base(Configuracion, cvID, fileData)
        {
            this.mConfiguracion = Configuracion;
        }

        private readonly List<string> listadoSecciones = new()
        {
            //Datos identificacion
            "000.010.000.000",
            "000.020.000.000",
            //Situación profesional
            "010.010.000.000",
            "010.020.000.000",
            //Formacion academica
            "020.010.010.000",
            "020.010.020.000",
            "020.010.030.000",
            "020.020.000.000",
            "020.050.000.000",
            "020.060.000.000",
            //Actividad docente
            "030.040.000.000",
            "030.010.000.000",
            "030.050.000.000",
            "030.060.000.000",
            "030.070.000.000",
            "030.080.000.000",
            "030.090.000.000",
            "060.030.080.000",
            "030.100.000.000",
            "030.110.000.000",
            //Experiencia cientifica tecnologica
            "050.020.010.000",
            "050.020.020.000",
            "050.030.010.000",
            "050.010.000.000",
            "050.020.030.000",
            "050.030.020.000",
            //Actividad cientifica tecnologica
            "060.010.000.000",
            "060.010.060.000",
            "060.010.060.010",
            "060.010.010.000",
            "060.010.020.000",
            "060.010.030.000",
            "060.010.040.000",
            "060.020.010.000",
            "060.020.030.000",
            "060.020.040.000",
            "060.020.050.000",
            "060.020.060.000",
            "060.010.050.000",
            "060.030.010.000",
            "060.020.020.000",
            "060.030.020.000",
            "060.030.030.000",
            "060.030.040.000",
            "060.030.050.000",
            "060.030.060.000",
            "060.030.070.000",
            "060.030.090.000",
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
        public void ImportacionTriples(string pCVID, string filePreimport, List<string> listaId, List<string> listaOpciones, PetitionStatus petitionStatus)
        {
            Dictionary<string, Dictionary<string, string>> dicOpciones = new();
            List<Tuple<string, string>> filtrador = new();
            foreach (string str in listaId)
            {
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                filtrador.Add(new Tuple<string, string>(str.Split("_").First(), str.Split("_").Last()));
            }
            Preimport preimport;

            XmlSerializer serializer = new(typeof(Preimport));
            using (TextReader reader = new StringReader(filePreimport))
            {
                preimport = (Preimport)serializer.Deserialize(reader);
            }

            //Fichero preimport
            List<SubseccionItem> listadoSubsetionItems = new();
            foreach (Subseccion subseccion in preimport.secciones)
            {
                foreach (SubseccionItem subseccionItem in subseccion.subsecciones)
                {
                    listadoSubsetionItems.Add(subseccionItem);
                }
            }

            string idOpcion;
            string valueOpcion;
            string guidOpcion;
            if (listaOpciones != null && listaOpciones.Count > 0)
            {
                foreach (string opcion in listaOpciones)
                {
                    if (opcion == null)
                    {
                        continue;
                    }
                    guidOpcion = opcion.Split("|||").First().Split("_").First();
                    idOpcion = opcion.Split("|||").First().Split("_").Last();
                    valueOpcion = opcion.Split("|||").Last();
                    Dictionary<string, string> keyValues = new();
                    keyValues.Add(idOpcion, valueOpcion);
                    if (dicOpciones.ContainsKey(guidOpcion))
                    {
                        dicOpciones[guidOpcion].Add(idOpcion, valueOpcion);
                    }
                    else
                    {
                        dicOpciones.Add(guidOpcion, keyValues);
                    }
                }
            }

            //El primer item debe ser la sección "000.020.000.000"
            List<CvnItemBean> listadoItems = base.cvn.cvnRootBean.ToList();
            if (listadoItems.Count == 0)
            {
                return;
            }
            //Elimino la sección de información del fichero.
            if (listadoItems.ElementAt(0).Code.Equals("000.020.000.000"))
            {
                listadoItems.RemoveAt(0);
            }

            List<IGrouping<string, CvnItemBean>> listadoItemsAgrupados = listadoItems.GroupBy(x => x.Code).ToList();
            petitionStatus.totalWorks = listadoItems.Count;

            //Elimino las secciones no deseadas
            foreach (var item in listadoItemsAgrupados)
            {
                if (!listadoSecciones.Contains(item.Key))
                {
                    listadoItemsAgrupados.Remove(item);
                }
            }

            //En el caso de que la seccion de Texto libre no esté en listadoSubsectionItems tambien la elimino.
            if (!listadoSubsetionItems.Any(x => x.propiedades.Any(x => x.prop.Contains("http://w3id.org/roh/summary"))) && listadoItemsAgrupados.Last().Key.Equals("070.010.000.000"))
            {
                listadoItemsAgrupados.RemoveAt(listadoItemsAgrupados.Count - 1);
            }

            string opcionSeleccionada = "";

            List<CvnItemBean> listadoDuplicar = new();
            List<CvnItemBean> listadoFusionar = new();
            List<CvnItemBean> listadoSobrescribir = new();
            List<string> listadoDuplicarBBDD = new();
            List<string> listadoFusionarBBDD = new();
            List<string> listadoSobrescribirBBDD = new();
            List<string> listadoTextoLibreBBDD = new();

            foreach (IGrouping<string, CvnItemBean> seccionAgrupada in listadoItemsAgrupados)
            {
                //Seccion datos personales
                if (seccionAgrupada.Key.Equals("000.010.000.000"))
                {
                    SobrescribirDatosPersonales(preimport, listadoSobrescribir, listadoSobrescribirBBDD, filtrador, seccionAgrupada);

                    //Actualizo el estado de los recursos tratados
                    petitionStatus.actualWork++;
                    continue;
                }
                //Seccion Indicadores generales
                if (seccionAgrupada.Key.Equals("060.010.060.000"))
                {
                    SobrescribirIndicadoresGenerales(preimport, listadoSobrescribir, listadoSobrescribirBBDD, filtrador, seccionAgrupada);

                    //Actualizo el estado de los recursos tratados
                    petitionStatus.actualWork++;
                    continue;
                }
                //Seccion texto libre
                if (seccionAgrupada.Key.Equals("070.010.000.000"))
                {
                    SobrescribirTextoLibre(preimport, listadoSobrescribir, listadoTextoLibreBBDD, filtrador, seccionAgrupada);

                    //Actualizo el estado de los recursos tratados
                    petitionStatus.actualWork++;
                    continue;
                }
                //Si el identificador no está contenido en preimport.secciones paso a la siguiente
                if (!preimport.secciones.Any(x => x.id.Equals(seccionAgrupada.Key)))
                {
                    //Actualizo el estado de los recursos tratados
                    petitionStatus.actualWork++;
                    continue;
                }

                List<SubseccionItem> listaAux = preimport.secciones.Where(x => x.id.Equals(seccionAgrupada.Key)).Select(x => x.subsecciones).FirstOrDefault().ToList();
                List<CvnItemBean> listaItemsAux = listadoItemsAgrupados.First(x => x.Key.Equals(seccionAgrupada.Key)).Select(x => x).ToList();
                foreach (SubseccionItem subseccionItem in listaAux)
                {
                    if (!filtrador.Select(x => x.Item1).Contains(subseccionItem.guid))
                    {
                        //Actualizo el estado de los recursos tratados
                        petitionStatus.actualWork++;
                        continue;
                    }
                    string ordenOpcion = filtrador.Where(x => x.Item1.Equals(subseccionItem.guid)).Select(x => x.Item2).FirstOrDefault();

                    //La opción por defecto será la de duplicar el objeto, en caso de que no traiga nada
                    // para que si no hay opción seleccionada carge un recurso nuevo.
                    opcionSeleccionada = "du";
                    if (dicOpciones.ContainsKey(subseccionItem.guid))
                    {
                        opcionSeleccionada = dicOpciones[subseccionItem.guid].Values.First();
                    }
                    //Duplicar
                    if (opcionSeleccionada.Equals("du"))
                    {
                        listadoDuplicar.Add(listaItemsAux[int.Parse(ordenOpcion)]);
                        listadoDuplicarBBDD.Add(subseccionItem.idBBDD + "@@@du");
                    }
                    //Fusion
                    if (opcionSeleccionada.Equals("fu"))
                    {
                        listadoFusionar.Add(listaItemsAux[int.Parse(ordenOpcion)]);
                        listadoFusionarBBDD.Add(subseccionItem.idBBDD + "@@@fu");
                    }
                    //Sobrescribir
                    if (opcionSeleccionada.Equals("so"))
                    {
                        listadoSobrescribir.Add(listaItemsAux[int.Parse(ordenOpcion)]);
                        listadoSobrescribirBBDD.Add(subseccionItem.idBBDD + "@@@so");
                    }

                    //Actualizo el estado de los recursos tratados
                    petitionStatus.actualWork++;
                }
            }

            //Listado de Identificadores DOI
            List<string> listaDOI = new();

            //Asigno los cvnRoot dependiendo de cada tipo de acción.
            cvnRootResultBean duplicadosResultBean = new() { cvnRootBean = listadoDuplicar.ToArray() };
            cvnRootResultBean fusionResultBean = new() { cvnRootBean = listadoFusionar.ToArray() };
            cvnRootResultBean sobrescribirResultBean = new() { cvnRootBean = listadoSobrescribir.ToArray() };


            //Duplicar
            Duplicar(duplicadosResultBean, listadoDuplicar, listadoDuplicarBBDD, listaDOI, petitionStatus);

            //Fusionar
            Fusionar(fusionResultBean, listadoFusionar, listadoFusionarBBDD, listaDOI, petitionStatus);

            //Sobrescribir
            Sobrescribir(sobrescribirResultBean, listadoSobrescribir, listadoSobrescribirBBDD, listadoTextoLibreBBDD, listaDOI, petitionStatus);

            //Despues de duplicar, fusionar y sobrescribir los ítems, llamo al servicio de FE para buscar aquellos ítems duplicados con DOI.
            string personId = Utils.Utility.PersonaCV(pCVID);
            string nombreCompletoPersona = Utils.Utility.GetNombreCompletoPersonaCV(pCVID);

            //Elimino los DOI que se encuentren en BBDD.
            listaDOI = Utils.UtilitySecciones.ComprobarDOIenBBDD(listaDOI);

            foreach (string doi in listaDOI)
            {
                try
                {
                    Utils.UtilitySecciones.EnvioFuentesExternasDOI(mConfiguracion, doi, personId, nombreCompletoPersona);
                }
                catch (Exception ex)
                {
                    mResourceApi.Log.Error(ex.Message);
                }
            }

            try
            {
                string person = Utils.Utility.GetPersonFromCV(pCVID);
                Utils.Utility.EnvioNotificacion(person, "loadCV", "NOTIFICACION_FIN_CARGA_CV");
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
            }

        }

        /// <summary>
        /// Añade la seccion de datos personales para sobrescribirla
        /// </summary>
        /// <param name="preimport">Preimport</param>
        /// <param name="listadoSobrescribir">Listado de items a </param>
        /// <param name="listadoSobrescribirBBDD">Listado de ítems en BBDD</param>
        /// <param name="filtrador">Filtrador</param>
        /// <param name="seccionAgrupada">Seccion</param>
        private static void SobrescribirDatosPersonales(Preimport preimport, List<CvnItemBean> listadoSobrescribir, List<string> listadoSobrescribirBBDD, List<Tuple<string, string>> filtrador, IGrouping<string, CvnItemBean> seccionAgrupada)
        {
            List<SubseccionItem> listaIndicadoresAux = preimport.secciones.Where(x => x.id.Equals("000.000.000.000")).Select(x => x.subsecciones).FirstOrDefault().ToList();
            if (!listadoSobrescribir.Exists(x => x.Code.Equals("000.010.000.000")))
            {
                listadoSobrescribir.Add(seccionAgrupada.Select(x => x).First());
            }
            if (filtrador.Select(x => x.Item1).Contains(listaIndicadoresAux.First().guid))
            {
                listadoSobrescribirBBDD.Add(listaIndicadoresAux.First().idBBDD + "@@@so");
            }

        }

        /// <summary>
        /// Añade la seccion de indicadores personales para sobrescribirla
        /// </summary>
        /// <param name="preimport">Preimport</param>
        /// <param name="listadoSobrescribir">Listado de items a </param>
        /// <param name="listadoSobrescribirBBDD">Listado de ítems en BBDD</param>
        /// <param name="filtrador">Filtrador</param>
        /// <param name="seccionAgrupada">Seccion</param>
        private static void SobrescribirIndicadoresGenerales(Preimport preimport, List<CvnItemBean> listadoSobrescribir, List<string> listadoSobrescribirBBDD, List<Tuple<string, string>> filtrador, IGrouping<string, CvnItemBean> seccionAgrupada)
        {
            List<SubseccionItem> listaIndicadoresAux = preimport.secciones.Where(x => x.id.Equals("060.010.060.010")).Select(x => x.subsecciones).FirstOrDefault().ToList();

            if (!listadoSobrescribir.Exists(x => x.Code.Equals("060.010.060.000")))
            {
                listadoSobrescribir.Add(seccionAgrupada.Select(x => x).First());
            }
            if (filtrador.Select(x => x.Item1).Contains(listaIndicadoresAux.First().guid))
            {
                listadoSobrescribirBBDD.Add(listaIndicadoresAux.First().idBBDD + "@@@so");
            }
        }

        /// <summary>
        /// Añade la seccion de texto libre para sobrescribirla
        /// </summary>
        /// <param name="preimport">Preimport</param>
        /// <param name="listadoSobrescribir">Listado de items a </param>
        /// <param name="listadoTextoLibreBBDD">Listado de ítems en BBDD</param>
        /// <param name="filtrador">Filtrador</param>
        /// <param name="seccionAgrupada">Seccion</param>
        private static void SobrescribirTextoLibre(Preimport preimport, List<CvnItemBean> listadoSobrescribir, List<string> listadoTextoLibreBBDD, List<Tuple<string, string>> filtrador, IGrouping<string, CvnItemBean> seccionAgrupada)
        {
            List<SubseccionItem> listaIndicadoresAux = preimport.secciones.Where(x => x.id.Equals("070.010.000.000")).Select(x => x.subsecciones).FirstOrDefault().ToList();

            //Recorro resumenLibre(0), resumenTFG(1) y resumenTFM(2) para comprobar si alguno de ellos está marcado,
            // e indicando cual de ellos para posteriormente cargar ese dato unicamente.
            for (int contadorTexto = 0; contadorTexto < 3; contadorTexto++)
            {
                if (filtrador.Select(x => x.Item1).Contains(listaIndicadoresAux.First().guid) && filtrador.Select(x => x.Item2).Contains(contadorTexto.ToString()))
                {
                    //Si no existe el CvnItemBean de la sección "070.010.000.000" lo añado, en otro caso sigo.
                    if (!listadoSobrescribir.Exists(x => x.Code.Equals("070.010.000.000")))
                    {
                        listadoSobrescribir.Add(seccionAgrupada.Select(x => x).First());
                    }
                    listadoTextoLibreBBDD.Add(listaIndicadoresAux.First().idBBDD + "@@@" + contadorTexto);
                }
            }
        }

        /// <summary>
        /// Duplicar
        /// </summary>
        /// <param name="duplicadosResultBean">cvnRootResultBean con los datos a duplicar</param>
        /// <param name="listadoDuplicar">Listado de items a duplicar</param>
        /// <param name="listadoDuplicarBBDD">Listado de ítems en BBDD</param>
        /// <param name="petitionStatus">PetitionStatus</param>
        private void Duplicar(cvnRootResultBean duplicadosResultBean, List<CvnItemBean> listadoDuplicar, List<string> listadoDuplicarBBDD, List<string> listaDOI, PetitionStatus petitionStatus)
        {
            base.cvn = duplicadosResultBean;

            petitionStatus.actualWorkTitle = "ESTADO_POSTIMPORTAR_DUPLICAR";
            petitionStatus.actualWorkSubtitle = "";
            petitionStatus.totalWorks = listadoDuplicar.Count;
            petitionStatus.actualWork = 0;

            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD, petitionStatus: petitionStatus);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD, petitionStatus: petitionStatus);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD, petitionStatus: petitionStatus);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD, petitionStatus: petitionStatus);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD, petitionStatus: petitionStatus, listaDOI: listaDOI);
        }

        /// <summary>
        /// Fusionar
        /// </summary>
        /// <param name="fusionResultBean">cvnRootResultBean con los datos a fusionar</param>
        /// <param name="listadoFusionar">Listado de items a fusionar</param>
        /// <param name="listadoFusionarBBDD">Listado de ítems en BBDD</param>
        /// <param name="petitionStatus">PetitionStatus</param>
        private void Fusionar(cvnRootResultBean fusionResultBean, List<CvnItemBean> listadoFusionar, List<string> listadoFusionarBBDD, List<string> listaDOI, PetitionStatus petitionStatus)
        {
            base.cvn = fusionResultBean;

            petitionStatus.actualWorkTitle = "ESTADO_POSTIMPORTAR_FUSIONAR";
            petitionStatus.totalWorks = listadoFusionar.Count;
            petitionStatus.actualWork = 0;

            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoFusionarBBDD, petitionStatus: petitionStatus);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD, petitionStatus: petitionStatus);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoFusionarBBDD, petitionStatus: petitionStatus);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD, petitionStatus: petitionStatus);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD, petitionStatus: petitionStatus, listaDOI: listaDOI);

        }

        /// <summary>
        /// Sobrescribir
        /// </summary>
        /// <param name="sobrescribirResultBean">cvnRootResultBean con los datos a duplicar</param>
        /// <param name="listadoSobrescribir">Listado de items a sobrescribir</param>
        /// <param name="listadoSobrescribirBBDD">Listado de ítems en BBDD</param>
        /// <param name="listadoTextoLibreBBDD">Listado de ítems en BBDD</param>
        /// <param name="listaDOI">Listado de DOI</param>
        /// <param name="petitionStatus">PetitionStatus</param>
        private void Sobrescribir(cvnRootResultBean sobrescribirResultBean, List<CvnItemBean> listadoSobrescribir, List<string> listadoSobrescribirBBDD, List<string> listadoTextoLibreBBDD, List<string> listaDOI, PetitionStatus petitionStatus)
        {
            base.cvn = sobrescribirResultBean;

            petitionStatus.actualWorkTitle = "ESTADO_POSTIMPORTAR_SOBRESCRIBIR";
            petitionStatus.totalWorks = listadoSobrescribir.Count;
            petitionStatus.actualWork = 0;

            base.SincroDatosIdentificacion(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus);
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD, petitionStatus: petitionStatus, listaDOI: listaDOI);
            base.SincroTextoLibre(preimportar: false, listadoIdBBDD: listadoTextoLibreBBDD);

        }


    }
}

