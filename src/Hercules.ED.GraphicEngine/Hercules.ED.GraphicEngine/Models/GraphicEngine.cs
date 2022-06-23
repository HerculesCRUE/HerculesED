using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.GraphicEngine.Models.Graficas;
using Hercules.ED.GraphicEngine.Models.Paginas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Web;
using System.Drawing;
using Gnoss.ApiWrapper.Model;

namespace Hercules.ED.GraphicEngine.Models
{
    public static class GraphicEngine
    {
        // Prefijos.
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configJson/prefijos.json")));
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static Guid mCommunityID = mCommunityApi.GetCommunityId();
        private static List<ConfigModel> mTabTemplates;
        private const int NUM_HILOS = 5;

        #region --- Páginas
        /// <summary>
        /// Obtiene los datos de la página.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Pagina GetPage(string pIdPagina, string pLang)
        {
            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);
            return CrearPagina(configModel, pLang);
        }
        /// <summary>
        /// Obtiene los datos de las páginas.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static List<Pagina> GetPages(string pLang)
        {
            List<Pagina> listaPaginas = new List<Pagina>();

            // Lectura de los JSON de configuración.
            List<ConfigModel> listaConfigModels = TabTemplates;
            foreach (ConfigModel configModel in listaConfigModels)
            {
                listaPaginas.Add(CrearPagina(configModel, pLang));
            }

            return listaPaginas;
        }

        /// <summary>
        /// Crea el objeto página.
        /// </summary>
        /// <param name="pConfigModel">Configuración.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Pagina CrearPagina(ConfigModel pConfigModel, string pLang)
        {
            Pagina pagina = new Pagina();
            pagina.id = pConfigModel.identificador;
            pagina.nombre = GetTextLang(pLang, pConfigModel.nombre);
            pagina.listaConfigGraficas = new List<ConfigPagina>();
            foreach (Grafica itemGrafica in pConfigModel.graficas)
            {
                ConfigPagina configPagina = new ConfigPagina()
                {
                    id = itemGrafica.identificador,
                    anchura = itemGrafica.anchura,
                    idGrupo = itemGrafica.idGrupo
                };

                string prefijoNodos = "nodes";
                string prefijoBarraHorizonal = "isHorizontal";
                string prefijoCircular = "circular";
                string prefijoAbreviar = "abr";
                string prefijoPorcentaje = "prc";

                configPagina.isCircular = itemGrafica.tipo == EnumGraficas.Circular;
                configPagina.isAbr = itemGrafica.config.abreviar;
                configPagina.isNodes = itemGrafica.tipo == EnumGraficas.Nodos;
                configPagina.isHorizontal = !(itemGrafica.tipo == EnumGraficas.Circular || itemGrafica.config.orientacionVertical);
                configPagina.isCircular = itemGrafica.tipo == EnumGraficas.Circular;
                configPagina.isPercentage = itemGrafica.config.porcentual;

                if (itemGrafica.config.abreviar && !itemGrafica.identificador.Contains(prefijoAbreviar))
                {
                    itemGrafica.identificador = prefijoAbreviar + "-" + itemGrafica.identificador;
                    configPagina.id = prefijoAbreviar + "-" + configPagina.id;

                }

                if (itemGrafica.tipo == EnumGraficas.Nodos && !itemGrafica.identificador.Contains(prefijoNodos))
                {
                    itemGrafica.identificador = prefijoNodos + "-" + itemGrafica.identificador;
                    configPagina.id = prefijoNodos + "-" + configPagina.id;
                }
                else if (!(itemGrafica.tipo == EnumGraficas.Circular || itemGrafica.config.orientacionVertical) && !itemGrafica.identificador.Contains(prefijoBarraHorizonal) && !itemGrafica.identificador.Contains(prefijoNodos))
                {
                    itemGrafica.identificador = prefijoBarraHorizonal + "-" + itemGrafica.identificador;
                    configPagina.id = prefijoBarraHorizonal + "-" + configPagina.id;

                }
                else if (itemGrafica.tipo == EnumGraficas.Circular && !itemGrafica.identificador.Contains(prefijoCircular))
                {
                    itemGrafica.identificador = prefijoCircular + "-" + itemGrafica.identificador;
                    configPagina.id = prefijoCircular + "-" + configPagina.id;

                }
                if (itemGrafica.config.porcentual && !itemGrafica.identificador.Contains(prefijoPorcentaje))
                {
                    itemGrafica.identificador = prefijoPorcentaje + "-" + itemGrafica.identificador;
                    configPagina.id = prefijoPorcentaje + "-" + configPagina.id;

                }

                // Si la anchura no contiene un valor aceptado, se le asigna 1/2 por defecto.
                List<int> valoresAceptados = new List<int>() { 11, 12, 13, 14, 16, 23, 34, 38, 58 };
                if (!valoresAceptados.Contains(configPagina.anchura))
                {
                    configPagina.anchura = 12;
                }

                pagina.listaConfigGraficas.Add(configPagina);
            }
            pagina.listaIdsFacetas = new List<string>();
            foreach (FacetaConf itemFaceta in pConfigModel.facetas)
            {
                pagina.listaIdsFacetas.Add(itemFaceta.filtro);
            }
            return pagina;
        }
        #endregion

        #region --- CSV
        /// <summary>
        /// Obtiene los datos del CSV.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static void GetCSV(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);

            // Obtiene los filtros relacionados con las fechas.
            List<string> listaFacetasAnios = configModel.facetas.Where(x => x.rangoAnio).Select(x => x.filtro).ToList();

