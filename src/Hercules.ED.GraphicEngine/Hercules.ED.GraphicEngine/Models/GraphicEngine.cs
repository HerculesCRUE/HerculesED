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

namespace Hercules.ED.GraphicEngine.Models
{
    public static class GraphicEngine
    {
        // Prefijos.
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configJson\prefijos.json")));
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static Guid mCommunityID = mCommunityApi.GetCommunityId();

        #region --- Páginas
        public static Pagina GetPage(string pIdPagina, string pLang)
        {
            // Lectura del JSON de configuración.
            List<ConfigModel> listaConfigModel = null;
            using (StreamReader reader = new StreamReader($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configGraficas\configuration.json"))
            {
                string json = reader.ReadToEnd();
                listaConfigModel = JsonConvert.DeserializeObject<List<ConfigModel>>(json);
            }

            ConfigModel configModel = listaConfigModel.FirstOrDefault(x => x.identificador == pIdPagina);

            return CrearPagina(configModel, pLang);
        }

        public static Pagina CrearPagina(ConfigModel pConfigModel, string pLang)
        {
            Pagina pagina = new Pagina();
            pagina.id = pConfigModel.identificador;
            pagina.nombre = GetTextLang(pLang, pConfigModel.nombre);
            pagina.listaIdsGraficas = new List<string>();
            foreach(Grafica itemGrafica in pConfigModel.graficas)
            {
                pagina.listaIdsGraficas.Add(itemGrafica.identificador);
            }
            pagina.listaIdsFacetas = new List<string>();
            foreach (FacetaConf itemFaceta in pConfigModel.facetas)
            {
                pagina.listaIdsFacetas.Add(itemFaceta.filtro);
            }
            return pagina;
        }
        #endregion

        #region --- Gráficas
        public static GraficaBase GetGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            // Lectura del JSON de configuración.
            List<ConfigModel> listaConfigModel = null;
            using (StreamReader reader = new StreamReader($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configGraficas\configuration.json"))
            {
                string json = reader.ReadToEnd();
                listaConfigModel = JsonConvert.DeserializeObject<List<ConfigModel>>(json);
            }

            ConfigModel configModel = listaConfigModel.FirstOrDefault(x => x.identificador == pIdPagina);
            if (configModel != null)
            {
                Grafica grafica = configModel.graficas.FirstOrDefault(x => x.identificador == pIdGrafica);
                return CrearGrafica(grafica, configModel.filtro, pFiltroFacetas, pLang);
            }

            return null;
        }

        public static GraficaBase CrearGrafica(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            switch (pGrafica.tipoGrafica)
            {
                case EnumGraficas.Barras:
                    // TODO: Controlar excepciones en la configuración.
                    if (string.IsNullOrEmpty(pGrafica.configBarras.ejeX))
                    {
                        throw new Exception("No está configurada la propiedad del agrupación del eje x.");
                    }
                    if (pGrafica.configBarras.dimensiones == null || pGrafica.configBarras.dimensiones.Count() == 0)
                    {
                        throw new Exception("No se ha configurado dimensiones.");
                    }
                    return CrearGraficaBarras(pGrafica, pFiltroBase, pFiltroFacetas, pLang);
                default:
                    return null;
            }
        }

        public static GraficaBase CrearGraficaBarras(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            // Objeto a devolver.
            GraficaBase grafica = new GraficaBase();
            grafica.type = "bar"; // Por defecto, de tipo bar.

            // Asignación de Data.
            Data data = new Data();
            data.datasets = new System.Collections.Concurrent.ConcurrentBag<Dataset>();
            grafica.data = data;

            // Asignación de Options.
            Options options = new Options();

            // Título
            options.plugins = new Plugin();
            options.plugins.title = new Title();
            options.plugins.title.display = true;
            options.plugins.title.text = GetTextLang(pLang, pGrafica.nombre);

            // Ejes Y
            options.scales = new Dictionary<string, EjeY>();
            foreach (EjeYConf item in pGrafica.configBarras.yAxisPrint)
            {
                options.scales.Add(item.yAxisID, new EjeY() { position = item.posicion });
            }
            grafica.options = options;

            //foreach (Dimension itemGrafica in pGrafica.configBarras.dimensiones)
            Parallel.ForEach(pGrafica.configBarras.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = 1 }, itemGrafica =>

            {
                // Orden.
                string orden = "ASC";
                if (pGrafica.configBarras.orderDesc == true)
                {
                    orden = "DESC";
                }

                // Filtro de página.
                List<string> filtros = new List<string>();

                Dictionary<string, float> dicResultados = new Dictionary<string, float>();
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();
                filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.configBarras.ejeX }, "ejeX"));
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }));
                }
                if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }
                if (string.IsNullOrEmpty(itemGrafica.calculo))
                {
                    // Consulta sparql.                    
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append("SELECT ?ejeX COUNT(DISTINCT ?s) AS ?numero ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                }
                else
                {
                    // Cálculo (SUM|AVG|MIN|MAX)
                    string calculo = itemGrafica.calculo;

                    // Consulta sparql.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append($@"SELECT ?ejeX {calculo}(?calc) AS ?numero ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                }

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        dicResultados.Add(fila["ejeX"].value, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture));
                    }
                }

                // Rellena y ordena con los años faltantes en el caso que sea true.
                if (pGrafica.configBarras.rellenarEjeX)
                {
                    if (dicResultados.Count > 0)
                    {
                        int inicio = dicResultados.Keys.Select(x => int.Parse(x)).Min();
                        int fin = dicResultados.Keys.Select(x => int.Parse(x)).Max();
                        for (int i = inicio; i < fin; i++)
                        {
                            if (!dicResultados.ContainsKey(i.ToString()))
                            {
                                dicResultados.Add(i.ToString(), 0);
                            }
                        }
                    }
                    dicResultados = dicResultados.OrderBy(item => item.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
                }

                // Obtención del objeto de la gráfica.
                List<string> listaLabels = dicResultados.Keys.ToList();
                Dataset dataset = new Dataset();
                dataset.data = dicResultados.Values.ToList();

                // Nombre del dato en leyenda.
                dataset.label = GetTextLang(pLang, itemGrafica.nombre);

                // Color.
                dataset.backgroundColor = ObtenerColores(dataset.data.Count(), itemGrafica.color);
                dataset.type = itemGrafica.tipoDimension;

                // Anchura.
                dataset.barPercentage = 1;
                if (itemGrafica.anchura != 0)
                {
                    dataset.barPercentage = itemGrafica.anchura;
                }

                // Stack.
                if (!string.IsNullOrEmpty(itemGrafica.stack))
                {
                    dataset.stack = itemGrafica.stack;
                }
                else
                {
                    dataset.stack = Guid.NewGuid().ToString();
                }

                // Eje Y.
                dataset.yAxisID = itemGrafica.yAxisID;

                grafica.data.datasets.Add(dataset);
                data.labels = listaLabels;
                data.type = itemGrafica.tipoDimension;
            });

            return grafica;
        }
        #endregion

        #region --- Facetas
        public static Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltros, string pLang)
        {
            // Lectura del JSON de configuración.
            List<ConfigModel> listaConfigModel = null;
            using (StreamReader reader = new StreamReader($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configGraficas\configuration.json"))
            {
                string json = reader.ReadToEnd();
                listaConfigModel = JsonConvert.DeserializeObject<List<ConfigModel>>(json);
            }

            ConfigModel configModel = listaConfigModel.FirstOrDefault(x => x.identificador == pIdPagina);
            if (configModel != null)
            {
                FacetaConf faceta = configModel.facetas.FirstOrDefault(x => x.filtro == pIdFaceta);
                return CrearFaceta(faceta, configModel.filtro, pLang);
            }

            return null;
        }

        public static Faceta CrearFaceta(FacetaConf pFacetaConf, string pFiltroBase, string pLang)
        {
            Faceta faceta = new Faceta();
            faceta.nombre = GetTextLang(pLang, pFacetaConf.nombre);
            faceta.items = new List<ItemFaceta>();

            // Filtro de página.
            List<string> filtros = new List<string>();
            filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
            filtros.AddRange(ObtenerFiltros(new List<string>() { pFacetaConf.filtro }));
            Dictionary<string, float> dicResultados = new Dictionary<string, float>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append($@"SELECT DISTINCT ?title1 AS ?itemFaceta COUNT(?s) AS ?numero ");
            where.Append("WHERE { ");
            foreach (string item in filtros)
            {
                where.Append(item);
            }
            where.Append($@"FILTER(LANG(?title1) = '{pLang}' OR LANG(?title1) = '' OR !isLiteral(?title1)) ");
            where.Append($@"}} ORDER BY DESC (?numero) ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    ItemFaceta itemFaceta = new ItemFaceta();
                    itemFaceta.nombre = fila["itemFaceta"].value;
                    itemFaceta.numero = Int32.Parse(fila["numero"].value);
                    itemFaceta.filtro = $@"{pFacetaConf.filtro}='{itemFaceta.nombre}'@{pLang}";
                    faceta.items.Add(itemFaceta);
                }
            }

            return faceta;
        }
        #endregion

        #region --- Utils
        public static List<string> ObtenerColores(int pNumVeces, string pColorHex)
        {
            List<string> colores = new List<string>();
            for (int i = 0; i < pNumVeces; i++)
            {
                colores.Add(pColorHex);
            }
            return colores;
        }

        public static List<string> ObtenerFiltros(List<string> pListaFiltros, string pNombreVar = null)
        {
            // Split por filtro.
            List<string> listaAux = new List<string>();
            foreach (string filtro in pListaFiltros)
            {
                string[] array = filtro.Split("&");
                listaAux.AddRange(array.ToList());
            }

            List<string> filtrosQuery = new List<string>();

            // Split por salto de ontología.
            foreach (string item in listaAux)
            {
                if (!item.Contains("@@@"))
                {
                    if (item.Contains("="))
                    {
                        string predicado = item.Split("=")[0];
                        string objeto = item.Split("=")[1];
                        filtrosQuery.Add($@"?s {predicado} '{objeto}'. ");
                    }
                    else if (string.IsNullOrEmpty(pNombreVar))
                    {
                        filtrosQuery.Add($@"?s {item} ?calc. ");
                    }
                    else
                    {
                        filtrosQuery.Add($@"?s {item} ?{pNombreVar}. ");
                    }
                }
                else
                {
                    filtrosQuery.Add(TratarParametros(item, "?s", 0));
                }
            }

            return filtrosQuery;
        }

        public static string TratarParametros(string pFiltro, string pVarAnterior, int pAux)
        {
            StringBuilder filtro = new StringBuilder();
            foreach (string parteFiltro in pFiltro.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!parteFiltro.Contains("="))
                {
                    string varActual = $@"?{parteFiltro.Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";
                    filtro.Append($@"{pVarAnterior} ");
                    filtro.Append($@"{parteFiltro} ");
                    filtro.Append($@"{varActual}. ");
                    pVarAnterior = varActual;
                    pAux++;
                }
                else
                {
                    string varActual = $@"{parteFiltro.Split("=")[1]}";
                    filtro.Append($@"{pVarAnterior} ");
                    filtro.Append($@"{parteFiltro.Split("=")[0]} ");
                    filtro.Append($@"{varActual}. ");
                }
            }
            return filtro.ToString();
        }

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
        #endregion
    }
}
