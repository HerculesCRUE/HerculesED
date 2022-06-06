using EditorCV.Models.API.Response;
using EditorCV.Models.API.Templates;
using EditorCV.Models.PreimportModels;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesImportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public Preimport PreimportarCV(ConfigService _Configuracion, string pCVID, IFormFile File)
        {
            try
            {
                //Petición al exportador
                MultipartFormDataContent multipartFormData = new MultipartFormDataContent();
                multipartFormData.Add(new StringContent(pCVID), "pCVID");

                var ms = new MemoryStream();
                File.CopyTo(ms);
                byte[] filebytes = ms.ToArray();
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

        public ConcurrentDictionary<int, API.Response.Tab> GetListTabs(ConcurrentBag<API.Templates.Tab> tabTemplatesAux, Preimport preimport)
        {
            ConcurrentDictionary<int, API.Response.Tab> dicTabs = new ConcurrentDictionary<int, API.Response.Tab>();
            int i = 0;
            foreach (API.Templates.Tab tab in tabTemplatesAux)
            {
                dicTabs.TryAdd(i, GetTabImport(tab, preimport));
                i++;
            }

            //recorrerse ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;
            //metodo GetTabImport(Preimport)
            //metodo GetSectionImport - llamo X veces desde ^
            //metodo GetItemImport - llamo X veces desde ^

            return dicTabs;
        }


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

        private API.Response.TabSection GetPersonalDataSection(ItemEdit section, Preimport preimport)
        {
            string lang = "es";
            API.Response.TabSection tabSection = new API.Response.TabSection();
            tabSection.items = new Dictionary<string, TabSectionItem>();
            foreach (ItemEditSection itemEditSection in section.sections)
            {
                TabSectionItem tabSectionItem = new TabSectionItem();

                tabSectionItem.title = UtilityCV.GetTextLang(lang, itemEditSection.title);
                tabSectionItem.properties = new List<TabSectionItemProperty>();

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
                                tsip.values = prop.title.Select(x => x.Value).ToList();

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

        private API.Response.TabSection GetSectionImport(API.Templates.TabSection section, Preimport preimport)
        {
            string lang = "es";
            API.Response.TabSection tabSection = new API.Response.TabSection();
            //Título sección
            tabSection.title = UtilityCV.GetTextLang(lang, section.presentation.title);


            //Órdenes sección
            tabSection.orders = new List<TabSectionPresentationOrder>();
            if (section.presentation != null && section.presentation.listItemsPresentation != null &&
                section.presentation.listItemsPresentation.listItem != null && section.presentation.listItemsPresentation.listItem.orders != null)
            {
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
            }

            //Items
            tabSection.items = new Dictionary<string, TabSectionItem>();
            if (section != null && section.presentation != null && section.presentation.listItemsPresentation != null &&
                section.presentation.listItemsPresentation.listItem != null && section.presentation.listItemsPresentation.listItem.properties != null &&
                section.presentation.listItemsPresentation.listItem.properties.Count != 0 &&
                preimport.secciones.Exists(x => x.id.Equals(section.presentation.listItemsPresentation.cvnsection)))
            {

                List<SubseccionItem> listaSubsecciones = preimport.secciones
                    .Where(x => x.id.Equals(section.presentation.listItemsPresentation.cvnsection))
                    .SelectMany(x => x.subsecciones).ToList();

                List<EntityPreimport.Property> listaPropiedades = listaSubsecciones.SelectMany(x => x.propiedades).ToList();

                for (int i = 0; i < listaSubsecciones.Count; i++)
                {
                    tabSection.items.Add(Guid.NewGuid().ToString(), GetItemImport(section.presentation.listItemsPresentation.listItem, listaSubsecciones[i]));
                }
            }

            return tabSection;
        }

        private TabSectionItem GetItemImport(TabSectionListItem tabSectionListItem, SubseccionItem subseccionItem)
        {
            string lang = "es";
            TabSectionItem sectionItem = new TabSectionItem();
            //Título
            PropertyDataTemplate configTitulo = tabSectionListItem.propertyTitle;

            string propCompleteTitle = UtilityCV.GetPropComplete(configTitulo);
            sectionItem.title = subseccionItem.propiedades.FirstOrDefault(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propCompleteTitle))?.values.FirstOrDefault();
            sectionItem.properties = new List<TabSectionItemProperty>();
            //propcomplete http://vivoweb.org/ontology/core#relatedBy@@@http://w3id.org/roh/professionalCategory
            //Prop subseccionItem "http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode"

            //TODO title or
            //sectionItem.title = property.values.First();

            if (tabSectionListItem.properties != null && tabSectionListItem.properties.Count > 0)
            {
                foreach (TabSectionListItemProperty property in tabSectionListItem.properties)
                {
                    string propComplete = UtilityCV.GetPropComplete(property.child);
                    string valor = subseccionItem.propiedades.FirstOrDefault(x => GetPropCompleteImport(x.prop) == GetPropCompleteWithoutRelatedBy(propComplete))?.values.FirstOrDefault();

                    if (!string.IsNullOrEmpty(valor))
                    {
                        TabSectionItemProperty tsip = new TabSectionItemProperty();
                        tsip.name = UtilityCV.GetTextLang(lang, property.name);
                        tsip.values = new List<string>();
                        tsip.values.Add(valor);
                        tsip.showMini= property.showMini;
                        tsip.showMiniBold= property.showMiniBold;

                        sectionItem.properties.Add(tsip);
                    }

                    //tsip.type = GetPropCompleteWithoutRelatedBy(GetPropCompleteImport(property.prop));
                    //tsip.values = property.values;
                    //sectionItem.properties.Add(tsip);
                }
            }


            if (subseccionItem.propiedades != null && subseccionItem.propiedades.Count > 0)
            {
                foreach (EntityPreimport.Property property in subseccionItem.propiedades)
                {
                    TabSectionItemProperty tsip = new TabSectionItemProperty();

                    tsip.type = GetPropCompleteWithoutRelatedBy(GetPropCompleteImport(property.prop));
                    tsip.values = property.values;
                    sectionItem.properties.Add(tsip);
                }
            }

            return sectionItem;
        }

        //inout http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode
        //output http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/categoryNode
        private string GetPropCompleteImport(string pPropImport)
        {
            if (string.IsNullOrEmpty(pPropImport))
            {
                return "";
            }
            return string.Join("@@@", pPropImport.Split("|").Select(x => x.Split("@@@").FirstOrDefault()));
        }

        private string GetPropCompleteWithoutRelatedBy(string pPropCompelte)
        {
            return pPropCompelte.Replace("http://vivoweb.org/ontology/core#relatedBy@@@", "");
        }

    }
}