            if (configModel != null)
            {
                Grafica grafica = configModel.graficas.FirstOrDefault(x => x.identificador == pIdGrafica);
            }
        }
        #endregion

        #region --- Gráficas
        /// <summary>
        /// Lee la configuración y obtiene los datos necesarios para el servicio de gráficas.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pIdGrafica">Identificador de la gráfica.</param>
        /// <param name="pFiltroFacetas">Filtros de la URL.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaBase GetGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);

            // Obtiene los filtros relacionados con las fechas.
            List<string> listaFacetasAnios = configModel.facetas.Where(x => x.rangoAnio).Select(x => x.filtro).ToList();

            if (configModel != null)
            {
                Grafica grafica = configModel.graficas.FirstOrDefault(x => x.identificador.Split('-').LastOrDefault() == pIdGrafica.Split('-').LastOrDefault());
                return CrearGrafica(grafica, configModel.filtro, pFiltroFacetas, pLang, listaFacetasAnios);
            }

            return null;
        }

        /// <summary>
        /// Crea el objeto gráfica.
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static GraficaBase CrearGrafica(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates)
        {
            pFiltroFacetas = HttpUtility.UrlDecode(pFiltroFacetas);



            switch (pGrafica.tipo)
            {
                case EnumGraficas.Barras:
                    if (pGrafica.config.orientacionVertical)
                    {
                        ControlarExcepcionesBarrasX(pGrafica);
                        return CrearGraficaBarras(pGrafica, pFiltroBase, pFiltroFacetas, pLang, pListaDates, pGrafica.config.datosNodos);
                    }
                    else
                    {
                        //ControlarExcepcionesBarrasY(pGrafica);
                        return CrearGraficaBarrasY(pGrafica, pFiltroBase, pFiltroFacetas, pLang, pListaDates, pGrafica.config.datosNodos);
                    }
                case EnumGraficas.Circular:
                    ControlarExcepcionesCircular(pGrafica);
                    return CrearGraficaCircular(pGrafica, pFiltroBase, pFiltroFacetas, pLang, pListaDates);
                case EnumGraficas.Nodos:
                    return CrearGraficaNodos(pGrafica, pFiltroBase, pFiltroFacetas, pLang, pListaDates);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Crea el objeto de la gráfica (Gráfica de Barras vertical).
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaBarras CrearGraficaBarras(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates, bool pNodos)
        {
            // Objeto a devolver.
            GraficaBarras grafica = new GraficaBarras();
            grafica.type = "bar"; // Por defecto, de tipo bar.

            // Tipo.
            grafica.isHorizontal = true;

            // Abreviación.
            if (pGrafica.config.abreviar)
            {
                grafica.isAbr = pGrafica.config.abreviar;
            }

            // Porcentage.
            if (pGrafica.config.porcentual)
            {
                grafica.isPercentage = pGrafica.config.porcentual;
            }

            // ID Grupo.
            if (!string.IsNullOrEmpty(pGrafica.idGrupo))
            {
                grafica.groupId = pGrafica.idGrupo;
            }

            // Es fecha.
            if (pListaDates != null && pListaDates.Any() && pListaDates.Contains(pGrafica.config.ejeX))
            {
                grafica.isDate = true;
            }

            // Asignación de Data.
            DataBarras data = new DataBarras();
            data.datasets = new ConcurrentBag<DatasetBarras>();
            grafica.data = data;

            // Asignación de Options.
            Options options = new Options();

            // Orientación
            options.indexAxis = "x";

            options.scales = new Dictionary<string, Eje>();

            // Ejes Y
            foreach (EjeYConf item in pGrafica.config.yAxisPrint)
            {
                Eje eje = new Eje();
                eje.position = item.posicion;
                eje.title = new Title();
                eje.title.display = true;

                if (item.nombreEje != null)
                {
                    eje.title.text = GetTextLang(pLang, item.nombreEje);
                }
                else
                {
                    eje.title.text = string.Empty;
                }

                options.scales.Add(item.yAxisID, eje);
            }

            // Animación
            options.animation = new Animation();
            options.animation.duration = 2000;

            // Título
            options.plugins = new Plugin();
            options.plugins.title = new Title();
            options.plugins.title.display = true;
            options.plugins.title.text = GetTextLang(pLang, pGrafica.nombre);

            grafica.options = options;

            ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>> resultadosDimension = new ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>>();
            Dictionary<Dimension, DatasetBarras> dimensionesDataset = new Dictionary<Dimension, DatasetBarras>();

            bool ejeFechas = false;
            Parallel.ForEach(pGrafica.config.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, itemGrafica =>
            {
                // Determina si en el filtro contiene '=' para tratarlo de manera especial.
                bool filtroEspecial = false;
                if (!string.IsNullOrEmpty(itemGrafica.filtro) && !itemGrafica.filtro.Contains("="))
                {
                    filtroEspecial = true;
                }

                // Orden.               
                string orden = "DESC";
                if (pGrafica.config.orderDesc == true)
                {
                    orden = "ASC";
                }
                foreach (string item in pListaDates)
                {
                    if (item.Contains(pGrafica.config.ejeX))
                    {
                        orden = "ASC";
                        ejeFechas = true;
                        break;
                    }
                }

                // Filtro de página.
                List<string> filtros = new List<string>();

                List<Tuple<string, string, float>> listaTuplas = new List<Tuple<string, string, float>>();
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();
                if (!string.IsNullOrEmpty(pGrafica.config.reciproco))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.config.ejeX }, "ejeX", null, pGrafica.config.reciproco));
                }
                else
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.config.ejeX }, "ejeX"));
                }
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }, pListaDates: pListaDates));
                }
                if (filtroEspecial)
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }, "aux"));
                }
                else if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }
                if (pNodos)
                {
                    //Nodos            
                    Dictionary<string, string> dicNodos = new Dictionary<string, string>();

                    //Relaciones
                    Dictionary<string, List<DataQueryRelaciones>> dicRelaciones = new Dictionary<string, List<DataQueryRelaciones>>();

                    //Respuesta
                    List<DataItemRelacion> itemsRelacion = new List<DataItemRelacion>();

                    Dictionary<string, List<string>> dicResultadosAreaRelacionAreas = new Dictionary<string, List<string>>();
                    Dictionary<string, int> scoreNodes = new Dictionary<string, int>();
                    Dictionary<string, float> dicResultados = new Dictionary<string, float>();
                    // Consulta sparql.
                    select.Append(mPrefijos);
                    select.Append("SELECT ?s group_concat(?categoria;separator=\",\") AS ?idCategorias ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"?s {pGrafica.propCategoryPath} ?area. ");
                    where.Append("?area roh:categoryNode ?categoria. ");
                    where.Append("MINUS { ?categoria skos:narrower ?hijos } ");
                    where.Append("} ");

                    resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string idCategorias = fila["idCategorias"].value;
                            HashSet<string> categorias = new HashSet<string>(idCategorias.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

                            foreach (string categoria in categorias)
                            {
                                if (!scoreNodes.ContainsKey(categoria))
                                {
                                    scoreNodes.Add(categoria, 0);
                                }

                                scoreNodes[categoria]++;

                                if (!dicResultadosAreaRelacionAreas.ContainsKey(categoria))
                                {
                                    dicResultadosAreaRelacionAreas.Add(categoria, new List<string>());
                                }

                                dicResultadosAreaRelacionAreas[categoria].AddRange(categorias.Except(new List<string>() { categoria }));
                            }
                        }
                    }

                    ProcesarRelaciones("Category", dicResultadosAreaRelacionAreas, ref dicRelaciones);

                    int maximasRelaciones = 0;
                    foreach (KeyValuePair<string, List<DataQueryRelaciones>> sujeto in dicRelaciones)
                    {
                        foreach (DataQueryRelaciones relaciones in sujeto.Value)
                        {
                            foreach (Datos relaciones2 in relaciones.idRelacionados)
                            {
                                maximasRelaciones = Math.Max(maximasRelaciones, relaciones2.numVeces);
                            }
                        }
                    }

                    // Creamos los nodos y las relaciones en función de pNumAreas.
                    int pNumAreas = pGrafica.config.dimensiones.FirstOrDefault().numMaxNodos;

                    Dictionary<string, int> numRelaciones = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, List<DataQueryRelaciones>> sujeto in dicRelaciones)
                    {
                        if (!numRelaciones.ContainsKey(sujeto.Key))
                        {
                            numRelaciones.Add(sujeto.Key, 0);
                        }
                        foreach (DataQueryRelaciones relaciones in sujeto.Value)
                        {
                            foreach (Datos relaciones2 in relaciones.idRelacionados)
                            {
                                if (!numRelaciones.ContainsKey(relaciones2.idRelacionado))
                                {
                                    numRelaciones.Add(relaciones2.idRelacionado, 0);
                                }
                                numRelaciones[sujeto.Key] += relaciones2.numVeces;
                                numRelaciones[relaciones2.idRelacionado] += relaciones2.numVeces;
                            }
                        }
                    }

                    List<string> itemsSeleccionados = numRelaciones.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.Distinct().ToList();
                    if (itemsSeleccionados.Count() > pNumAreas)
                    {
                        itemsSeleccionados = itemsSeleccionados.GetRange(0, pNumAreas);
                    }

                    if (itemsSeleccionados.Count > 0)
                    {
                        // Recuperamos los nombres de categorías y creamos los nodos.
                        select = new StringBuilder();
                        where = new StringBuilder();

                        select.Append(mPrefijos);
                        select.Append("SELECT ?categoria ?nombreCategoria ");
                        where.Append("WHERE { ");
                        where.Append("?categoria skos:prefLabel ?nombreCategoria. ");
                        where.Append($@"FILTER(?categoria IN (<{string.Join(">,<", itemsSeleccionados)}>)) ");
                        where.Append("} ");

                        resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                        if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                        {
                            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                            {
                                if (!dicNodos.ContainsKey(fila["categoria"].value))
                                {
                                    dicNodos.Add(fila["categoria"].value, fila["nombreCategoria"].value);
                                }
                            }
                        }

                        // Nodos. 
                        if (dicNodos != null && dicNodos.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> nodo in dicNodos)
                            {
                                string clave = nodo.Key;
                                Data data = new Data(clave, nodo.Value, null, null, null, "nodes", Data.Type.icon_area);
                                if (scoreNodes.ContainsKey(clave))
                                {
                                    data.score = scoreNodes[clave];
                                    data.name = data.name + " (" + data.score + ")";
                                }
                                DataItemRelacion dataColabo = new DataItemRelacion(data, true, true);
                                itemsRelacion.Add(dataColabo);
                            }
                        }
                    }

                    resultadosDimension[itemGrafica] = listaTuplas;
                }
                else if (string.IsNullOrEmpty(itemGrafica.calculo))
                {
                    // Consulta sparql.                    
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    if (filtroEspecial)
                    {
                        select.Append($@"SELECT ?ejeX ?aux COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    else
                    {
                        select.Append($@"SELECT ?ejeX COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    if (filtroEspecial)
                    {
                        where.Append($@"FILTER(LANG(?aux) = 'es' OR LANG(?aux) = '' OR !isLiteral(?aux))");
                    }
                    else
                    {
                        where.Append($@"FILTER(LANG(?ejeX) = 'es' OR LANG(?ejeX) = '' OR !isLiteral(?ejeX))");
                    }
                    if (ejeFechas)
                    {
                        where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                    }
                    else
                    {
                        where.Append($@"}} ORDER BY {orden}(?numero) ");
                    }
                }
                else
                {
                    // Cálculo (SUM|AVG|MIN|MAX)
                    string calculo = itemGrafica.calculo;

                    // Consulta sparql.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append($@"SELECT ?ejeX {calculo}(?aux) AS ?numero ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"FILTER(LANG(?ejeX) = 'es' OR LANG(?ejeX) = '' OR !isLiteral(?ejeX))");
                    if (ejeFechas)
                    {
                        where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                    }
                    else
                    {
                        where.Append($@"}} ORDER BY {orden}(?numero) ");
                    }
                }

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        if (filtroEspecial && string.IsNullOrEmpty(itemGrafica.calculo))
                        {
                            listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, fila["aux"].value, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, string.Empty, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                        }
                    }
                }
                resultadosDimension[itemGrafica] = listaTuplas;
            });

            #region --- Cálculo de los valores del Eje X
            HashSet<string> valuesEje = new HashSet<string>();
            HashSet<string> tipos = new HashSet<string>();

            List<Tuple<string, float>> rangoValor = new List<Tuple<string, float>>();

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                if (item.Value != null && item.Value.Any())
                {
                    if (pGrafica.config.rango)
                    {
                        int indice;
                        float[] sumaDatos = new float[4];
                        string[] rangos = { "1-3", "4-10", "11-30", "30+" };
                        foreach (Tuple<string, string, float> item2 in item.Value)
                        {
                            indice = int.Parse(item2.Item1);
                            switch (indice)
                            {
                                case < 4:
                                    sumaDatos[0] += item2.Item3;
                                    break;
                                case < 11:
                                    sumaDatos[1] += item2.Item3;
                                    break;
                                case < 31:
                                    sumaDatos[2] += item2.Item3;
                                    break;
                                default:
                                    sumaDatos[3] += item2.Item3;
                                    break;
                            }
                        }
                        for (int i = 0; i < sumaDatos.Length; i++)
                        {
                            if (sumaDatos[i] > 0)
                            {
                                rangoValor.Add(new Tuple<string, float>(rangos[i], sumaDatos[i]));
                            }
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, float> item2 in item.Value)
                        {
                            valuesEje.Add(item2.Item1);
                            tipos.Add(item2.Item2);
                        }
                    }
                }
            }

            bool isInt = valuesEje.Where(x => !int.TryParse(x, out int aux)).Count() == 0;

            if (pGrafica.config.rellenarEjeX && isInt && valuesEje.Count > 0)
            {
                int numMin = valuesEje.Min(x => int.Parse(x));
                int numMax = valuesEje.Max(x => int.Parse(x));
                for (int i = numMin; i <= numMax; i++)
                {
                    valuesEje.Add(i.ToString());
                }
            }

            if (ejeFechas || pGrafica.config.rango)
            {
                foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
                {
                    if (item.Value != null && item.Value.Any())
                    {
                        foreach (string valor in valuesEje)
                        {
                            Tuple<string, string, float> tuplaAux = item.Value.FirstOrDefault(x => x.Item1.Equals(valor));
                            if (tuplaAux == null)
                            {
                                item.Value.Add(new Tuple<string, string, float>(valor, "", 0));
                            }
                        }
                    }

                    if (isInt)
                    {
                        resultadosDimension[item.Key] = item.Value.OrderBy(x => int.Parse(x.Item1)).ToList();
                    }
                    else
                    {
                        resultadosDimension[item.Key] = item.Value.OrderBy(x => x.Item1).ToList();
                    }
                }

                if (isInt)
                {
                    valuesEje = new HashSet<string>(valuesEje.OrderBy(item => int.Parse(item)));
                }
                else
                {
                    valuesEje = new HashSet<string>(valuesEje.OrderBy(item => item));
                }
            }
            #endregion

            // Obtención del objeto de la gráfica.
            List<string> listaLabels = new List<string>();
            if (pGrafica.config.rango)
            {
                foreach (Tuple<string, float> itemAux in rangoValor)
                {
                    listaLabels.Add(itemAux.Item1);
                }
            }
            else
            {
                listaLabels = valuesEje.ToList();
            }

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                DatasetBarras dataset = new DatasetBarras();
                List<float> listaData = new List<float>();
                if (pGrafica.config.rango)
                {
                    foreach (Tuple<string, float> itemAux in rangoValor)
                    {
                        listaData.Add(itemAux.Item2);
                    }
                }
                else
                {
                    foreach (Tuple<string, string, float> itemAux in item.Value)
                    {
                        listaData.Add(itemAux.Item3);
                    }
                }
                dataset.data = listaData;

                // Nombre del dato en leyenda.
                dataset.label = GetTextLang(pLang, item.Key.nombre);

                // Color.
                dataset.backgroundColor = ObtenerColores(dataset.data.Count(), item.Key.color);
                dataset.type = item.Key.tipoDimension;

                // Anchura.
                dataset.barPercentage = 1;
                if (item.Key.anchura != 0)
                {
                    dataset.barPercentage = item.Key.anchura;
                }
                // Anchura máxima.
                dataset.maxBarThickness = 300;

                // Stack.
                if (!string.IsNullOrEmpty(item.Key.stack))
                {
                    dataset.stack = item.Key.stack;
                }
                else
                {
                    dataset.stack = Guid.NewGuid().ToString();
                }

                // Eje Y.
                dataset.yAxisID = item.Key.yAxisID;

                // Orden
                dataset.order = item.Key.orden;

                data.labels = listaLabels;
                data.type = item.Key.tipoDimension;
                dimensionesDataset[item.Key] = dataset;
            }

            foreach (Dimension dim in pGrafica.config.dimensiones)
            {
                grafica.data.datasets.Add(dimensionesDataset[dim]);
            }

            return grafica;
        }

        /// <summary>
        /// Crea el objeto de la gráfica (Gráfica de Barras horizontal).
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaBarrasY CrearGraficaBarrasY(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates, bool pNodos)
        {
            // Objeto a devolver.
            GraficaBarrasY grafica = new GraficaBarrasY();
            grafica.type = "bar"; // Por defecto, de tipo bar.

            // Tipo.
            grafica.isVertical = true;

            // Abreviación.
            if (pGrafica.config.abreviar)
            {
                grafica.isAbr = pGrafica.config.abreviar;
            }

            // Porcentage.
            if (pGrafica.config.porcentual)
            {
                grafica.isPercentage = pGrafica.config.porcentual;
            }

            // ID Grupo.
            if (!string.IsNullOrEmpty(pGrafica.idGrupo))
            {
                grafica.groupId = pGrafica.idGrupo;
            }

            // Es fecha.
            if (pListaDates != null && pListaDates.Any() && pListaDates.Contains(pGrafica.config.ejeX))
            {
                grafica.isDate = true;
            }

            // Asignación de Data.
            DataBarrasY data = new DataBarrasY();
            data.datasets = new ConcurrentBag<DatasetBarrasY>();
            grafica.data = data;

            // Asignación de Options.
            Options options = new Options();

            // Orientación
            options.indexAxis = "y";

            options.scales = new Dictionary<string, Eje>();

            // Ejes X
            foreach (EjeXConf item in pGrafica.config.xAxisPrint)
            {
                Eje eje = new Eje();
                eje.position = item.posicion;
                eje.title = new Title();
                eje.title.display = true;

                if (item.nombreEje != null)
                {
                    eje.title.text = GetTextLang(pLang, item.nombreEje);
                }
                else
                {
                    eje.title.text = string.Empty;
                }

                options.scales.Add(item.xAxisID, eje);
            }

            // Animación
            options.animation = new Animation();
            options.animation.duration = 2000;

            // Título
            options.plugins = new Plugin();
            options.plugins.title = new Title();
            options.plugins.title.display = true;
            options.plugins.title.text = GetTextLang(pLang, pGrafica.nombre);

            grafica.options = options;

            ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>> resultadosDimension = new ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>>();
            Dictionary<Dimension, DatasetBarrasY> dimensionesDataset = new Dictionary<Dimension, DatasetBarrasY>();

            bool ejeFechas = false;
            Parallel.ForEach(pGrafica.config.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, itemGrafica =>
            {
                // Determina si en el filtro contiene '=' para tratarlo de manera especial.
                bool filtroEspecial = false;
                if (!string.IsNullOrEmpty(itemGrafica.filtro) && !itemGrafica.filtro.Contains("="))
                {
                    filtroEspecial = true;
                }

                // Orden.                
                string orden = "DESC";
                if (pGrafica.config.orderDesc == true)
                {
                    orden = "ASC";
                }
                foreach (string item in pListaDates)
                {
                    if (item.Contains(pGrafica.config.ejeX))
                    {
                        orden = "ASC";
                        ejeFechas = true;
                        break;
                    }
                }

                // Filtro de página.
                List<string> filtros = new List<string>();

                List<Tuple<string, string, float>> listaTuplas = new List<Tuple<string, string, float>>();
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();
                if (!string.IsNullOrEmpty(pGrafica.config.reciproco))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.config.ejeX }, "ejeX", null, pGrafica.config.reciproco));
                }
                else
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.config.ejeX }, "ejeX"));
                } 
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }, pListaDates: pListaDates));
                }
                if (filtroEspecial)
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }, "aux"));
                }
                else if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }
                if (pNodos)
                {
                    //Nodos            
                    Dictionary<string, string> dicNodos = new Dictionary<string, string>();
                    Dictionary<string, int> scoreNodes = new Dictionary<string, int>();

                    // Consulta sparql.
                    select.Append(mPrefijos);
                    select.Append("SELECT ?s group_concat(?categoria;separator=\",\") AS ?idCategorias ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"?s {pGrafica.propCategoryPath} ?area. ");
                    where.Append("?area roh:categoryNode ?categoria. ");
                    where.Append("MINUS { ?categoria skos:narrower ?hijos } ");
                    where.Append("} ");

                    resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string idCategorias = fila["idCategorias"].value;
                            HashSet<string> categorias = new HashSet<string>(idCategorias.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

                            foreach (string categoria in categorias)
                            {
                                if (!scoreNodes.ContainsKey(categoria))
                                {
                                    scoreNodes.Add(categoria, 0);
                                }

                                scoreNodes[categoria]++;
                            }
                        }
                    }
                    List<string> itemsSeleccionados = scoreNodes.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.Distinct().ToList();

                    // Recuperamos los nombres de categorías y creamos los nodos.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append("SELECT ?categoria ?nombreCategoria ");
                    where.Append("WHERE { ");
                    where.Append("?categoria skos:prefLabel ?nombreCategoria. ");
                    where.Append($@"FILTER(?categoria IN (<{string.Join(">,<", itemsSeleccionados)}>)) ");
                    where.Append("} ");

                    resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            if (!dicNodos.ContainsKey(fila["categoria"].value))
                            {
                                dicNodos.Add(fila["categoria"].value, fila["nombreCategoria"].value);
                            }
                        }
                    }

                    // Añado a la lista de tuplas el nodo con su valor.
                    if (dicNodos != null && dicNodos.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> nodo in dicNodos)
                        {
                            if (scoreNodes.ContainsKey(nodo.Key))
                            {
                                listaTuplas.Add(new Tuple<string, string, float>(nodo.Value, string.Empty, float.Parse(scoreNodes[nodo.Key].ToString().Replace(",", "."), CultureInfo.InvariantCulture)));
                            }
                        }
                    }
                    resultadosDimension[itemGrafica] = listaTuplas.OrderByDescending(x => x.Item3).ToList();
                }
                else if (string.IsNullOrEmpty(itemGrafica.calculo))
                {
                    // Consulta sparql.                    
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    if (filtroEspecial)
                    {
                        select.Append($@"SELECT ?ejeX ?aux COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    else
                    {
                        select.Append($@"SELECT ?ejeX COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    if (filtroEspecial)
                    {
                        where.Append($@"FILTER(LANG(?aux) = 'es' OR LANG(?aux) = '' OR !isLiteral(?aux))");
                    }
                    else
                    {
                        where.Append($@"FILTER(LANG(?ejeX) = 'es' OR LANG(?ejeX) = '' OR !isLiteral(?ejeX))");
                    }
                    if (ejeFechas)
                    {
                        where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                    }
                    else
                    {
                        where.Append($@"}} ORDER BY {orden}(?numero) ");
                    }
                }
                else
                {
                    // Cálculo (SUM|AVG|MIN|MAX)
                    string calculo = itemGrafica.calculo;

                    // Consulta sparql.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append($@"SELECT ?ejeX {calculo}(?aux) AS ?numero ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"FILTER(LANG(?ejeX) = 'es' OR LANG(?ejeX) = '' OR !isLiteral(?ejeX))");
                    if (ejeFechas)
                    {
                        where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                    }
                    else
                    {
                        where.Append($@"}} ORDER BY {orden}(?numero) ");
                    }
                }
                if (!pNodos)
                {
                    resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            if (filtroEspecial && string.IsNullOrEmpty(itemGrafica.calculo))
                            {
                                listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, fila["aux"].value, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, string.Empty, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                            }
                        }
                    }
                    resultadosDimension[itemGrafica] = listaTuplas;
                }
            });

            #region --- Cálculo de los valores del Eje X
            HashSet<string> valuesEje = new HashSet<string>();
            HashSet<string> tipos = new HashSet<string>();

            List<Tuple<string, float>> rangoValor = new List<Tuple<string, float>>();

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                if (item.Value != null && item.Value.Any())
                {
                    if (pGrafica.config.rango)
                    {
                        int indice;
                        float[] sumaDatos = new float[4];
                        string[] rangos = { "1-3", "4-10", "11-30", "30+" };
                        foreach (Tuple<string, string, float> item2 in item.Value)
                        {
                            indice = int.Parse(item2.Item1);
                            switch (indice)
                            {
                                case < 4:
                                    sumaDatos[0] += item2.Item3;
                                    break;
                                case < 11:
                                    sumaDatos[1] += item2.Item3;
                                    break;
                                case < 31:
                                    sumaDatos[2] += item2.Item3;
                                    break;
                                default:
                                    sumaDatos[3] += item2.Item3;
                                    break;
                            }
                        }
                        for (int i = 0; i < sumaDatos.Length; i++)
                        {
                            if (sumaDatos[i] > 0)
                            {
                                rangoValor.Add(new Tuple<string, float>(rangos[i], sumaDatos[i]));
                            }
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, float> item2 in item.Value)
                        {
                            valuesEje.Add(item2.Item1);
                            tipos.Add(item2.Item2);
                        }
                    }
                }
            }

            bool isInt = valuesEje.Where(x => !int.TryParse(x, out int aux)).Count() == 0;

            if (pGrafica.config.rellenarEjeX && isInt && valuesEje.Count > 0)
            {
                int numMin = valuesEje.Min(x => int.Parse(x));
                int numMax = valuesEje.Max(x => int.Parse(x));
                for (int i = numMin; i <= numMax; i++)
                {
                    valuesEje.Add(i.ToString());
                }
            }

            if (ejeFechas || pGrafica.config.rango)
            {
                foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
                {
                    if (item.Value != null && item.Value.Any())
                    {
                        foreach (string valor in valuesEje)
                        {
                            Tuple<string, string, float> tuplaAux = item.Value.FirstOrDefault(x => x.Item1.Equals(valor));
                            if (tuplaAux == null)
                            {
                                item.Value.Add(new Tuple<string, string, float>(valor, "", 0));
                            }
                        }
                    }

                    if (isInt)
                    {
                        resultadosDimension[item.Key] = item.Value.OrderBy(x => int.Parse(x.Item1)).ToList();
                    }
                    else
                    {
                        resultadosDimension[item.Key] = item.Value.OrderBy(x => x.Item1).ToList();
                    }
                }

                if (isInt)
                {
                    valuesEje = new HashSet<string>(valuesEje.OrderBy(item => int.Parse(item)));
                }
                else
                {
                    valuesEje = new HashSet<string>(valuesEje.OrderBy(item => item));
                }
            }
            #endregion

            // Obtención del objeto de la gráfica.
            List<string> listaLabels = new List<string>();
            if (pGrafica.config.rango)
            {
                foreach (Tuple<string, float> itemAux in rangoValor)
                {
                    listaLabels.Add(itemAux.Item1);
                }
            }
            else
            {
                listaLabels = valuesEje.ToList();
            }

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                DatasetBarrasY dataset = new DatasetBarrasY();
                List<float> listaData = new List<float>();
                if (pGrafica.config.rango)
                {
                    foreach (Tuple<string, float> itemAux in rangoValor)
                    {
                        listaData.Add(itemAux.Item2);
                    }
                }
                else
                {
                    foreach (Tuple<string, string, float> itemAux in item.Value)
                    {
                        listaData.Add(itemAux.Item3);
                    }
                }
                dataset.data = listaData;

                // Nombre del dato en leyenda.
                dataset.label = GetTextLang(pLang, item.Key.nombre);

                // Color.
                dataset.backgroundColor = ObtenerColores(dataset.data.Count(), item.Key.color);
                dataset.type = item.Key.tipoDimension;

                // Anchura.
                dataset.barPercentage = 1;
                if (item.Key.anchura != 0)
                {
                    dataset.barPercentage = item.Key.anchura;
                }
                // Anchura máxima.
                dataset.maxBarThickness = 300;

                // Stack.
                if (!string.IsNullOrEmpty(item.Key.stack))
                {
                    dataset.stack = item.Key.stack;
                }
                else
                {
                    dataset.stack = Guid.NewGuid().ToString();
                }

                // Eje X.
                dataset.xAxisID = item.Key.xAxisID;

                // Orden
                dataset.order = item.Key.orden;

                data.labels = listaLabels;
                data.type = item.Key.tipoDimension;
                dimensionesDataset[item.Key] = dataset;
            }

            foreach (Dimension dim in pGrafica.config.dimensiones)
            {
                grafica.data.datasets.Add(dimensionesDataset[dim]);
            }

            return grafica;
        }

        /// <summary>
        /// Crea el objeto de la gráfica (Gráfica circular).
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaCircular CrearGraficaCircular(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates)
        {
            // Objeto a devolver.
            GraficaCircular grafica = new GraficaCircular();
            grafica.type = "pie"; // Por defecto, de tipo pie.

            // Abreviación.
            if (pGrafica.config.abreviar)
            {
                grafica.isAbr = pGrafica.config.abreviar;
            }

            // Porcentage.
            if (pGrafica.config.porcentual)
            {
                grafica.isPercentage = pGrafica.config.porcentual;
            }

            // ID Grupo.
            if (!string.IsNullOrEmpty(pGrafica.idGrupo))
            {
                grafica.groupId = pGrafica.idGrupo;
            }

            // Asignación de Data.
            DataCircular data = new DataCircular();
            data.datasets = new ConcurrentBag<DatasetCircular>();
            grafica.data = data;

            // Asignación de Options.
            Options options = new Options();

            // Animación
            options.animation = new Animation();
            options.animation.duration = 2000;

            // Título
            options.plugins = new Plugin();
            options.plugins.title = new Title();
            options.plugins.title.display = true;
            options.plugins.title.text = GetTextLang(pLang, pGrafica.nombre);

            grafica.options = options;

            ConcurrentDictionary<Dimension, ConcurrentDictionary<string, float>> resultadosDimension = new ConcurrentDictionary<Dimension, ConcurrentDictionary<string, float>>();
            Dictionary<Dimension, DatasetCircular> dimensionesDataset = new Dictionary<Dimension, DatasetCircular>();
            ConcurrentDictionary<string, float> dicNombreData = new ConcurrentDictionary<string, float>();

            Parallel.ForEach(pGrafica.config.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, itemGrafica =>
            {
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();

                // Consulta sparql.
                List<string> filtros = new List<string>();
                Dictionary<string, float> dicResultados = new Dictionary<string, float>();
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }, pListaDates: pListaDates));
                }
                if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }, "tipo"));
                }

                select.Append(mPrefijos);
                select.Append($@"SELECT ?tipo COUNT(?s) AS ?numero ");
                where.Append("WHERE { ");
                foreach (string item in filtros)
                {
                    where.Append(item);
                }
                string limite = itemGrafica.limite == 0 ? "" : "LIMIT " + itemGrafica.limite;
                where.Append($@"FILTER(LANG(?tipo) = '{pLang}' OR LANG(?tipo) = '' OR !isLiteral(?tipo)) ");
                where.Append($@"}} ORDER BY DESC (?numero) {limite}");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        try
                        {
                            dicNombreData.TryAdd(fila["tipo"].value, Int32.Parse(fila["numero"].value));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("No se ha configurado el apartado de dimensiones.");
                        }
                    }
                    resultadosDimension[itemGrafica] = dicNombreData;
                }
            });

            // Lista de los ordenes de las revistas.
            List<string> listaNombres = new List<string>();
            List<string> listaLabels = new List<string>();
            // Ordeno los datos
            Dictionary<string, float> ordered = dicNombreData.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            List<float> listaData = new List<float>();
            foreach (KeyValuePair<string, float> nombreData in ordered)
            {
                listaNombres.Add(nombreData.Key);
                listaData.Add(nombreData.Value);
            }

            DatasetCircular dataset = new DatasetCircular();
            dataset.data = listaData;

            List<string> listaColores = new List<string>();

            foreach (string orden in listaNombres)
            {
                foreach (KeyValuePair<Dimension, ConcurrentDictionary<string, float>> item in resultadosDimension)
                {
                    if (item.Key.colorMaximo != null)
                    {
                        listaColores = ObtenerDegradadoColores(item.Key.colorMaximo, item.Key.color, item.Value.Count());
                    }
                    else
                    {
                        string nombreRevista = item.Key.filtro.Contains("=") ? item.Key.filtro.Split("=")[1].Split("@")[0].Substring(1, item.Key.filtro.Split("=")[1].Split("@")[0].Length - 2) : "";
                        if (nombreRevista == orden)
                        {
                            // Nombre del dato en leyenda.
                            dataset.label = GetTextLang(pLang, item.Key.nombre);
                            listaLabels.Add(GetTextLang(pLang, item.Key.nombre));
                            // Color. 
                            listaColores.Add(item.Key.color);
                        }
                    }
                }
            }
            if (listaLabels.Any())
            {
                data.labels = listaLabels;
            }
            else
            {
                data.labels = listaNombres;
            }
            dataset.backgroundColor = listaColores;

            // HoverOffset por defecto.
            dataset.hoverOffset = 4;

            grafica.data.datasets.Add(dataset);

            return grafica;
        }

        /// <summary>
        /// Crea el objeto de la gráfica (Gráfica nodos).
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaNodos CrearGraficaNodos(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates)
        {
            GraficaNodos grafica = new GraficaNodos();

            // Abreviación.
            if (pGrafica.config.abreviar)
            {
                grafica.isAbr = pGrafica.config.abreviar;
            }

            // Porcentage.
            if (pGrafica.config.porcentual)
            {
                grafica.isPercentage = pGrafica.config.porcentual;
            }

            // ID Grupo.
            if (!string.IsNullOrEmpty(pGrafica.idGrupo))
            {
                grafica.groupId = pGrafica.idGrupo;
            }

            // Tipo.
            grafica.isNodes = true;

            #region --- Configuración
            // Opciones interactivas
            grafica.userZoomingEnabled = false;
            grafica.zoomingEnabled = true;
            grafica.minZoom = 0.5f;
            grafica.maxZoom = 4.0f;

            // Layout Base
            grafica.layout = new Layout();
            grafica.layout.name = "cose";
            grafica.layout.idealEdgeLength = 100;
            grafica.layout.nodeOverlap = 20;
            grafica.layout.refresh = 20;
            grafica.layout.fit = true;
            grafica.layout.padding = 30;
            grafica.layout.randomize = false;
            grafica.layout.componentSpacing = 50;
            grafica.layout.nodeRepulsion = 400000;
            grafica.layout.edgeElasticity = 100;
            grafica.layout.nestingFactor = 5;
            grafica.layout.gravity = 80;
            grafica.layout.numIter = 1000;
            grafica.layout.initialTemp = 200;
            grafica.layout.coolingFactor = 0.95f;
            grafica.layout.minTemp = 1.0f;

            // Titulo
            grafica.title = pGrafica.config.dimensiones.FirstOrDefault().nombre.Values.FirstOrDefault();

            // Layout Nodos/Lineas
            grafica.style = new List<Style>();
            Style estiloNodo = new Style();

            // Nodos
            LayoutStyle layoutNodos = new LayoutStyle();
            layoutNodos.width = "mapData(score, 0, 80, 10, 90)";
            layoutNodos.height = "mapData(score, 0, 80, 10, 90)";
            layoutNodos.content = "data(name)";
            layoutNodos.font_size = "12px";
            layoutNodos.font_family = "Roboto";
            layoutNodos.background_color = pGrafica.config.dimensiones.FirstOrDefault().colorNodo;
            layoutNodos.text_outline_width = "0px";
            layoutNodos.overlay_padding = "6px";
            layoutNodos.line_color = "";
            layoutNodos.z_index = "10";

            estiloNodo.selector = "node";
            estiloNodo.style = layoutNodos;
            grafica.style.Add(estiloNodo);

            // Líneas
            LayoutStyle layoutLineas = new LayoutStyle();
            Style estiloLinea = new Style();
            estiloLinea.selector = "edge";
            layoutLineas.curve_style = "haystack";
            layoutLineas.content = "data(name)";
            layoutLineas.font_size = "24px";
            layoutLineas.font_family = "Roboto";
            layoutLineas.background_color = "#6cafd3";
            layoutLineas.haystack_radius = "0.5";
            layoutLineas.opacity = "0.5";
            layoutLineas.width = "mapData(weight, 0, 10, 0, 10)";
            layoutLineas.overlay_padding = "1px";
            layoutLineas.z_index = "11";
            layoutLineas.line_color = pGrafica.config.dimensiones.FirstOrDefault().colorLinea;
            estiloLinea.style = layoutLineas;
            grafica.style.Add(estiloLinea);
            #endregion

            //Nodos            
            Dictionary<string, string> dicNodos = new Dictionary<string, string>();

            //Relaciones
            Dictionary<string, List<DataQueryRelaciones>> dicRelaciones = new Dictionary<string, List<DataQueryRelaciones>>();

            //Respuesta
            List<DataItemRelacion> itemsRelacion = new List<DataItemRelacion>();

            Dictionary<string, List<string>> dicResultadosAreaRelacionAreas = new Dictionary<string, List<string>>();
            Dictionary<string, int> scoreNodes = new Dictionary<string, int>();

            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            Parallel.ForEach(pGrafica.config.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, itemGrafica =>
            {
                // Consulta sparql.
                List<string> filtros = new List<string>();
                Dictionary<string, float> dicResultados = new Dictionary<string, float>();
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }, pListaDates: pListaDates));
                }
                if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }

                // Consulta sparql.
                select.Append(mPrefijos);
                select.Append("SELECT ?s group_concat(?categoria;separator=\",\") AS ?idCategorias ");
                where.Append("WHERE { ");
                foreach (string item in filtros)
                {
                    where.Append(item);
                }
                where.Append($@"?s {pGrafica.propCategoryPath} ?area. ");
                where.Append("?area roh:categoryNode ?categoria. ");
                where.Append("MINUS { ?categoria skos:narrower ?hijos } ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string idCategorias = fila["idCategorias"].value;
                        HashSet<string> categorias = new HashSet<string>(idCategorias.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));

                        foreach (string categoria in categorias)
                        {
                            if (!scoreNodes.ContainsKey(categoria))
                            {
                                scoreNodes.Add(categoria, 0);
                            }

                            scoreNodes[categoria]++;

                            if (!dicResultadosAreaRelacionAreas.ContainsKey(categoria))
                            {
                                dicResultadosAreaRelacionAreas.Add(categoria, new List<string>());
                            }

                            dicResultadosAreaRelacionAreas[categoria].AddRange(categorias.Except(new List<string>() { categoria }));
                        }
                    }
                }

                ProcesarRelaciones("Category", dicResultadosAreaRelacionAreas, ref dicRelaciones);

                int maximasRelaciones = 0;
                foreach (KeyValuePair<string, List<DataQueryRelaciones>> sujeto in dicRelaciones)
                {
                    foreach (DataQueryRelaciones relaciones in sujeto.Value)
                    {
                        foreach (Datos relaciones2 in relaciones.idRelacionados)
                        {
                            maximasRelaciones = Math.Max(maximasRelaciones, relaciones2.numVeces);
                        }
                    }
                }

                // Creamos los nodos y las relaciones en función de pNumAreas.
                int pNumAreas = pGrafica.config.dimensiones.FirstOrDefault().numMaxNodos;

                Dictionary<string, int> numRelaciones = new Dictionary<string, int>();
                foreach (KeyValuePair<string, List<DataQueryRelaciones>> sujeto in dicRelaciones)
                {
                    if (!numRelaciones.ContainsKey(sujeto.Key))
                    {
                        numRelaciones.Add(sujeto.Key, 0);
                    }
                    foreach (DataQueryRelaciones relaciones in sujeto.Value)
                    {
                        foreach (Datos relaciones2 in relaciones.idRelacionados)
                        {
                            if (!numRelaciones.ContainsKey(relaciones2.idRelacionado))
                            {
                                numRelaciones.Add(relaciones2.idRelacionado, 0);
                            }
                            numRelaciones[sujeto.Key] += relaciones2.numVeces;
                            numRelaciones[relaciones2.idRelacionado] += relaciones2.numVeces;
                        }
                    }
                }

                List<string> itemsSeleccionados = numRelaciones.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.Distinct().ToList();
                if (itemsSeleccionados.Count() > pNumAreas)
                {
                    itemsSeleccionados = itemsSeleccionados.GetRange(0, pNumAreas);
                }

                if (itemsSeleccionados.Count > 0)
                {
                    // Recuperamos los nombres de categorías y creamos los nodos.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append("SELECT ?categoria ?nombreCategoria ");
                    where.Append("WHERE { ");
                    where.Append("?categoria skos:prefLabel ?nombreCategoria. ");
                    where.Append($@"FILTER(?categoria IN (<{string.Join(">,<", itemsSeleccionados)}>)) ");
                    where.Append("} ");

                    resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            if (!dicNodos.ContainsKey(fila["categoria"].value))
                            {
                                dicNodos.Add(fila["categoria"].value, fila["nombreCategoria"].value);
                            }
                        }
                    }

                    // Nodos. 
                    if (dicNodos != null && dicNodos.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> nodo in dicNodos)
                        {
                            string clave = nodo.Key;
                            Data data = new Data(clave, nodo.Value, null, null, null, "nodes", Data.Type.icon_area);
                            if (scoreNodes.ContainsKey(clave))
                            {
                                data.score = scoreNodes[clave];
                                data.name = data.name + " (" + data.score + ")";
                            }
                            DataItemRelacion dataColabo = new DataItemRelacion(data, true, true);
                            itemsRelacion.Add(dataColabo);
                        }
                    }

                    // Relaciones.
                    if (dicRelaciones != null && dicRelaciones.Count > 0)
                    {
                        foreach (KeyValuePair<string, List<DataQueryRelaciones>> sujeto in dicRelaciones)
                        {
                            if (itemsSeleccionados.Contains(sujeto.Key))
                            {
                                foreach (DataQueryRelaciones relaciones in sujeto.Value)
                                {
                                    foreach (Datos relaciones2 in relaciones.idRelacionados)
                                    {
                                        if (itemsSeleccionados.Contains(relaciones2.idRelacionado))
                                        {
                                            string id = $@"{sujeto.Key}~{relaciones.nombreRelacion}~{relaciones2.idRelacionado}~{relaciones2.numVeces}";
                                            Data data = new Data(id, relaciones.nombreRelacion, sujeto.Key, relaciones2.idRelacionado, CalcularGrosor(maximasRelaciones, relaciones2.numVeces), "edges", Data.Type.relation_document);
                                            DataItemRelacion dataColabo = new DataItemRelacion(data, null, null);
                                            itemsRelacion.Add(dataColabo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            grafica.elements = itemsRelacion;
            return grafica;
        }
        #endregion

        #region --- Facetas
        /// <summary>
        /// Lee la configuración y obtiene los datos necesarios para el servicio de facetas.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pIdFaceta">Identificador de la faceta.</param>
        /// <param name="pFiltroFacetas">Filtros de la URL.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltroFacetas, string pLang, bool pGetAll = false)
        {
            // Decode de los filtros.
            pFiltroFacetas = HttpUtility.UrlDecode(pFiltroFacetas);

            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);

            // Obtiene los filtros relacionados con las fechas.
            List<string> listaFacetasAnios = configModel.facetas.Where(x => x.rangoAnio).Select(x => x.filtro).ToList();

            if (configModel != null)
            {
                FacetaConf faceta = configModel.facetas.FirstOrDefault(x => x.filtro == pIdFaceta);
                return CrearFaceta(faceta, configModel.filtro, pFiltroFacetas, pLang, listaFacetasAnios, pGetAll);
            }

            return null;
        }

        /// <summary>
        /// Crea el objeto faceta.
        /// </summary>
        /// <param name="pFacetaConf">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Faceta CrearFaceta(FacetaConf pFacetaConf, string pFiltroBase, string pFiltroFacetas, string pLang, List<string> pListaDates, bool pGetAll = false)
        {
            Faceta faceta = new Faceta();

            faceta.isDate = false;
            if (pFacetaConf.rangoAnio)
            {
                faceta.isDate = true;
            }

            faceta.verTodos = false;
            if (pFacetaConf.verTodos)
            {
                faceta.verTodos = true;
            }

            faceta.numeroItemsFaceta = 10000;
            if (pFacetaConf.numeroItemsFaceta != 0 && !pGetAll)
            {
                faceta.numeroItemsFaceta = pFacetaConf.numeroItemsFaceta;
            }

            faceta.ordenAlfaNum = false;
            if (pFacetaConf.ordenAlfaNum != false)
            {
                faceta.ordenAlfaNum = true;
            }

            faceta.tesauro = false;
            if (pFacetaConf.tesauro != false)
            {
                faceta.tesauro = true;
            }

            faceta.id = pFacetaConf.filtro;
            faceta.nombre = GetTextLang(pLang, pFacetaConf.nombre);
            faceta.items = new List<ItemFaceta>();

            // Filtro de página.
            List<string> filtros = new List<string>();
            filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
            if (!faceta.tesauro)
            {
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFacetaConf.filtro }, "nombreFaceta"));
            }
            else
            {
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFacetaConf.filtro }, "categoria"));
            }
            if (!string.IsNullOrEmpty(pFiltroFacetas))
            {
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }, pListaDates: pListaDates));
            }
            Dictionary<string, float> dicResultados = new Dictionary<string, float>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            if (!faceta.tesauro)
            {
                select.Append(mPrefijos);
                select.Append($@"SELECT DISTINCT ?nombreFaceta LANG(?nombreFaceta) AS ?lang COUNT(?s) AS ?numero ");
                where.Append("WHERE { ");
                foreach (string item in filtros)
                {
                    where.Append(item);
                }
                where.Append($@"FILTER(LANG(?nombreFaceta) = '{pLang}' OR LANG(?nombreFaceta) = '' OR !isLiteral(?nombreFaceta)) ");
                if (faceta.ordenAlfaNum)
                {
                    where.Append($@"}} ORDER BY ASC (?nombreFaceta) ");
                }
                else
                {
                    where.Append($@"}} ORDER BY DESC (?numero) ");
                }

                where.Append($@"LIMIT {faceta.numeroItemsFaceta} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        ItemFaceta itemFaceta = new ItemFaceta();
                        itemFaceta.nombre = fila["nombreFaceta"].value;
                        itemFaceta.numero = Int32.Parse(fila["numero"].value);

                        // Comprobación si tiene idioma asignado.
                        string lang = "";
                        if (!string.IsNullOrEmpty(fila["lang"].value))
                        {
                            lang = $@"@{fila["lang"].value}";
                        }

                        // Comprobación si es literal o numerico.
                        string filtro = itemFaceta.nombre;
                        if (fila["nombreFaceta"].type == "literal")
                        {
                            filtro = $@"'{filtro}'";
                        }

                        itemFaceta.filtro = $@"{pFacetaConf.filtro}={filtro}{lang}";
                        faceta.items.Add(itemFaceta);
                    }
                }
            }
            else
            {
                select.Append(mPrefijos);
                select.Append($@"SELECT ?categoria ?nombre COUNT(DISTINCT (?s)) AS ?numero ");
                where.Append("WHERE { ");
                foreach (string item in filtros)
                {
                    where.Append(item);
                }
                where.Append("?categoria skos:prefLabel ?nombre. ");
                where.Append($@"}} ORDER BY ASC (?categoria) ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    Dictionary<ItemFaceta, int> itemsFaceta = new Dictionary<ItemFaceta, int>();
                    int maxNivel = 0;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        ItemFaceta itemFaceta = new ItemFaceta();
                        itemFaceta.idTesauro = fila["categoria"].value.Substring(fila["categoria"].value.LastIndexOf("_") + 1);
                        itemFaceta.nombre = fila["nombre"].value;
                        itemFaceta.numero = Int32.Parse(fila["numero"].value);
                        itemFaceta.filtro = $@"{pFacetaConf.filtro}={fila["categoria"].value}";
                        itemFaceta.childsTesauro = new List<ItemFaceta>();
                        // Asigno el nivel del item.
                        int nivel = 0;
                        for (int i = 0; i < itemFaceta.idTesauro.Length; i++)
                        {
                            if (itemFaceta.idTesauro[i] == '0' && (i == 0 || itemFaceta.idTesauro[i - 1] == '.'))
                            {
                                nivel++;
                            }
                        }
                        maxNivel = Math.Max(nivel, maxNivel);
                        itemsFaceta.Add(itemFaceta, nivel);
                    }
                    foreach (KeyValuePair<ItemFaceta, int> item in itemsFaceta)
                    {
                        if (item.Value != 0)
                        {
                            getHijosTesauro(item.Key, item.Value, itemsFaceta);
                        }
                    }
                    faceta.items.AddRange(itemsFaceta.Where(x => x.Value == maxNivel).Select(x => x.Key));
                }
            }

            return faceta;
        }

        private static void getHijosTesauro(ItemFaceta item, int nivel, Dictionary<ItemFaceta, int> itemsFaceta)
        {
            string prefijo = item.idTesauro.Substring(0, item.idTesauro.Length - 2 * nivel + 1);
            item.childsTesauro.AddRange(itemsFaceta.Where(x => x.Value == nivel - 1 && x.Key.idTesauro.StartsWith(prefijo)).Select(x => x.Key));
        }

        #endregion

        #region --- Utils
        /// <summary>
        /// Permite agregar un triple a un recurso.
        /// </summary>
        /// <param name="pResourceApi">API.</param>
        /// <param name="pRecursoID">Id del recurso al que se quiere agregar el triple.</param>
        /// <param name="pTriples">Triple a agregar.</param>
        /// <returns></returns>
        public static bool IncluirTriplesRecurso(ResourceApi pResourceApi, Guid pRecursoID, List<TriplesToInclude> pTriples)
        {
            List<TriplesToInclude> triplesIncluir = new List<TriplesToInclude>();

            foreach (TriplesToInclude triple in pTriples)
            {
                if (triple.NewValue == string.Empty)
                {
                    triple.NewValue = null;
                }
                triplesIncluir.Add(triple);
            }

            Dictionary<Guid, List<TriplesToInclude>> dicTriplesInsertar = new Dictionary<Guid, List<TriplesToInclude>>();
            dicTriplesInsertar.Add(pRecursoID, triplesIncluir);
            Dictionary<Guid, bool> dicInsertado = pResourceApi.InsertPropertiesLoadedResources(dicTriplesInsertar);
            return (dicInsertado != null && dicInsertado.ContainsKey(pRecursoID) && dicInsertado[pRecursoID]);
        }

        /// <summary>
        /// Permite obtener el ID del recurso de la persona mediante el ID del usuario.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <returns></returns>
        public static string GetIdPersonByGnossUser(string pUserId)
        {
            // ID de la persona.
            string idRecurso = string.Empty;

            // Filtro de página.
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append($@"SELECT ?s ");
            where.Append("WHERE { ");
            where.Append($@"?s roh:gnossUser <http://gnoss/{pUserId.ToUpper()}>. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    idRecurso = fila["s"].value;
                }
            }

            return idRecurso;
        }

        /// <summary>
        /// Obtiene los datos de las gráficas guardadas del usuario.
        /// </summary>
        /// <param name="pIdPage">ID de la página.</param>
        /// <returns>Lista de datos de las gráficas.</returns>
        public static List<DataGraphicUser> GetGraficasUserByPageId(string pIdPage)
        {
            // Lista de datos de las gráficas.
            List<DataGraphicUser> listaGraficas = new List<DataGraphicUser>();

            // Filtro de página.
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append($@"SELECT distinct ?datosGraficas ?titulo ?orden ?idPagina ?idGrafica ?filtro ?anchura ");
            where.Append("WHERE { ");
            where.Append($@"<{pIdPage}> roh:metricGraphic ?datosGraficas. ");
            where.Append("?datosGraficas roh:title ?titulo. ");
            where.Append("?datosGraficas roh:order ?orden. ");
            where.Append("?datosGraficas roh:pageId ?idPagina. ");
            where.Append("?datosGraficas roh:graphicId ?idGrafica. ");
            where.Append("OPTIONAL{?datosGraficas roh:filters ?filtro. } ");
            where.Append("?datosGraficas roh:width ?anchura. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    DataGraphicUser data = new DataGraphicUser();
                    data.idRecurso = fila["datosGraficas"].value;
                    data.titulo = fila["titulo"].value;
                    data.orden = Int32.Parse(fila["orden"].value);
                    data.idPagina = fila["idPagina"].value;
                    data.idGrafica = fila["idGrafica"].value;
                    if (fila.ContainsKey("filtro") && !string.IsNullOrEmpty(fila["filtro"].value))
                    {
                        data.filtro = fila["filtro"].value;
                    }
                    data.anchura = fila["anchura"].value;
                    listaGraficas.Add(data);
                }
            }

            return listaGraficas.OrderBy(x => x.orden).ToList();
        }

        /// <summary>
        /// Obtiene el listado de páginas asociadas a un usuario.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <returns></returns>
        public static List<DataPageUser> GetPagesUser(string pUserId)
        {
            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            // Lista de datos de las páginas.
            List<DataPageUser> listaPaginas = new List<DataPageUser>();

            // Filtro de página.
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append($@"SELECT ?datosPagina ?titulo ?orden ");
            where.Append("WHERE { ");
            where.Append($@"<{idRecurso}> roh:metricPage ?datosPagina. ");
            where.Append("?datosPagina roh:title ?titulo. ");
            where.Append("?datosPagina roh:order ?orden. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    DataPageUser data = new DataPageUser();
                    data.idRecurso = fila["datosPagina"].value;
                    data.titulo = fila["titulo"].value;
                    data.orden = Int32.Parse(fila["orden"].value);
                    listaPaginas.Add(data);
                }
            }

            return listaPaginas.OrderBy(x => x.orden).ToList();
        }

        /// <summary>
        /// Guarda a la persona la gráfica que quiere quedarse en su administración.
        /// </summary>
        /// <param name="pTitulo">Título de la gráfica a guardar.</param>
        /// <param name="pAnchura">Anchura de la gráfica a guardar.</param>
        /// <param name="pIdRecursoPagina">ID del recurso de la página.</param>
        /// <param name="pIdPaginaGrafica">ID de la página de la gráfica.<</param>
        /// <param name="pIdGrafica">ID de la gráfica.</param>
        /// <param name="pFiltros">Filtros a aplicar en la gráfica.</param>
        /// <param name="pUserId">ID del usuario conectado.</param>
        public static void GuardarGrafica(string pTitulo, string pAnchura, string pIdPaginaGrafica, string pIdGrafica, string pFiltros, string pUserId, string pIdRecursoPagina = null, string pTituloPagina = null)
        {
            string idRecursoPagina = pIdRecursoPagina;
            if (string.IsNullOrEmpty(idRecursoPagina) && !string.IsNullOrEmpty(pTituloPagina))
            {
                idRecursoPagina = CrearPaginaUsuario(pUserId, pTituloPagina);
            }

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            mResourceApi.ChangeOntoly("person");

            Guid shortId = mResourceApi.GetShortGuid(idRecurso);
            Guid entidadGuid = Guid.NewGuid();
            List<TriplesToInclude> triplesInclude = new List<TriplesToInclude>();
            string predicadoBase = "http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic|";
            string idRecursoGrafica = $@"{mResourceApi.GraphsUrl}items/MetricGraphic_{shortId}_{entidadGuid}";
            string valorEntidadAuxiliar = $@"{idRecursoPagina}|{idRecursoGrafica}";
            string valorBase = $@"{valorEntidadAuxiliar}|";

            // Título de la página
            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/title",
                NewValue = valorBase + pTitulo
            });

            // Orden de la gráfica
            int orden = 0;
            List<DataGraphicUser> listaGraficas = GetGraficasUserByPageId(idRecursoPagina);
            foreach (DataGraphicUser item in listaGraficas)
            {
                if (item.orden > orden)
                {
                    orden = item.orden;
                }
            }
            orden++;

            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/order",
                NewValue = valorBase + orden
            });

            // ID de la página de la gráfica
            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/pageId",
                NewValue = valorBase + pIdPaginaGrafica
            });

            // ID de la gráfica
            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/graphicId",
                NewValue = valorBase + pIdGrafica
            });

            // Filtros
            if (!string.IsNullOrEmpty(pFiltros))
            {
                triplesInclude.Add(new TriplesToInclude
                {
                    Description = false,
                    Title = false,
                    Predicate = predicadoBase + "http://w3id.org/roh/filters",
                    NewValue = valorBase + pFiltros
                });
            }

            // Anchura
            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/width",
                NewValue = valorBase + pAnchura
            });

            bool insertado = IncluirTriplesRecurso(mResourceApi, shortId, triplesInclude);
        }

        /// <summary>
        /// Permite guardar los datos de una gráfica asignados a un usuario.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pIdPagina">ID de la página de la gráfica.</param>
        /// <param name="pIdGrafica">ID de la gráfica.</param>
        /// <param name="pFiltro">Filtro de la gráfica.</param>
        public static string CrearPaginaUsuario(string pUserId, string pTitulo)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid shortId = mResourceApi.GetShortGuid(idRecurso);
            Guid entidadGuid = Guid.NewGuid();
            List<TriplesToInclude> triplesInclude = new List<TriplesToInclude>();
            string predicadoBase = "http://w3id.org/roh/metricPage|";
            string valorEntidadAuxiliar = $@"{mResourceApi.GraphsUrl}items/MetricPage_{shortId}_{entidadGuid}";
            string valorBase = $@"{valorEntidadAuxiliar}|";

            // Título de la página
            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/title",
                NewValue = valorBase + pTitulo
            });

            // Orden de la página
            int orden = 0;
            List<DataPageUser> listaPaginas = GetPagesUser(pUserId);
            foreach (DataPageUser item in listaPaginas)
            {
                if (item.orden > orden)
                {
                    orden = item.orden;
                }
            }
            orden++;

            triplesInclude.Add(new TriplesToInclude
            {
                Description = false,
                Title = false,
                Predicate = predicadoBase + "http://w3id.org/roh/order",
                NewValue = valorBase + orden
            });

            bool insertado = IncluirTriplesRecurso(mResourceApi, shortId, triplesInclude);
            if (insertado)
            {
                return valorEntidadAuxiliar;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Borra la relación de la gráfica.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pRecursoId">ID del recurso a borrar el triple.</param>
        public static void BorrarGrafica(string pUserId, string pPageID, string pGraphicID)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listaTriplesBorrado = new List<RemoveTriples>();

            RemoveTriples triple = new RemoveTriples();
            triple.Title = false;
            triple.Description = false;
            triple.Predicate = $@"http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic";
            triple.Value = pPageID + "|" + pGraphicID;
            listaTriplesBorrado.Add(triple);

            dicBorrado.Add(guid, listaTriplesBorrado);
            Dictionary<Guid, bool> eliminado = mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
            ReordenarGráficas(pUserId, pPageID);
        }

        /// <summary>
        /// Borra la relación de la página.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pPageID">ID del recurso a borrar el triple.</param>
        public static void BorrarPagina(string pUserId, string pPageID)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listaTriplesBorrado = new List<RemoveTriples>();

            RemoveTriples triple = new RemoveTriples();
            triple.Title = false;
            triple.Description = false;
            triple.Predicate = $@"http://w3id.org/roh/metricPage";
            triple.Value = pPageID;
            listaTriplesBorrado.Add(triple);

            dicBorrado.Add(guid, listaTriplesBorrado);
            Dictionary<Guid, bool> eliminado = mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
            ReordenarPaginas(pUserId);
        }

        /// <summary>
        /// Reordena las páginas después de un cambio
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        public static void ReordenarPaginas(string pUserId)
        {
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            List<DataPageUser> listaPaginas = GetPagesUser(pUserId).OrderBy(x => x.orden).ToList();

            int index = 1;
            foreach (DataPageUser pagina in listaPaginas)
            {
                if (pagina.orden != index)
                {
                    TriplesToModify triple = new TriplesToModify();
                    triple.Predicate = "http://w3id.org/roh/metricPage|http://w3id.org/roh/order";
                    triple.NewValue = pagina.idRecurso + "|" + index;
                    triple.OldValue = pagina.idRecurso + "|" + pagina.orden;
                    listaTriplesModificacion.Add(triple);
                }
                index++;
            }

            dicModificacion.Add(guid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Edita el nombre de la página de usuario.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pPageID">ID del recurso a editar el triple.</param>
        /// <param name="pNewTitle">Nuevo título de la página.</param>
        /// <param name="pOldTitle">Anterior título de la página.</param>
        public static void EditarNombrePagina(string pUserId, string pPageID, string pNewTitle, string pOldTitle)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);

            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();

            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = "http://w3id.org/roh/metricPage|http://w3id.org/roh/title";
            triple.NewValue = pPageID + "|" + pNewTitle;
            triple.OldValue = pPageID + "|" + pOldTitle;
            listaTriplesModificacion.Add(triple);

            dicModificacion.Add(guid, listaTriplesModificacion);
            Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra la relación de la página.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pRecursoId">ID del recurso a borrar el triple.</param>
        public static void EditarOrdenPagina(string pUserId, string pPageID, int pNewOrder, int pOldOrder)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            List<DataPageUser> listaPaginas = GetPagesUser(pUserId).OrderBy(x => x.orden).ToList();
            foreach (DataPageUser pagina in listaPaginas)
            {
                if (pagina.orden >= pNewOrder && pagina.orden < pOldOrder)
                {
                    pagina.orden++;
                }
                else if (pagina.orden <= pNewOrder && pagina.orden > pOldOrder)
                {
                    pagina.orden--;
                }
            }
            listaPaginas[pOldOrder - 1].orden = pNewOrder;
            int index = 1;
            foreach (DataPageUser pagina in listaPaginas)
            {
                if (pagina.orden != index)
                {
                    TriplesToModify triple = new TriplesToModify();
                    triple.Predicate = "http://w3id.org/roh/metricPage|http://w3id.org/roh/order";
                    triple.NewValue = pagina.idRecurso + "|" + pagina.orden;
                    triple.OldValue = pagina.idRecurso + "|" + index;
                    listaTriplesModificacion.Add(triple);
                }
                index++;
            }

            dicModificacion.Add(guid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra la relación de la página.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pRecursoId">ID del recurso a borrar el triple.</param>
        public static void EditarNombreGrafica(string pUserId, string pPageID, string pGraphicID, string pNewTitle, string pOldTitle)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = $@"http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic|http://w3id.org/roh/title";
            triple.NewValue = pPageID + "|" + pGraphicID + "|" + pNewTitle;
            triple.OldValue = pPageID + "|" + pGraphicID + "|" + pOldTitle;
            listaTriplesModificacion.Add(triple);

            dicModificacion.Add(guid, listaTriplesModificacion);
            Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra la relación de la página.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pRecursoId">ID del recurso a borrar el triple.</param>
        public static void EditarOrdenGrafica(string pUserId, string pPageID, string pGraphicID, int pNewOrder, int pOldOrder)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            List<DataGraphicUser> listaGraficas = GetGraficasUserByPageId(pPageID).OrderBy(x => x.orden).ToList();
            foreach (DataGraphicUser grafica in listaGraficas)
            {
                if (grafica.orden >= pNewOrder && grafica.orden < pOldOrder)
                {
                    grafica.orden++;
                }
                else if (grafica.orden <= pNewOrder && grafica.orden > pOldOrder)
                {
                    grafica.orden--;
                }
            }
            listaGraficas[pOldOrder - 1].orden = pNewOrder;
            int index = 1;
            foreach (DataGraphicUser grafica in listaGraficas)
            {
                if (grafica.orden != index)
                {
                    TriplesToModify triple = new TriplesToModify();
                    triple.Predicate = "http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic|http://w3id.org/roh/order";
                    triple.NewValue = pPageID + "|" + grafica.idRecurso + "|" + grafica.orden;
                    triple.OldValue = pPageID + "|" + grafica.idRecurso + "|" + index;
                    listaTriplesModificacion.Add(triple);
                }
                index++;
            }

            dicModificacion.Add(guid, listaTriplesModificacion);
            Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }
        /// <summary>
        /// Reordena las gráficas después de un cambio
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        public static void ReordenarGráficas(string pUserId, string pPageID)
        {
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            List<DataGraphicUser> listaGraficas = GetGraficasUserByPageId(pPageID).OrderBy(x => x.orden).ToList();

            int index = 1;
            foreach (DataGraphicUser grafica in listaGraficas)
            {
                if (grafica.orden != index)
                {
                    TriplesToModify triple = new TriplesToModify();
                    triple.Predicate = "http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic|http://w3id.org/roh/order";
                    triple.NewValue = pPageID + "|" + grafica.idRecurso + "|" + index;
                    triple.OldValue = pPageID + "|" + grafica.idRecurso + "|" + grafica.orden;
                    listaTriplesModificacion.Add(triple);
                }
                index++;
            }

            dicModificacion.Add(guid, listaTriplesModificacion);
            Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra la relación de la página.
        /// </summary>
        /// <param name="pUserId">ID del usuario.</param>
        /// <param name="pRecursoId">ID del recurso a borrar el triple.</param>
        public static void EditarAnchuraGrafica(string pUserId, string pPageID, string pGraphicID, int pNewWidth, int pOldWidth)
        {
            mResourceApi.ChangeOntoly("person");

            // ID del recurso del usuario.
            string idRecurso = GetIdPersonByGnossUser(pUserId);

            Guid guid = mResourceApi.GetShortGuid(idRecurso);
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = $@"http://w3id.org/roh/metricPage|http://w3id.org/roh/metricGraphic|http://w3id.org/roh/width";
            triple.NewValue = pPageID + "|" + pGraphicID + "|" + pNewWidth;
            triple.OldValue = pPageID + "|" + pGraphicID + "|" + pOldWidth;
            listaTriplesModificacion.Add(triple);

            dicModificacion.Add(guid, listaTriplesModificacion);
            Dictionary<Guid, bool> modificado = mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Obtiene la lista de configuraciones.
        /// </summary>
        public static List<ConfigModel> TabTemplates
        {
            get
            {
                if (mTabTemplates == null || mTabTemplates.Count != Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configGraficas").Count())
                {
                    mTabTemplates = new List<ConfigModel>();
                    foreach (string file in Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configGraficas"))
                    {
                        ConfigModel tab = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(file));
                        mTabTemplates.Add(tab);
                    }
                }
                return mTabTemplates.OrderBy(x => x.orden).ToList();
            }
        }

        /// <summary>
        /// Crea la lista de colores para rellenar las gráficas.
        /// </summary>
        /// <param name="pNumVeces">Número de la lista.</param>
        /// <param name="pColorHex">Color a rellenar.</param>
        /// <returns></returns>
        public static List<string> ObtenerColores(int pNumVeces, string pColorHex)
        {
            List<string> colores = new List<string>();
            for (int i = 0; i < pNumVeces; i++)
            {
                colores.Add(pColorHex);
            }
            return colores;
        }

        /// <summary>
        /// Splitea los filtros para tratarlos.
        /// </summary>
        /// <param name="pListaFiltros">Listado de filtros.</param>
        /// <param name="pNombreVar">Nombre a poner a la última variable.</param>
        /// <returns></returns>
        public static List<string> ObtenerFiltros(List<string> pListaFiltros, string pNombreVar = null, List<string> pListaDates = null, string pReciproco = null)
        {
            // Split por filtro.
            List<string> listaAux = new List<string>();
            foreach (string filtro in pListaFiltros)
            {
                // --- ÑAPA
                string aux = filtro.Replace(" & ", "|||");
                string[] array = aux.Split("&", StringSplitOptions.RemoveEmptyEntries);
                List<string> lista = array.Select(x => x.Replace("|||", " & ")).ToList();
                listaAux.AddRange(lista);
            }

            List<string> filtrosQuery = new List<string>();

            // Split por salto de ontología.
            if (!string.IsNullOrEmpty(pReciproco))
            {

            }
            int i = 0;
            foreach (string item in listaAux)
            {
                bool isDate = false;
                if (pListaDates != null && pListaDates.Any() && pListaDates.Contains(item.Split("=")[0]))
                {
                    isDate = true;
                }
                filtrosQuery.Add(TratarParametros(item, "?s", i, pNombreVar, isDate, pReciproco));

                i += 10;
            }

            return filtrosQuery;
        }

        /// <summary>
        /// Según el tipo de parametros, los trata de una manera u otra para el filtro.
        /// </summary>
        /// <param name="pFiltro">Filtro a tratar.</param>
        /// <param name="pVarAnterior">Sujeto.</param>
        /// <param name="pAux">Iterador incremental.</param>
        /// <param name="pNombreVar">Nombre de la última variable.</param>
        /// <returns></returns>
        public static string TratarParametros(string pFiltro, string pVarAnterior, int pAux, string pNombreVar = null, bool pIsDate = false, string pReciproco = null)
        {
            StringBuilder filtro = new StringBuilder();
            string[] filtros = pFiltro.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;

            string[] filtrosReciproco = null;
            // TODO Revisar con más casos o ejemplos para ver si funciona totalmente bien
            if (!string.IsNullOrEmpty(pReciproco))
            {
                filtrosReciproco = pReciproco.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
                filtro.Append($@"?aux {filtrosReciproco[0].Split('=').FirstOrDefault()} {filtrosReciproco[0].Split('=').LastOrDefault()}. ");
                int j = 0;
                pVarAnterior = "?aux";
                foreach (string filtroReciproco in filtrosReciproco)
                {
                    j++;
                    if (j == 1)
                    {
                        continue;
                    }
                    int pAuxR = pAux;
                    if (!filtroReciproco.Contains("="))
                    {
                        string varActual = $@"?{filtroReciproco.Substring(filtroReciproco.IndexOf(":") + 1)}{pAuxR}";
                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{filtroReciproco} ");
                        // Si es el último, le asignamos el nombre que queramos.
                        if (j == filtrosReciproco.Length)
                        {
                            filtro.Append($@"?s. ");
                        }
                        else
                        {
                            filtro.Append($@"{varActual}. ");
                        }
                        pVarAnterior = varActual;
                        pAuxR++;
                    }
                }
                pVarAnterior = "?aux";
            }
            foreach (string parteFiltro in filtros)
            {
                i++;
                if (!parteFiltro.Contains("="))
                {
                    string varActual = $@"?{parteFiltro.Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";
                    filtro.Append($@"{pVarAnterior} ");
                    filtro.Append($@"{parteFiltro} ");
                    // Si es el último, le asignamos el nombre que queramos.
                    if (i == filtros.Length && !string.IsNullOrEmpty(pNombreVar))
                    {
                        filtro.Append($@"?{pNombreVar}. ");
                    }
                    else
                    {
                        filtro.Append($@"{varActual}. ");
                    }
                    pVarAnterior = varActual;
                    pAux++;
                }
                else
                {
                    string varActual = $@"{parteFiltro.Split("=")[1]}";
                    if (varActual.StartsWith("'"))
                    {
                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{parteFiltro.Split("=")[0]} ");

                        if (pNombreVar != null)
                        {
                            filtro.Append($@"?{pNombreVar}. ");
                        }
                        else
                        {
                            filtro.Append($@"{varActual}. ");
                        }
                    }
                    else if (pIsDate && (varActual.Contains("-") || varActual.Equals("lastyear") || varActual.Equals("fiveyears")))
                    {
                        string fechaInicio = "";
                        string fechaFin = "";
                        string varActualAux = $@"?{parteFiltro.Split("=")[0].Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";
                        if (varActual.Contains("-"))
                        {
                            fechaInicio = varActual.Split("-")[0];
                            fechaFin = varActual.Split("-")[1];
                        }
                        else if (varActual.Equals("lastyear"))
                        {
                            fechaInicio = DateTime.Now.Year.ToString();
                            fechaFin = DateTime.Now.Year.ToString();
                        }
                        else if (varActual.Equals("fiveyears"))
                        {
                            fechaInicio = (DateTime.Now.Year - 4).ToString();
                            fechaFin = DateTime.Now.Year.ToString();
                        }
                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{parteFiltro.Split("=")[0]} ");
                        filtro.Append($@"{varActualAux}. ");
                        filtro.Append($@"FILTER({varActualAux} >= {fechaInicio} && {varActualAux} <= {fechaFin}) ");
                    }
                    else
                    {
                        // Si el valor es númerico, se le asigna con el FILTER.
                        string varActualAux = $@"?{parteFiltro.Split("=")[0].Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";
                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{parteFiltro.Split("=")[0]} ");
                        filtro.Append($@"{varActualAux}. ");

                        // Si es un tesauro.
                        if (varActual.StartsWith($@"http://"))
                        {
                            filtro.Append($@"FILTER({varActualAux} = <{varActual}>) ");
                        }
                        else
                        {
                            filtro.Append($@"FILTER({varActualAux} = {varActual}) ");
                        }
                    }

                }
            }
            return filtro.ToString();
        }

        /// <summary>
        /// Obtiene el idioma de del diccionario de idiomas.
        /// </summary>
        /// <param name="pLang">Idioma.</param>
        /// <param name="pValores">Diccionario.</param>
        /// <returns></returns>
        public static string GetTextLang(string pLang, Dictionary<string, string> pValores)
        {
            if (pValores == null)
            {
                return "";
            }
            else if (pValores.ContainsKey(pLang))
            {
                return pValores[pLang];
            }
            else if (pValores.ContainsKey("es"))
            {
                return pValores["es"];
            }
            else if (pValores.Count > 0)
            {
                return pValores.Values.First();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Obtiene la lista de colores degradados.
        /// </summary>
        /// <param name="pColorMax">Color máximo como límite.</param>
        /// <param name="pColorMin">Color mínimo como límite.</param>
        /// <param name="pNumColores">Número de colores a devolver.</param>
        /// <returns></returns>
        public static List<string> ObtenerDegradadoColores(string pColorMax, string pColorMin, int pNumColores)
        {
            List<string> listaColores = new List<string>();
            if (pColorMax.Length < 7 || pColorMin.Length < 7)
            {
                pColorMax = "#FFFFFF";
                pColorMin = "#000000";
            }
            int rMax = Convert.ToInt32(pColorMax.Substring(1, 2), 16);
            int gMax = Convert.ToInt32(pColorMax.Substring(3, 2), 16);
            int bMax = Convert.ToInt32(pColorMax.Substring(5, 2), 16);
            int rMin = Convert.ToInt32(pColorMin.Substring(1, 2), 16);
            int gMin = Convert.ToInt32(pColorMin.Substring(3, 2), 16);
            int bMin = Convert.ToInt32(pColorMin.Substring(5, 2), 16);

            for (int i = 0; i < pNumColores; i++)
            {
                int rAverage = rMin + (int)((rMax - rMin) * i / pNumColores);
                int gAverage = gMin + (int)((gMax - gMin) * i / pNumColores);
                int bAverage = bMin + (int)((bMax - bMin) * i / pNumColores);
                string colorHex = '#' + rAverage.ToString("X2") + gAverage.ToString("X2") + bAverage.ToString("X2");
                listaColores.Add(colorHex);
            }

            return listaColores;
        }

        /// <summary>
        /// Procesa las relaciones de las gráficas de nodos.
        /// </summary>
        /// <param name="pNombreRelacion">Nombre de la relación.</param>
        /// <param name="pItems">Número de ítems.</param>
        /// <param name="pDicRelaciones">Diccionario con las relaciones.</param>
        public static void ProcesarRelaciones(string pNombreRelacion, Dictionary<string, List<string>> pItems, ref Dictionary<string, List<DataQueryRelaciones>> pDicRelaciones)
        {
            foreach (string itemA in pItems.Keys)
            {
                if (!pDicRelaciones.ContainsKey(itemA))
                {
                    pDicRelaciones.Add(itemA, new List<DataQueryRelaciones>());
                }
                DataQueryRelaciones dataQueryRelaciones = (pDicRelaciones[itemA].FirstOrDefault(x => x.nombreRelacion == pNombreRelacion));
                if (dataQueryRelaciones == null)
                {
                    dataQueryRelaciones = new DataQueryRelaciones()
                    {
                        nombreRelacion = pNombreRelacion,
                        idRelacionados = new List<Datos>()
                    };
                    pDicRelaciones[itemA].Add(dataQueryRelaciones);
                }
                foreach (string itemB in pItems.Keys)
                {
                    if (itemA != itemB)
                    {
                        if (string.Compare(itemA, itemB, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            int num = pItems[itemA].Intersect(pItems[itemB]).Count();
                            if (num > 0)
                            {
                                dataQueryRelaciones.idRelacionados.Add(new Datos()
                                {
                                    idRelacionado = itemB,
                                    numVeces = num
                                });
                            }
                        }
                    }
                }
                if (dataQueryRelaciones.idRelacionados.Count == 0)
                {
                    pDicRelaciones[itemA].Remove(dataQueryRelaciones);
                }
            }
        }

        /// <summary>
        /// Permite calcular el valor del ancho de la línea según el número de colaboraciones que tenga el nodo.
        /// </summary>
        /// <param name="pMax">Valor máximo.</param>
        /// <param name="pColabo">Número de colaboraciones.</param>
        /// <returns>Ancho de la línea en formate double.</returns>
        public static double CalcularGrosor(int pMax, int pColabo)
        {
            return Math.Round(((double)pColabo / (double)pMax) * 10, 2);
        }
        #endregion

        #region --- Excepciones
        public static void ControlarExcepcionesBarrasX(Grafica pGrafica)
        {
            if (pGrafica.config == null)
            {
                throw new Exception("La gráfica no tiene configuración");
            }
            if (string.IsNullOrEmpty(pGrafica.config.ejeX))
            {
                throw new Exception("No está configurada la propiedad del agrupación del eje x.");
            }
            if (pGrafica.config.yAxisPrint == null)
            {
                throw new Exception("No está configurada la propiedad yAxisPrint");
            }
            if (pGrafica.config.dimensiones == null || pGrafica.config.dimensiones.Count() == 0)
            {
                throw new Exception("No se ha configurado dimensiones.");
            }
        }
        public static void ControlarExcepcionesBarrasY(Grafica pGrafica)
        {
            if (pGrafica.config == null)
            {
                throw new Exception("La gráfica no tiene configuración");
            }
            if (string.IsNullOrEmpty(pGrafica.config.ejeX))
            {
                throw new Exception("No está configurada la propiedad del agrupación del eje x.");
            }
            if (pGrafica.config.xAxisPrint == null)
            {
                throw new Exception("No está configurada la propiedad xAxisPrint");
            }
            if (pGrafica.config.dimensiones == null || pGrafica.config.dimensiones.Count() == 0)
            {
                throw new Exception("No se ha configurado dimensiones.");
            }
        }
        public static void ControlarExcepcionesCircular(Grafica pGrafica)
        {
            if (pGrafica.config == null)
            {
                throw new Exception("La gráfica no tiene configuración");
            }
            if (pGrafica.config.dimensiones == null || pGrafica.config.dimensiones.Count() == 0)
            {
                throw new Exception("No se ha configurado dimensiones.");
            }
        }
        #endregion
    }
}
