using EditorCV.Models.API.Response;
using EditorCV.Models.API.Templates;
using EditorCV.Models.PreimportModels;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Microsoft.AspNetCore.Http;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{
    public class AccionesImportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");
        private static Dictionary<string, Dictionary<string, List<string>>> dicPropiedades = new Dictionary<string, Dictionary<string, List<string>>>();
        public byte[] filebytes;


        /// <summary>
        /// Lectura del archivo <paramref name="File"/> y creación del objeto Preimport
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pCVID"></param>
        /// <param name="File"></param>
        /// <param name="petitionID">ID de la petición</param>
        /// <returns></returns>
        public Preimport PreimportarCV(ConfigService _Configuracion, string pCVID, IFormFile File, string petitionID)
        {
            try
            {
                //Petición al exportador
                MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                multipartFormData.Add(new StringContent(pCVID), "pCVID");
                multipartFormData.Add(new StringContent(petitionID), "petitionID");

                var ms = new MemoryStream();
                File.CopyTo(ms);
                filebytes = ms.ToArray();
                multipartFormData.Add(new ByteArrayContent(filebytes), "File", File.FileName);

                string urlPreImportador = "";
                urlPreImportador = _Configuracion.GetUrlImportador() + "/Preimportar";

                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);

                HttpResponseMessage response = client.PostAsync($"{urlPreImportador}", multipartFormData).Result;
                response.EnsureSuccessStatusCode();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(response.StatusCode.ToString() + " " + response.Content);
                }
                Preimport preimport = JsonConvert.DeserializeObject<Preimport>(response.Content.ReadAsStringAsync().Result);

                return preimport;
            }
            catch (Exception ex)
            {
                mResourceApi.Log.Error(ex.Message);
                return new Preimport();
            }
        }

        /// <summary>
        /// Servicio de PostImportación del CV
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pCVID"></param>
        /// <param name="file"></param>
        /// <param name="filePreimport"></param>
        /// <param name="listaId"></param>
        /// <param name="dicOpciones"></param>
        public void PostimportarCV(ConfigService _Configuracion, string pCVID, string petitionID, ConcurrentDictionary<string, PetitionStatus> petitionStatus,
            byte[] file, string filePreimport, List<string> listaId, Dictionary<string, string> dicOpciones)
        {
            //Si la opcion es "ig"-"ignorar" elimino ese Identificador de los listados
            foreach (KeyValuePair<string, string> valuePair in dicOpciones)
            {
                if (valuePair.Value.Equals("ig") && listaId.Contains(valuePair.Key))
                {
                    listaId.Remove(valuePair.Key);
                    dicOpciones.Remove(valuePair.Key);
                }
            }

            petitionStatus[petitionID].subTotalWorks = listaId.Count;

            //Petición al exportador
            var multipartFormData = new MultipartFormDataContent();
            //Identificador del curriculumvitae
            multipartFormData.Add(new StringContent(pCVID), "pCVID");
            //Identificador de la peticion
            multipartFormData.Add(new StringContent(petitionID), "petitionID");
            //Archivo XML leido
            multipartFormData.Add(new ByteArrayContent(file), "file");
            //Objeto Preimport
            multipartFormData.Add(new StringContent(filePreimport), "filePreimport");
            //Listado de identificadores de los recursos a cargar
            if (listaId != null && listaId.Count > 0)
            {
                foreach (string id in listaId)
                {
                    multipartFormData.Add(new StringContent(id), "listaId");
                }
            }
            else
            {
                multipartFormData.Add(new StringContent(""), "listaId");
            }
            //Opciones de los recursos seleccionados
            if (dicOpciones != null && dicOpciones.Count > 0)
            {
                foreach (KeyValuePair<string, string> opcion in dicOpciones)
                {
                    multipartFormData.Add(new StringContent(opcion.Key + "|||" + opcion.Value), "listaOpciones");
                }
            }
            else
            {
                multipartFormData.Add(new StringContent(""), "listaOpciones");
            }

            string urlPreImportador = "";
            urlPreImportador = _Configuracion.GetUrlImportador() + "/Postimportar";

            //Petición al exportador para conseguir el archivo PDF
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(1, 15, 0);

            HttpResponseMessage response = client.PostAsync($"{urlPreImportador}", multipartFormData).Result;
            response.EnsureSuccessStatusCode();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.StatusCode.ToString() + " " + response.Content);
            }

        }

        /// <summary>
        /// Devuelve un diccionario con las Tabs del objeto <paramref name="preimport"/>
        /// </summary>
        /// <param name="tabTemplatesAux"></param>
        /// <param name="preimport"></param>
        /// <returns></returns>
        public ConcurrentDictionary<int, API.Response.Tab> GetListTabs(ConcurrentBag<API.Templates.Tab> tabTemplatesAux, Preimport preimport)
        {
            ConcurrentDictionary<int, API.Response.Tab> dicTabs = new ConcurrentDictionary<int, API.Response.Tab>();
            int i = 0;
            foreach (API.Templates.Tab tab in tabTemplatesAux)
            {
                dicTabs.TryAdd(i, GetTabImport(tab, preimport));
                i++;
            }

            return dicTabs;
        }

        /// <summary>
        /// Devuelve un Tab pasado un identificador de la misma y un objeto Preimport
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="preimport"></param>
        /// <returns></returns>
        private API.Response.Tab GetTabImport(API.Templates.Tab tab, Preimport preimport)
        {
            API.Response.Tab tabResponse = new API.Response.Tab();
            tabResponse.sections = new List<API.Response.TabSection>();

            if (tab.personalData)
            {
                tabResponse.title = tab.title.FirstOrDefault().Value;
                tabResponse.sections.Add(GetPersonalDataSection(tab.personalDataSections, preimport));

                return tabResponse;
            }
            else
            {
                foreach (API.Templates.TabSection section in tab.sections)
                {
                    tabResponse.title = tab.title.FirstOrDefault().Value;
                    tabResponse.sections.Add(GetSectionImport(section, preimport));
                }
            }

            return tabResponse;
        }

        /// <summary>
        /// Devuelvo un TabSection de la seccion de datos personales
        /// </summary>
        /// <param name="section"></param>
        /// <param name="preimport"></param>
        /// <returns></returns>
        private API.Response.TabSection GetPersonalDataSection(ItemEdit section, Preimport preimport)
        {
            string lang = "es";
            API.Response.TabSection tabSection = new API.Response.TabSection();
            tabSection.items = new Dictionary<string, TabSectionItem>();
            tabSection.identifier = section.proptitle;

            // Últimos 5 años
            TabSectionLast5Years last5Years = new TabSectionLast5Years();
            last5Years.always = true;
            tabSection.last5Years = last5Years;

            foreach (ItemEditSection itemEditSection in section.sections)
            {
                TabSectionItem tabSectionItem = new TabSectionItem();

                tabSectionItem.title = UtilityCV.GetTextLang(lang, itemEditSection.title);
                tabSectionItem.properties = new List<TabSectionItemProperty>();
                tabSectionItem.identifier = preimport.secciones.Where(x => x.id.Equals("000.000.000.000"))
                    .Select(x => x.subsecciones.Select(x => x.guid).FirstOrDefault()).FirstOrDefault();

                tabSectionItem.iseditable = !preimport.secciones.Where(x => x.id.Equals("000.000.000.000"))
                    .Select(x => x.subsecciones.Select(x => x.isBlocked).FirstOrDefault()).FirstOrDefault();

                if (itemEditSection.rows != null && itemEditSection.rows.Count > 0)
                {
                    foreach (ItemEditSectionRow row in itemEditSection.rows)
                    {
                        foreach (ItemEditSectionRowProperty prop in row.properties)
                        {
                            if (prop.property.Equals("http://xmlns.com/foaf/0.1/firstName") || prop.property.Equals("http://xmlns.com/foaf/0.1/familyName")
                                || prop.property.Equals("http://w3id.org/roh/secondFamilyName"))
                            {
                                TabSectionItemProperty tsip = new TabSectionItemProperty();
                                tsip.type = GetPropCompleteWithoutRelatedBy(GetPropCompleteImport(prop.property));
                                tsip.values = preimport.secciones.Where(x => x.id.Equals("000.000.000.000"))
                                    .SelectMany(x => x.subsecciones.SelectMany(x => x.propiedades.Where(x => x.prop.Equals(prop.property)).ToList()))
                                    .SelectMany(x => x.values).ToList();

                                tabSectionItem.properties.Add(tsip);
                            }
                        }
                    }
                }
                if (tabSectionItem != null && tabSectionItem.properties != null && tabSectionItem.properties.Count > 0)
                {
                    tabSection.items.Add(Guid.NewGuid().ToString(), tabSectionItem);
                }
            }
            return tabSection;
        }

        /// <summary>
        /// Devuelvo un TabSection dependiendo del <paramref name="section"/> pasado.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="preimport"></param>
        /// <returns></returns>
        private API.Response.TabSection GetSectionImport(API.Templates.TabSection section, Preimport preimport)
        {
            string lang = "es";
            API.Response.TabSection tabSection = new API.Response.TabSection();
            //Título sección
            tabSection.title = UtilityCV.GetTextLang(lang, section.presentation.title);
            tabSection.identifier = section.property;

            //Últimos 5 años
            if (section.presentation != null && section.presentation.listItemsPresentation != null && section.presentation.listItemsPresentation.last5Years != null)
            {
                TabSectionLast5Years last5Years = new TabSectionLast5Years();
                last5Years.always = section.presentation.listItemsPresentation.last5Years.always;
                last5Years.end = section.presentation.listItemsPresentation.last5Years.end;
                last5Years.start = section.presentation.listItemsPresentation.last5Years.start;

                tabSection.last5Years = last5Years;
            }

            //Órdenes e items de sección
            tabSection.orders = new List<TabSectionPresentationOrder>();
            if (section.presentation != null && section.presentation.listItemsPresentation != null &&
                section.presentation.listItemsPresentation.listItem != null && section.presentation.listItemsPresentation.listItem.orders != null)
            {
                //Órdenes sección
                foreach (TabSectionListItemOrder listItemOrder in section.presentation.listItemsPresentation.listItem.orders)
                {
                    TabSectionPresentationOrder presentationOrderTabSection = new TabSectionPresentationOrder()
                    {
                        name = UtilityCV.GetTextLang(lang, listItemOrder.name),
                        properties = new List<TabSectionPresentationOrderProperty>()
                    };

                    if (listItemOrder.properties != null)
                    {
                        foreach (TabSectionListItemOrderProperty listItemConfigOrderProperty in listItemOrder.properties)
                        {
                            TabSectionPresentationOrderProperty presentationOrderTabSectionProperty = new TabSectionPresentationOrderProperty()
                            {
                                property = UtilityCV.GetPropComplete(listItemConfigOrderProperty),
                                asc = listItemConfigOrderProperty.asc
                            };
                            presentationOrderTabSection.properties.Add(presentationOrderTabSectionProperty);
                        }
                    }
                    tabSection.orders.Add(presentationOrderTabSection);
                }

                //Items
                tabSection.items = new Dictionary<string, TabSectionItem>();
                if (section != null && section.presentation != null && section.presentation.listItemsPresentation != null &&
                    section.presentation.listItemsPresentation.listItem != null && section.presentation.listItemsPresentation.listItem.properties != null &&
                    section.presentation.listItemsPresentation.listItem.properties.Count != 0 &&
                    preimport.secciones.Exists(x => x.id.Equals(section.presentation.listItemsPresentation.cvnsection)) ||
                    string.IsNullOrEmpty(section.presentation.listItemsPresentation.cvnsection) && preimport.secciones.Exists(x => x.id.Equals("050.020.010.000")) ||
                    string.IsNullOrEmpty(section.presentation.listItemsPresentation.cvnsection) && preimport.secciones.Exists(x => x.id.Equals("050.020.020.000")) ||
                    string.IsNullOrEmpty(section.presentation.listItemsPresentation.cvnsection) && preimport.secciones.Exists(x => x.id.Equals("050.010.000.000"))
                )
                {

                    List<SubseccionItem> listaSubsecciones = new List<SubseccionItem>();
                    if (string.IsNullOrEmpty(section.presentation.listItemsPresentation.cvnsection))// && !string.IsNullOrEmpty(section.presentation.listItemsPresentation.rdftype_cv))
                    {
                        Dictionary<string, string> propFecytID = new Dictionary<string, string>();
                        propFecytID.Add("http://w3id.org/roh/RelatedCompetitiveProjectCV", "050.020.010.000");
                        propFecytID.Add("http://w3id.org/roh/RelatedNonCompetitiveProjectCV", "050.020.020.000");
                        propFecytID.Add("http://w3id.org/roh/RelatedGroupCV", "050.010.000.000");
                        propFecytID.Add("http://w3id.org/roh/RelatedScientificPublicationCV", "060.010.010.000");
                        propFecytID.Add("http://w3id.org/roh/RelatedWorkSubmittedConferencesCV", "060.010.020.000");
                        propFecytID.Add("http://w3id.org/roh/RelatedWorkSubmittedSeminarsCV", "060.010.030.000");
                        propFecytID.Add("http://purl.org/ontology/bibo/Patent", "050.030.010.000");

                        foreach (string prop in propFecytID.Keys)
                        {
                            if (section.presentation.listItemsPresentation.rdftype_cv != null &&
                            section.presentation.listItemsPresentation.rdftype_cv.Equals(prop))
                            {
                                listaSubsecciones = preimport.secciones.Where(x => x.id.Equals(propFecytID[prop])).SelectMany(x => x.subsecciones).ToList();
                                break;
                            }
                        }
                    }
                    else
                    {
                        listaSubsecciones = preimport.secciones
                            .Where(x => x.id.Equals(section.presentation.listItemsPresentation.cvnsection))
                            .SelectMany(x => x.subsecciones).ToList();
                    }


                    for (int i = 0; i < listaSubsecciones.Count; i++)
                    {
                        tabSection.items.Add(listaSubsecciones[i].guid, GetItemImport(section.presentation.listItemsPresentation.listItem, listaSubsecciones[i]));
                    }
                }
            }

            if (section.presentation != null && section.presentation.itemPresentation != null && section.presentation.itemPresentation.itemEdit != null)
            {
                tabSection = new API.Response.TabSection();
                tabSection.items = new Dictionary<string, TabSectionItem>();
                tabSection.title = section.presentation.title.First().Value;

                foreach (ItemEditSectionRow itemEditSectionRow in section.presentation.itemPresentation.itemEdit.sections.First().rows)
                {
                    foreach (ItemEditSectionRowProperty itemEditSection in itemEditSectionRow.properties)
                    {
                        TabSectionItem tabSectionItem = new TabSectionItem();

                        tabSectionItem.title = UtilityCV.GetTextLang(lang, itemEditSection.title);
                        tabSectionItem.properties = new List<TabSectionItemProperty>();
                        tabSectionItem.orderProperties = new List<TabSectionItemOrderProperty>();
                        tabSectionItem.iseditable = !itemEditSection.blocked;

                        TabSectionItemProperty tsip = new TabSectionItemProperty();
                        tsip.type = GetPropCompleteWithoutRelatedBy(GetPropCompleteImport(itemEditSection.property));
                        tsip.values = preimport.secciones.Where(x => x.id.Equals("070.010.000.000") || x.id.Equals("060.010.060.010"))
                                        .SelectMany(x => x.subsecciones.SelectMany(x => x.propiedades.Where(x => x.prop.Equals(itemEditSection.property)).ToList()))
                                        .SelectMany(x => x.values).ToList();

                        tabSectionItem.identifier = preimport.secciones.Where(x => x.id.Equals("070.010.000.000") || x.id.Equals("060.010.060.010"))?
                            .Select(w => w.subsecciones.Where(q => q.propiedades.Count != 0 && q.propiedades.Any(x => x.prop.Equals(itemEditSection.property))))?
                            .Where(x => x.Any()).FirstOrDefault()?.Select(x => x.guid).FirstOrDefault();
                        tabSectionItem.idBBDD = preimport.secciones.Where(x => x.id.Equals("070.010.000.000") || x.id.Equals("060.010.060.010"))?
                            .Select(w => w.subsecciones.Where(q => q.propiedades.Count != 0 && q.propiedades.Any(x => x.prop.Equals(itemEditSection.property))))?
                            .Where(p => p.Any()).FirstOrDefault()?.Select(x => x.idBBDD).FirstOrDefault();

                        tabSectionItem.properties.Add(tsip);

                        if (tabSectionItem != null && tabSectionItem.properties != null && tabSectionItem.properties.Count > 0)
                        {
                            tabSection.items.Add(Guid.NewGuid().ToString(), tabSectionItem);
                        }
                    }
                }
            }

            return tabSection;
        }

        /// <summary>
        /// Devuelve un objeto TabSectionItem de <paramref name="tabSectionListItem"/>
        /// </summary>
        /// <param name="tabSectionListItem"></param>
        /// <param name="subseccionItem"></param>
        /// <returns></returns>
        private TabSectionItem GetItemImport(TabSectionListItem tabSectionListItem, SubseccionItem subseccionItem)
        {
            string lang = "es";

            string si = "";
            string no = "";
            switch (lang)
            {
                case "es":
                    si = "Sí";
                    no = "No";
                    break;
                case "en":
                    si = "Yes";
                    no = "No";
                    break;
                default:
                    si = "Sí";
                    no = "No";
                    break;
            }

            TabSectionItem sectionItem = new TabSectionItem();
            List<string> valor = new List<string>();
            string graph = "";

            //Título
            PropertyDataTemplate configTitulo = tabSectionListItem.propertyTitle;

            //Titulo Tutorias academicas
            if (subseccionItem.propiedades.Where(x => x.prop.Equals("http://w3id.org/roh/cvnCode")).Select(x => x.values).Any() &&
               subseccionItem.propiedades.Where(x => x.prop.Equals("http://w3id.org/roh/cvnCode")).Select(x => x.values).First().First().Equals("030.050.000.000"))
            {
                configTitulo.property = "http://w3id.org/roh/frequency";
            }

            string propCompleteTitle = UtilityCV.GetPropComplete(configTitulo);
            sectionItem.title = subseccionItem.propiedades.FirstOrDefault(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propCompleteTitle))?.values.FirstOrDefault();
            if (sectionItem.title == null)
            {
                valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propCompleteTitle))?
                            .Select(x => x.values.First().Split("@@@").Last()).ToList();
                if (valor == null || valor.Count == 0)
                {
                    valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propCompleteTitle).Split("@@@").First())?
                        .Select(x => x.values.First().Split("@@@").Last()).ToList();
                }
                if (tabSectionListItem.propertyTitle.child != null)
                {
                    graph = tabSectionListItem.propertyTitle.child.graph;

                    string select = "select distinct ?w";
                    string where = $@"where{{
    <{valor.First()}> <{propCompleteTitle.Split("@@@").Last()}> ?w .
    FILTER( lang(?w) = '{lang}' OR lang(?w) = '')
}}";
                    SparqlObject sparqlObjectTitle = mResourceApi.VirtuosoQuery(select, where, graph);

                    if (sparqlObjectTitle.results.bindings.Count != 0)
                    {
                        foreach (Dictionary<string, Data> fila in sparqlObjectTitle.results.bindings)
                        {
                            sectionItem.title = fila["w"].value;
                        }
                    }
                }
            }
            sectionItem.properties = new List<TabSectionItemProperty>();
            sectionItem.iseditable = !subseccionItem.isBlocked;
            sectionItem.idBBDD = subseccionItem.idBBDD;
            sectionItem.identifier = subseccionItem.guid;
            sectionItem.orderProperties = new List<TabSectionItemOrderProperty>();


            if (tabSectionListItem.orders != null)
            {
                foreach (TabSectionListItemOrder order in tabSectionListItem.orders)
                {
                    if (order.properties != null)
                    {
                        foreach (TabSectionListItemOrderProperty data in order.properties)
                        {

                            TabSectionItemOrderProperty itemOrderProperty = new TabSectionItemOrderProperty()
                            {
                                //Si es 
                                property = UtilityCV.GetPropComplete(data),
                                values = subseccionItem.propiedades.Where(x => x.prop.StartsWith(GetPropCompleteWithoutRelatedBy(UtilityCV.GetPropComplete(data))))?
                                    .Select(x => x.values.First().Split("@@@").Last()).ToList()
                            };
                            if (sectionItem.orderProperties.Any(x => x.property.Contains(itemOrderProperty.property)))
                            {
                                int indice = sectionItem.orderProperties.FindIndex(x => x.property.Equals(itemOrderProperty.property));
                                sectionItem.orderProperties[indice].values.AddRange(itemOrderProperty.values);
                            }
                            else
                            {
                                sectionItem.orderProperties.Add(itemOrderProperty);
                            }
                        }
                    }
                }
            }

            if (tabSectionListItem.properties != null && tabSectionListItem.properties.Count > 0)
            {
                foreach (TabSectionListItemProperty property in tabSectionListItem.properties)
                {
                    string propComplete = "";
                    valor = new List<string>();
                    graph = "";
                    PropertyDataTemplate childOR = new PropertyDataTemplate();

                    if (property.childOR != null && property.childOR.Count != 0)
                    {
                        foreach (PropertyDataTemplate propertyDataTemplate in property.childOR)
                        {
                            propComplete = UtilityCV.GetPropComplete(propertyDataTemplate.child);
                            valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propComplete))?
                                .Select(x => x.values.First().Split("@@@").Last()).ToList();
                            if (valor == null || valor.Count == 0)
                            {
                                valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propComplete).Split("@@@").First())?
                                    .Select(x => x.values.First().Split("@@@").Last()).ToList();
                            }

                            int lengthProp = propComplete.Split("@@@").Length;
                            PropertyDataTemplate aux = propertyDataTemplate.child;
                            for (int i = 0; i < lengthProp - 1; i++)
                            {
                                graph = aux.graph;
                                aux = aux.child;
                            }
                            if (valor != null && valor.Count != 0 && valor.All(x => !string.IsNullOrEmpty(x)))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        propComplete = UtilityCV.GetPropComplete(property.child);

                        valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propComplete))?
                            .Select(x => x.values.First().Split("@@@").Last()).ToList();
                        if (valor == null || valor.Count == 0)
                        {
                            valor = subseccionItem.propiedades.Where(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propComplete).Split("@@@").First())?
                                .Select(x => x.values.First().Split("@@@").Last()).ToList();
                        }

                        int lengthProp = propComplete.Split("@@@").Length;
                        PropertyDataTemplate aux = property.child;
                        for (int i = 0; i < lengthProp - 2; i++)
                        {
                            aux = aux.child;
                            graph = aux.graph;
                        }

                    }


                    if (valor != null && valor.Count != 0 && valor.All(x => !string.IsNullOrEmpty(x)))
                    {
                        string value = valor.First().Split("@@@").Last();
                        if (value.StartsWith("http://gnoss.com/items/") && value.Split("_").Length > 1 && !string.IsNullOrEmpty(graph))
                        {
                            string prop = GetPropCompleteWithoutRelatedBy(propComplete).Split("@@@").Last();

                            if (!dicPropiedades.ContainsKey(value))
                            {
                                string select = "select distinct ?w";
                                string where = $@"where{{
    <{value}> <{prop}> ?w .
    FILTER( lang(?w) = '{lang}' OR lang(?w) = '')
}}";

                                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, graph);
                                if (!dicPropiedades.ContainsKey(value) && sparqlObject.results.bindings.Count != 0)
                                {
                                    dicPropiedades.Add(value, new Dictionary<string, List<string>>());
                                    if (!dicPropiedades[value].ContainsKey(lang))
                                    {
                                        dicPropiedades[value].Add(lang, new List<string>());
                                    }
                                    else
                                    {
                                        dicPropiedades[value][lang].Add("");
                                    }
                                }
                                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                                {
                                    dicPropiedades[value][lang].Add(fila["w"].value);
                                }
                            }

                        }

                        TabSectionItemProperty tsip = new TabSectionItemProperty();
                        tsip.name = UtilityCV.GetTextLang(lang, property.name);
                        tsip.values = new List<string>();

                        if (GetPropCompleteWithoutRelatedBy(propComplete).Split("@@@").Length != 1 &&
                            dicPropiedades.ContainsKey(value) && dicPropiedades[value].ContainsKey(lang))
                        {
                            tsip.values.Add(dicPropiedades[value][lang].First());
                        }
                        else
                        {

                            if (value.Equals("true"))
                            {
                                tsip.values.Add(si);
                            }
                            else if (value.Equals("false"))
                            {
                                tsip.values.Add(no);
                            }
                            else
                            {
                                tsip.values.AddRange(valor.Select(x => x.Split("@@@").Last()).ToList());
                            }
                        }
                        tsip.showMini = property.showMini;
                        tsip.showMiniBold = property.showMiniBold;
                        tsip.type = property.type.ToString();

                        sectionItem.properties.Add(tsip);
                    }
                }
            }

            return sectionItem;
        }

        /// <summary>
        /// Concatena mediante "@@@" las cadenas que forman parte de <paramref name="pPropImport"/> tras separar por "|"
        /// y elegir la primerda de separar posteriormente por "@@@".
        /// "Ejemplo1@@@Ejemplo2|Ejemplo3" -> "Ejemplo1@@@Ejemplo3"
        /// </summary>
        /// <param name="pPropImport"></param>
        /// <returns></returns>
        private string GetPropCompleteImport(string pPropImport)
        {
            if (string.IsNullOrEmpty(pPropImport))
            {
                return "";
            }
            return string.Join("@@@", pPropImport.Split("|").Select(x => x.Split("@@@").FirstOrDefault()));
        }

        /// <summary>
        /// Elimina de la cadena "http://vivoweb.org/ontology/core#relatedBy@@@"
        /// </summary>
        /// <param name="pPropCompelte"></param>
        /// <returns></returns>
        private string GetPropCompleteWithoutRelatedBy(string pPropCompelte)
        {
            return pPropCompelte.Replace("http://vivoweb.org/ontology/core#relatedBy@@@", "");
        }

    }
}
