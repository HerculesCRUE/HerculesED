using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using Hercules.ED.ImportExportCV.Models.FuentesExternas;
using ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Utils;
using static Models.Entity;

namespace ImportadorWebCV.Sincro.Secciones
{
    class ActividadCientificaTecnologica : SeccionBase
    {
        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private List<CvnItemBean> listadoSituacionProfesional = new List<CvnItemBean>();
        private List<CvnItemBean> listadoCvn = new List<CvnItemBean>();
        private readonly string RdfTypeTab = "http://w3id.org/roh/ScientificActivity";
        private Person personaCV = new Person();

        public ActividadCientificaTecnologica(cvnRootResultBean cvn, string cvID, string personID, ConfigService configuracion) : base(cvn, cvID, personID, configuracion)
        {
            listadoDatos = mCvn.GetListadoBloque("060");
            listadoSituacionProfesional = mCvn.GetListadoBloque("010");
            listadoCvn = mCvn.cvnRootBean.ToList();
            personaCV = Utility.GetNombrePersonaCV(cvID);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al
        /// bloque "Producción cientifica".
        /// Con el codigo identificativo 060.010.000.000
        /// </summary>
        public List<SubseccionItem> SincroProduccionCientifica(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/scientificProduction", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "scientificproduction";
            string propTitle = "http://w3id.org/roh/h-index";
            string rdfType = "http://w3id.org/roh/ScientificProduction";
            string rdfTypePrefix = "RelatedScientificProduction";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetProduccionCientifica(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ProduccionCientifica produccionCientifica = new ProduccionCientifica();
                    produccionCientifica.fuenteH = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceH)?.values.FirstOrDefault();
                    produccionCientifica.fuenteHOtros = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceHOtros)?.values.FirstOrDefault();
                    produccionCientifica.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(produccionCientifica.ID, produccionCientifica);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ProduccionCientifica.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al código de campo
        /// "Indicadores generales de calidad de la producción científica".
        /// Con el codigo identificativo 060.010.060.010
        /// </summary>
        public List<SubseccionItem> SincroIndicadoresGenerales(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            //Actualizo el estado de los recursos tratados
            if (petitionStatus != null)
            {
                petitionStatus.actualWork++;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/generalQualityIndicators", "http://w3id.org/roh/generalQualityIndicatorCV" };
            List<string> rdfTypeItem = new List<string>() { "http://w3id.org/roh/GeneralQualityIndicator", "http://w3id.org/roh/GeneralQualityIndicatorCV" };

            //1º Obtenemos la entidad de BBDD.
            Tuple<string, string, string> identificadores = GetIdentificadoresItemPresentation(mCvID, propiedadesItem, rdfTypeItem);

            //2º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;
            GetEntidadesSecundarias(ref entityBBDD, identificadores, rdfTypeItem, "curriculumvitae");

            //3º Obtenemos la entidad de los datos del XML.
            Entity entityXML = GetIndicadoresGenerales(listadoDatos);

            if (preimportar)
            {
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                if (entityBBDD == null)
                {
                    listaAux.Add(new SubseccionItem(0, null, entityXML.properties));
                }
                else
                {
                    listaAux.Add(new SubseccionItem(0, entityBBDD.id, entityXML.properties));
                }
                return listaAux;
            }
            else
            {
                if (listadoIdBBDD != null && listadoIdBBDD.Count > 0 && listadoIdBBDD.ElementAt(0).StartsWith("http://gnoss.com/items/GeneralQualityIndicatorCV_"))
                {
                    //4º Actualizamos la entidad
                    UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propiedadesItem, new List<string>() { identificadores.Item1, identificadores.Item2, identificadores.Item3 }, entityBBDD, entityXML);
                    listadoIdBBDD.RemoveAt(0);
                }
                else
                {
                    //4º Actualizamos la entidad
                    UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propiedadesItem, new List<string>() { identificadores.Item1, identificadores.Item2, identificadores.Item3 }, entityBBDD, entityXML);
                }

                return null;
            }
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Publicaciones, documentos científicos y técnicos".
        /// Con el codigo identificativo 060.010.010.000
        /// </summary>
        public List<SubseccionItem> SincroPublicacionesDocumentos(ConfigService mConfiguracion, bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus, [Optional] List<string> listaDOI)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/scientificPublications", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedScientificPublication";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPublicacionesDocumentos(mConfiguracion, listadoDatos, listadoSituacionProfesional, petitionStatus, listaDOI,preimportar);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos();
                publicacionesDocumentos.title = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo)?.values.FirstOrDefault();
                publicacionesDocumentos.autores = new HashSet<string>(entityXML.autores.Select(x => x.ID));
                publicacionesDocumentos.ID = entityXML.id;
                entidadesXML.Add(publicacionesDocumentos.ID, publicacionesDocumentos);
            }

            foreach (string idPublicacion in entidadesXML.Keys)
            {
                entidadesXML[idPublicacion].distincts = new HashSet<string>(entidadesXML.Keys.Except(new List<string> { idPublicacion }));
            }

            string personaCV = Utility.PersonaCV(mCvID);
            List<string> listado = new List<string>() { personaCV };

            //Obtenemos Organización, Departamento, Grupos y Publicaciones del propietario del CV.
            Utility.DatosDepartamentoPersona(listado).TryGetValue(personaCV, out HashSet<string> departamentos);
            Utility.DatosOrganizacionPersona(listado).TryGetValue(personaCV, out HashSet<string> organizaciones);
            Utility.DatosGrupoPersona(listado).TryGetValue(personaCV, out HashSet<string> grupos);
            Utility.DatosProyectoPersona(listado).TryGetValue(personaCV, out HashSet<string> proyectos);

            //Añado los autores del documento para la desambiguación
            for (int i = 0; i < listadoAux.Count; i++)
            {
                foreach (Persona persona in listadoAux[i].autores)
                {
                    if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                    {
                        continue;
                    }
                    persona.departamento = departamentos;
                    persona.organizacion = organizaciones;
                    persona.grupos = grupos;
                    persona.proyectos = proyectos;
                    persona.coautores = new HashSet<string>(listadoAux[i].autores.Select(x => x.ID).Where(x => x != persona.ID));
                    persona.documentos = new HashSet<string>() { listadoAux[i].id };
                    entidadesXML[persona.ID] = persona;
                }
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PublicacionesDocumentos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem, listadoAux);
            List<string> idValuesBBDD = entidadesBBDD.Values.Select(x => x.ID).ToList();

            //3º Comparamos las equivalentes
            Disambiguation.mResourceApi = mResourceApi;
            Dictionary<string, HashSet<string>> equivalencias = Disambiguation.Disambiguate(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            if (preimportar)
            {
                Dictionary<string, bool> bloqueados = ComprobarBloqueados(idValuesBBDD, graph);
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                for (int i = 0; i < listadoAux.Count; i++)
                {
                    KeyValuePair<string, HashSet<string>> x = equivalencias.FirstOrDefault(x => x.Value.Select(x => x.Split('|')[1]).Contains(listadoAux[i].id));
                    //Si NO es un Guid añado el valor.
                    string idBBDD = !Guid.TryParse(x.Key, out Guid aux) ? x.Key : "";

                    if (bloqueados.ContainsKey(idBBDD))
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv, bloqueados[idBBDD]));
                    }
                    else
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv, isBlocked: false));
                    }
                }
                return listaAux;
            }
            else
            {
                //4º Añadimos o modificamos las entidades
                AniadirModificarPublicaciones(listadoAux, equivalencias, propTitle, graph, rdfType, rdfTypePrefix,
                    propiedadesItem, RdfTypeTab, "http://w3id.org/roh/relatedScientificPublicationCV", "http://w3id.org/roh/RelatedScientificPublicationCV", listadoIdBBDD: listadoIdBBDD);
                return null;
            }
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Trabajos presentados en congresos nacionales o internacionales".
        /// Con el codigo identificativo 060.010.020.000
        /// </summary>
        public List<SubseccionItem> SincroTrabajosCongresos(ConfigService mConfiguracion, bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus, [Optional] List<string> listaDOI)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/worksSubmittedConferences", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedWorkSubmittedConferences";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTrabajosCongresos(mConfiguracion, listadoDatos, petitionStatus, listaDOI,preimportar);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                TrabajosCongresos trabajosCongresos = new TrabajosCongresos();
                trabajosCongresos.titulo = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo)?.values.FirstOrDefault();
                trabajosCongresos.autores = new HashSet<string>(entityXML.autores.Select(x => x.ID));
                trabajosCongresos.ID = entityXML.id;
                entidadesXML.Add(trabajosCongresos.ID, trabajosCongresos);
            }

            foreach (string idPublicacion in entidadesXML.Keys)
            {
                entidadesXML[idPublicacion].distincts = new HashSet<string>(entidadesXML.Keys.Except(new List<string> { idPublicacion }));
            }

            string personaCV = Utility.PersonaCV(mCvID);
            List<string> listado = new List<string>() { personaCV };

            //Obtenemos Organización, Departamento, Grupos y Publicaciones del propietario del CV.
            Utility.DatosDepartamentoPersona(listado).TryGetValue(personaCV, out HashSet<string> departamentos);
            Utility.DatosOrganizacionPersona(listado).TryGetValue(personaCV, out HashSet<string> organizaciones);
            Utility.DatosGrupoPersona(listado).TryGetValue(personaCV, out HashSet<string> grupos);
            Utility.DatosProyectoPersona(listado).TryGetValue(personaCV, out HashSet<string> proyectos);

            //Añado los autores del documento para la desambiguación
            for (int i = 0; i < listadoAux.Count; i++)
            {
                foreach (Persona persona in listadoAux[i].autores)
                {
                    if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                    {
                        continue;
                    }
                    persona.departamento = departamentos;
                    persona.organizacion = organizaciones;
                    persona.grupos = grupos;
                    persona.proyectos = proyectos;
                    persona.coautores = new HashSet<string>(listadoAux[i].autores.Select(x => x.ID).Where(x => x != persona.ID));
                    persona.documentos = new HashSet<string>() { listadoAux[i].id };
                    entidadesXML[persona.ID] = persona;
                }
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = TrabajosCongresos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem, listadoAux);
            List<string> idValuesBBDD = entidadesBBDD.Values.Select(x => x.ID).ToList();

            //3º Comparamos las equivalentes
            Disambiguation.mResourceApi = mResourceApi;
            Dictionary<string, HashSet<string>> equivalencias = Disambiguation.Disambiguate(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            if (preimportar)
            {
                Dictionary<string, bool> bloqueados = ComprobarBloqueados(idValuesBBDD, graph);
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                for (int i = 0; i < listadoAux.Count; i++)
                {
                    KeyValuePair<string, HashSet<string>> x = equivalencias.FirstOrDefault(x => x.Value.Select(x => x.Split('|')[1]).Contains(listadoAux[i].id));
                    //Si NO es un Guid añado el valor.
                    string idBBDD = !Guid.TryParse(x.Key, out Guid aux) ? x.Key : "";

                    if (bloqueados.ContainsKey(idBBDD))
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv, bloqueados[idBBDD]));
                    }
                    else
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv));
                    }
                }
                return listaAux;
            }
            else
            {
                //4º Añadimos o modificamos las entidades
                AniadirModificarPublicaciones(listadoAux, equivalencias, propTitle, graph, rdfType, rdfTypePrefix,
                    propiedadesItem, RdfTypeTab, "http://w3id.org/roh/relatedWorkSubmittedConferencesCV", "http://w3id.org/roh/RelatedWorkSubmittedConferencesCV", listadoIdBBDD: listadoIdBBDD);
                return null;
            }
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Trabajos presentados en jornadas, seminarios,
        /// talleres de trabajo y/o cursos nacionales o internacionales".
        /// Con el codigo identificativo 060.010.030.000
        /// </summary>
        public List<SubseccionItem> SincroTrabajosJornadasSeminarios(ConfigService mConfiguracion, bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/worksSubmittedSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedWorkSubmittedSeminars";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTrabajosJornadasSeminarios(mConfiguracion, listadoDatos, petitionStatus,preimportar);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                TrabajosJornadasSeminarios trabajosJornadas = new TrabajosJornadasSeminarios();
                trabajosJornadas.titulo = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo)?.values.FirstOrDefault();
                trabajosJornadas.autores = new HashSet<string>(entityXML.autores.Select(x => x.ID));
                trabajosJornadas.ID = entityXML.id;
                entidadesXML.Add(trabajosJornadas.ID, trabajosJornadas);
            }

            foreach (string idPublicacion in entidadesXML.Keys)
            {
                entidadesXML[idPublicacion].distincts = new HashSet<string>(entidadesXML.Keys.Except(new List<string> { idPublicacion }));
            }

            string personaCV = Utility.PersonaCV(mCvID);
            List<string> listado = new List<string>() { personaCV };

            //Obtenemos Organización, Departamento, Grupos y Publicaciones del propietario del CV.
            Utility.DatosDepartamentoPersona(listado).TryGetValue(personaCV, out HashSet<string> departamentos);
            Utility.DatosOrganizacionPersona(listado).TryGetValue(personaCV, out HashSet<string> organizaciones);
            Utility.DatosGrupoPersona(listado).TryGetValue(personaCV, out HashSet<string> grupos);
            Utility.DatosProyectoPersona(listado).TryGetValue(personaCV, out HashSet<string> proyectos);

            //Añado los autores del documento para la desambiguación
            for (int i = 0; i < listadoAux.Count; i++)
            {
                foreach (Persona persona in listadoAux[i].autores)
                {
                    if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                    {
                        continue;
                    }
                    persona.departamento = departamentos;
                    persona.organizacion = organizaciones;
                    persona.grupos = grupos;
                    persona.proyectos = proyectos;
                    persona.coautores = new HashSet<string>(listadoAux[i].autores.Select(x => x.ID).Where(x => x != persona.ID));
                    persona.documentos = new HashSet<string>() { listadoAux[i].id };
                    entidadesXML[persona.ID] = persona;
                }
            }


            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = TrabajosJornadasSeminarios.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem, listadoAux);
            List<string> idValuesBBDD = entidadesBBDD.Values.Select(x => x.ID).ToList();

            //3º Comparamos las equivalentes
            Disambiguation.mResourceApi = mResourceApi;
            Dictionary<string, HashSet<string>> equivalencias = Disambiguation.Disambiguate(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            if (preimportar)
            {
                Dictionary<string, bool> bloqueados = ComprobarBloqueados(idValuesBBDD, graph);
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                for (int i = 0; i < listadoAux.Count; i++)
                {
                    KeyValuePair<string, HashSet<string>> x = equivalencias.FirstOrDefault(x => x.Value.Select(x => x.Split('|')[1]).Contains(listadoAux[i].id));
                    //Si NO es un Guid añado el valor.
                    string idBBDD = !Guid.TryParse(x.Key, out Guid aux) ? x.Key : "";

                    if (bloqueados.ContainsKey(idBBDD))
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv, bloqueados[idBBDD]));
                    }
                    else
                    {
                        listaAux.Add(new SubseccionItem(i, idBBDD, listadoAux.ElementAt(i).properties, listadoAux.ElementAt(i).properties_cv));
                    }
                }
                return listaAux;
            }
            else
            {
                //4º Añadimos o modificamos las entidades
                AniadirModificarPublicaciones(listadoAux, equivalencias, propTitle, graph, rdfType, rdfTypePrefix,
                    propiedadesItem, RdfTypeTab, "http://w3id.org/roh/relatedWorkSubmittedSeminarsCV", "http://w3id.org/roh/RelatedWorkSubmittedSeminarsCV", listadoIdBBDD: listadoIdBBDD);
                return null;
            }
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otras actividades de divulgación".
        /// Con el codigo identificativo 060.010.040.000
        /// </summary>
        public List<SubseccionItem> SincroOtrasActividadesDivulgacion(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDisseminationActivities", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedOtherDisseminationActivity";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasActividadesDivulgacion(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion();
                    otrasActividadesDivulgacion.tituloTrabajo = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulTitulo)?.values.FirstOrDefault();
                    otrasActividadesDivulgacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha)?.values.FirstOrDefault();
                    otrasActividadesDivulgacion.nombreEvento = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulNombreEvento)?.values.FirstOrDefault();
                    otrasActividadesDivulgacion.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otrasActividadesDivulgacion.ID, otrasActividadesDivulgacion);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasActividadesDivulgacion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Comités científicos, técnicos y/o asesores".
        /// Con el codigo identificativo 060.020.010.000
        /// </summary>
        public List<SubseccionItem> SincroComitesCTA(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/committees", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "committee";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Committee";
            string rdfTypePrefix = "RelatedCommittee";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetComitesCTA(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ComitesCTA comitesCTA = new ComitesCTA();
                    comitesCTA.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTATitulo)?.values.FirstOrDefault();
                    comitesCTA.entidadAfiliacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTAEntidadAfiliacionNombre)?.values.FirstOrDefault();
                    comitesCTA.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTAFechaInicio)?.values.FirstOrDefault();
                    comitesCTA.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(comitesCTA.ID, comitesCTA);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ComitesCTA.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Organización de actividades de I+D+i".
        /// Con el codigo identificativo 060.020.030.000
        /// </summary>
        public List<SubseccionItem> SincroOrganizacionIDI(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesOrganization", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedActivityOrganization";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOrganizacionIDI(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI();
                    organizacionesIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDITituloActividad)?.values.FirstOrDefault();
                    organizacionesIDI.tipoActividad = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDITipoActividad)?.values.FirstOrDefault();
                    organizacionesIDI.entidadConvocante = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre)?.values.FirstOrDefault();
                    organizacionesIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDIFechaInicio)?.values.FirstOrDefault();
                    organizacionesIDI.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(organizacionesIDI.ID, organizacionesIDI);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OrganizacionesIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Gestión de I+D+i".
        /// Con el codigo identificativo 060.020.040.000
        /// </summary>
        public List<SubseccionItem> SincroGestionIDI(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesManagement", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedActivityManagement";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetGestionIDI(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    GestionIDI gestionIDI = new GestionIDI();
                    gestionIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDINombreActividad)?.values.FirstOrDefault();
                    gestionIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre)?.values.FirstOrDefault();
                    gestionIDI.funciones = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIFunciones)?.values.FirstOrDefault();
                    gestionIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio)?.values.FirstOrDefault();
                    gestionIDI.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(gestionIDI.ID, gestionIDI);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = GestionIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Foros y comités nacionales e internacionales".
        /// Con el codigo identificativo 060.020.050.000
        /// </summary>
        public List<SubseccionItem> SincroForosComites(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/forums", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedForum";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetForosComites(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ForosComites forosComites = new ForosComites();
                    forosComites.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.forosComitesNombre)?.values.FirstOrDefault();
                    forosComites.categoriaProfesional = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.forosComitesCategoriaProfesional)?.values.FirstOrDefault();
                    forosComites.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(forosComites.ID, forosComites);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ForosComites.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Evaluación y revisión de proyectos y artículos de I+D+i".
        /// Con el codigo identificativo 060.020.060.000
        /// </summary>
        public List<SubseccionItem> SincroEvalRevIDI(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchEvaluations", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/functions";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedResearchEvaluation";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEvalRevIDI(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    EvalRevIDI evalRevIDI = new EvalRevIDI();
                    evalRevIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIFunciones)?.values.FirstOrDefault();
                    evalRevIDI.nombreActividad = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDINombre)?.values.FirstOrDefault();
                    evalRevIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIEntidadNombre)?.values.FirstOrDefault();
                    evalRevIDI.fechaInicio = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIFechaInicio)?.values.FirstOrDefault();
                    evalRevIDI.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(evalRevIDI.ID, evalRevIDI);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = EvalRevIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Estancias en centros de I+D+i públicos o privados".
        /// Con el codigo identificativo 060.010.050.000
        /// </summary>
        public List<SubseccionItem> SincroEstanciasIDI(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/stays", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "stay";
            string propTitle = "http://w3id.org/roh/performedTasks";
            string rdfType = "http://w3id.org/roh/Stay";
            string rdfTypePrefix = "RelatedStay";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEstanciasIDI(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    EstanciasIDI estanciasIDI = new EstanciasIDI();
                    estanciasIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables)?.values.FirstOrDefault();
                    estanciasIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion)?.values.FirstOrDefault();
                    estanciasIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre)?.values.FirstOrDefault();
                    estanciasIDI.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(estanciasIDI.ID, estanciasIDI);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = EstanciasIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Ayudas y becas obtenidas".
        /// Con el codigo identificativo 060.030.010.000
        /// </summary>
        public List<SubseccionItem> SincroAyudasBecas(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/grants", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "grant";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#Grant";
            string rdfTypePrefix = "RelatedGrant";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAyudasBecas(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    AyudaBecas ayudaBecas = new AyudaBecas();
                    ayudaBecas.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasNombre)?.values.FirstOrDefault();
                    ayudaBecas.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion)?.values.FirstOrDefault();
                    ayudaBecas.entidadConcesionaria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre)?.values.FirstOrDefault();
                    ayudaBecas.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(ayudaBecas.ID, ayudaBecas);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = AyudaBecas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otros modos de colaboración con investigadores/as o tecnólogos/as".
        /// Con el codigo identificativo 060.020.020.000
        /// </summary>
        public List<SubseccionItem> SincroOtrosModosColaboracion(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherCollaborations", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "collaboration";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Collaboration";
            string rdfTypePrefix = "RelatedOtherCollaboration";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrosModosColaboracion(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion();
                    otrosModosColaboracion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion)?.values.FirstOrDefault();
                    otrosModosColaboracion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabFechaInicio)?.values.FirstOrDefault();
                    otrosModosColaboracion.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otrosModosColaboracion.ID, otrosModosColaboracion);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrosModosColaboracion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Sociedades científicas y asociaciones profesionales".
        /// Con el codigo identificativo 060.030.020.000
        /// </summary>
        public List<SubseccionItem> SincroSociedadesAsociaciones(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/societies", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "society";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Society";
            string rdfTypePrefix = "RelatedSociety";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetSociedadesAsociaciones(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones();
                    sociedadesAsociaciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesNombre)?.values.FirstOrDefault();
                    sociedadesAsociaciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesFechaInicio)?.values.FirstOrDefault();
                    sociedadesAsociaciones.entidadAfiliacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre)?.values.FirstOrDefault();
                    sociedadesAsociaciones.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(sociedadesAsociaciones.ID, sociedadesAsociaciones);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = SociedadesAsociaciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Consejos editoriales".
        /// Con el codigo identificativo 060.030.030.000
        /// </summary>
        public List<SubseccionItem> SincroConsejos(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/councils", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "council";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Council";
            string rdfTypePrefix = "RelatedCouncil";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetConsejos(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    Consejos consejos = new Consejos();
                    consejos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.consejosNombre)?.values.FirstOrDefault();
                    consejos.EntAfi = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacionNombre)?.values.FirstOrDefault();
                    consejos.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(consejos.ID, consejos);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = Consejos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Redes de cooperación".
        /// Con el codigo identificativo 060.030.040.000
        /// </summary>
        public List<SubseccionItem> SincroRedesCooperacion(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/networks", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "network";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Network";
            string rdfTypePrefix = "RelatedNetwork";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetRedesCooperacion(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    RedesCooperacion redesCooperacion = new RedesCooperacion();
                    redesCooperacion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopNombre)?.values.FirstOrDefault();
                    redesCooperacion.IdRed = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopIdentificacion)?.values.FirstOrDefault();
                    redesCooperacion.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(redesCooperacion.ID, redesCooperacion);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = RedesCooperacion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Premios, menciones y distinciones".
        /// Con el codigo identificativo 060.030.050.000
        /// </summary>
        public List<SubseccionItem> SincroPremiosMenciones(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/prizes", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedPrize";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPremiosMenciones(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    PremiosMenciones premiosMenciones = new PremiosMenciones();
                    premiosMenciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.premiosMencionesDescripcion)?.values.FirstOrDefault();
                    premiosMenciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.premiosMencionesFechaConcesion)?.values.FirstOrDefault();
                    premiosMenciones.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(premiosMenciones.ID, premiosMenciones);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = PremiosMenciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otras distinciones (carrera profesional y/o empresarial)".
        /// Con codigo identificativo 060.030.060.000
        /// </summary>
        public List<SubseccionItem> SincroOtrasDistinciones(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDistinctions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedOtherDistinction";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasDistinciones(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtrasDistinciones otrasDistinciones = new OtrasDistinciones();
                    otrasDistinciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasDistincionesDescripcion)?.values.FirstOrDefault();
                    otrasDistinciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasDistincionesFechaConcesion)?.values.FirstOrDefault();
                    otrasDistinciones.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otrasDistinciones.ID, otrasDistinciones);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasDistinciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Períodos de actividad investigadora".
        /// Con codigo identificativo 060.030.070.000
        /// </summary>
        public List<SubseccionItem> SincroPeriodosActividad(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchActivityPeriods", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/recognizedPeriods";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedResearchActivityPeriod";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad de los datos del XML.
            List<Entity> listadoAux = GetPeriodosActividadInvestigadora(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    PeriodosActividad periodosActividad = new PeriodosActividad();
                    periodosActividad.numTramos = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraNumeroTramos)?.values.FirstOrDefault();
                    periodosActividad.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraFechaObtencion)?.values.FirstOrDefault();
                    periodosActividad.entidadAcreditante = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidadNombre)?.values.FirstOrDefault();
                    periodosActividad.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(periodosActividad.ID, periodosActividad);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = PeriodosActividad.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Acreditaciones/reconocimientos obtenidos".
        /// Con codigo identificativo 060.030.090.000
        /// </summary>
        public List<SubseccionItem> SincroAcreditacionesObtenidas(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/obtainedRecognitions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedObtainedRecognition";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAcreditacionesObtenidas(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos();
                    acreditacionesReconocimientos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.acreditacionesDescripcion)?.values.FirstOrDefault();
                    acreditacionesReconocimientos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.acreditacionesFechaObtencion)?.values.FirstOrDefault();
                    acreditacionesReconocimientos.nombreEntAcreditante = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.acreditacionesEntidadNombre)?.values.FirstOrDefault();
                    acreditacionesReconocimientos.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(acreditacionesReconocimientos.ID, acreditacionesReconocimientos);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = AcreditacionesReconocimientos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Resumen de otros méritos".
        /// Con codigo identificativo 060.030.100.000
        /// </summary>
        public List<SubseccionItem> SincroResumenOtrosMeritos(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherAchievements", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedOtherAchievement";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrosMeritos(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtrosMeritos otrosMeritos = new OtrosMeritos();
                    otrosMeritos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrosMeritosTextoLibre)?.values.FirstOrDefault();
                    otrosMeritos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrosMeritosFechaConcesion)?.values.FirstOrDefault();
                    otrosMeritos.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otrosMeritos.ID, otrosMeritos);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrosMeritos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }



        private void GetEntidadesSecundarias(ref Entity entityBBDD, Tuple<string, string, string> identificadores, List<string> rdfTypeItem, string graph)
        {
            if (!string.IsNullOrEmpty(identificadores.Item3))
            {
                entityBBDD = GetLoadedEntity(identificadores.Item3, graph);
            }
            else
            {
                string item1 = identificadores.Item1;
                string item2 = identificadores.Item2;
                string item3 = identificadores.Item3;
                if (string.IsNullOrEmpty(item2))
                {
                    string nombreEntidad = rdfTypeItem[0];
                    if (nombreEntidad.Contains("#"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains("/"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item2 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                if (string.IsNullOrEmpty(item3))
                {
                    string nombreEntidad = rdfTypeItem[1];
                    if (nombreEntidad.Contains("#"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains("/"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item3 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                identificadores = new Tuple<string, string, string>(item1, item2, item3);
            }
        }

        /// <summary>
        /// 060.010.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetProduccionCientifica(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPublicacionesDocumentos = listadoDatos.Where(x => x.Code.Equals("060.010.000.000")).ToList();
            if (listadoPublicacionesDocumentos.Count > 0)
            {
                foreach (CvnItemBean item in listadoPublicacionesDocumentos)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_PRODUCCION_CIENTIFICA";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();

                    if (!string.IsNullOrEmpty(item.GetStringDoublePorIDCampo("060.010.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "060.010.000.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceH, item.GetIndiceH("060.010.000.030")),
                            new Property(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceHOtros, item.GetStringPorIDCampo("060.010.000.040")),
                            new Property(Variables.ActividadCientificaTecnologica.prodCientificaIndiceH, item.GetStringDoublePorIDCampo("060.010.000.010")),
                            new Property(Variables.ActividadCientificaTecnologica.prodCientificaFechaAplicacion, item.GetStringDatetimePorIDCampo("060.010.000.020"))
                        ));

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// 060.010.060.010
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private Entity GetIndicadoresGenerales(List<CvnItemBean> listadoDatos)
        {
            try
            {
                Entity entity = new Entity();
                entity.auxEntityRemove = new List<string>();

                entity.properties = UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.indicadoresGeneralesCalidad, listadoDatos.GetElementoPorIDCampo<CvnItemBeanCvnRichText>("060.010.060.010")?.Value)
                );

                return entity;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 060.010.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetPublicacionesDocumentos(ConfigService mConfiguracion, List<CvnItemBean> listadoDatos,
            List<CvnItemBean> listadoSituacionProfesional, [Optional] PetitionStatus petitionStatus, [Optional] List<string> listaDOI, [Optional] bool preimportar)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPublicacionesDocumentos = listadoDatos.Where(x => x.Code.Equals("060.010.010.000")).ToList();
            if (listadoPublicacionesDocumentos.Count > 0)
            {
                foreach (CvnItemBean item in listadoPublicacionesDocumentos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    entidadAux.properties_cv = new List<Property>();
                    entidadAux.id = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.010.030")))
                    {
                        //Actualizo el estado de los recursos tratados
                        if (petitionStatus != null)
                        {
                            petitionStatus.actualWork++;
                            petitionStatus.actualWorkSubtitle = "IMPORTAR_PUBLICACIONES_DOCUMENTOS";
                        }

                        string doi = PublicacionesDocumentosComprobarDOI(item);
                        if (!string.IsNullOrEmpty(doi))
                        {
                            listaDOI.Add(doi);
                        }

                        //Carga normal de los datos

                        //Añado las etiquetas enriquecidas
                        string tituloPublicacion = item.GetStringPorIDCampo("060.010.010.030");
                        tituloPublicacion = Regex.Replace(tituloPublicacion, "<.*?>", string.Empty);
                        ObjEnriquecimiento objEnriquecimiento = new ObjEnriquecimiento(tituloPublicacion);

                        //Categorias enriquecidas
                        if (!preimportar)
                        {
                            Dictionary<string, string> dicTopics = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "thematic");
                            PublicacionesDocumentosTopics(dicTopics, entidadAux);
                            //Etiquetas enriquecidas
                            Dictionary<string, string> dicEtiquetas = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "specific");
                            PublicacionesDocumentosEtiquetas(dicEtiquetas, entidadAux);
                        }


                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property("http://w3id.org/roh/scientificActivityDocument", mResourceApi.GraphsUrl + "items/scientificactivitydocument_SAD1")
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProd, item.GetTipoPublicacionPorIDCampo("060.010.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProdOtros, item.GetStringPorIDCampo("060.010.010.020")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo, item.GetStringPorIDCampo("060.010.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubVolumen, item.GetVolumenPorIDCampo("060.010.010.080")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.010.080")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.010.090")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.010.090")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPais, item.GetPaisPorIDCampo("060.010.010.110")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubCCAA, item.GetRegionPorIDCampo("060.010.010.120")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha, item.GetStringDatetimePorIDCampo("060.010.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubURL, item.GetStringPorIDCampo("060.010.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubDepositoLegal, item.GetValueCvnExternalPKBean("060.010.010.170")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubCiudad, item.GetStringPorIDCampo("060.010.010.220")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosColeccion, item.GetStringPorIDCampo("060.010.010.270")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosReseniaRevista, item.GetStringDoublePorIDCampo("060.010.010.340"))
                        ));
                        entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosGradoContribucion, item.GetGradoContribucionDocumentoPorIDCampo("060.010.010.060")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosResultadosDestacados, item.GetStringPorIDCampo("060.010.010.290")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubRelevante, item.GetStringBooleanPorIDCampo("060.010.010.300")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("060.010.010.390"))
                        ));
                        PublicacionesDocumentosSoporte(item, entidadAux);
                        PublicacionesDocumentosAutores(item, entidadAux, listadoSituacionProfesional);
                        PublicacionesDocumentosTraducciones(item, entidadAux);
                        PublicacionesDocumentosIDPublicacion(item, entidadAux);
                        PublicacionesDocumentosISBN(item, entidadAux);
                        PublicacionesDocumentosCitasINRECS(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Devuelve el doi de la publicación
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string PublicacionesDocumentosComprobarDOI(CvnItemBean item)
        {
            string idDOIValue = "";
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.400");
            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                if (!string.IsNullOrEmpty(identificador.Type) && identificador.Type.Equals("040") && !string.IsNullOrEmpty(identificador.Value))
                {
                    idDOIValue = identificador.Value.Replace("http://dx.doi.org/", "").Replace("http://doi.org/", "");
                    idDOIValue = idDOIValue.Replace("https://dx.doi.org/", "").Replace("https://doi.org/", "");
                    idDOIValue = idDOIValue.Replace("doi:", "").Replace("DOI:", "");
                    idDOIValue = idDOIValue.Trim();
                }
            }
            return idDOIValue;
        }

        /// <summary>
        /// Añade el valor de las citas Inrecs del documento
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionesDocumentosCitasINRECS(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoCitas = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.010.010.310");
            foreach (CvnItemBeanCvnCodeGroup codeGroup in listadoCitas)
            {
                if (codeGroup.CvnString != null && codeGroup.CvnString.Length > 0 && codeGroup.CvnDouble != null && codeGroup.CvnDouble.Length > 0
                    && codeGroup.CvnString.First().Value.Equals("020") && codeGroup.CvnString.First().Code.Equals("060.010.010.320")
                    && codeGroup.CvnDouble.First().Code.Equals("060.010.010.310"))
                {
                    string valueInrecs = codeGroup.CvnDouble.First().Value;
                    entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosCitasInrecs, valueInrecs)
                    ));
                }
            }
        }

        /// <summary>
        /// Añade los topics/categorias enriquecidos del documento.
        /// </summary>
        /// <param name="dicTopics"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionesDocumentosTopics(Dictionary<string, string> dicTopics, Entity entidadAux)
        {
            if (dicTopics == null || dicTopics.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> topics in dicTopics)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string topic = UtilitySecciones.ObtenerTopics(topics.Key);

                List<string> topicList = new List<string>();
                topicList.AddRange(UtilitySecciones.GetPadresTesauro(topic));

                foreach (string topicIn in topicList)
                {
                    string topicInsert = UtilitySecciones.StringGNOSSID(entityPartAux, topicIn);
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAreasTematicasEnriquecidas, topicInsert)
                    ));
                }
            }
        }

        /// <summary>
        /// Añade las etiquetas enriquecidas del documento.
        /// </summary>
        /// <param name="dicEtiquetas"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionesDocumentosEtiquetas(Dictionary<string, string> dicEtiquetas, Entity entidadAux)
        {
            if (dicEtiquetas == null || dicEtiquetas.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> etiquetas in dicEtiquetas)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string etiquetasValor = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Key);
                string etiquetasScore = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Value);

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTextosEnriquecidosScore, etiquetasScore),
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTextosEnriquecidosTitulo, etiquetasValor)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Soporte. En el caso de que se encuentre el nombre de la revista en BBDD
        /// se añade directamente como tipo de soporte revista.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosSoporte(CvnItemBean item, Entity entidadAux)
        {
            //Compruebo si existe alguna revista con ese nombre
            string nombreRevista = item.GetStringPorIDCampo("060.010.010.210");
            string revista = UtilitySecciones.GetNombreRevista(mResourceApi, nombreRevista);

            //Si existe añado como tipo de soporte revista directamente.
            if (!string.IsNullOrEmpty(revista))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte, mResourceApi.GraphsUrl + "items/documentformat_057"),
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubMainDoc, revista)
                ));
            }
            else
            {
                //Si el tipo de soporte es revista, añado los datos
                if (item.GetStringPorIDCampo("060.010.010.070") != null && item.GetStringPorIDCampo("060.010.010.070").Equals("057"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                       new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte, mResourceApi.GraphsUrl + "items/documentformat_057"),
                       new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubEditorial, item.GetStringPorIDCampo("060.010.010.100")),
                       new Property(Variables.ActividadCientificaTecnologica.pubDocumentosNombreRevista, item.GetStringPorIDCampo("060.010.010.210"))
                   ));
                }
                else
                {
                    //Si el tipo de soporte es distinto a revista, añado los datos
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte, item.GetFormatoDocumentoPorIDCampo("060.010.010.070")),
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubEditorial, item.GetStringPorIDCampo("060.010.010.100")),
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubNombre, item.GetStringPorIDCampo("060.010.010.210"))
                    ));
                }
            }


        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosAutores(CvnItemBean item, Entity entidadAux, List<CvnItemBean> listadoSituacionProfesional)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.010.040");

            entidadAux.autores = new List<Persona>();
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                Persona persona = new Persona(autor.GetNombreAutor(), autor.GetPrimerApellidoAutor(), autor.GetSegundoApellidoAutor(), autor.GetFirmaAutor());

                //Si no tiene nombre no lo añado
                if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(persona.nombreCompleto) && !string.IsNullOrEmpty(persona.firma))
                {
                    persona.nombreCompleto = persona.firma;
                    persona.nombre = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).Count() > 1)
                    {
                        persona.primerApellido = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                }
                //Si no tiene firma le añado como firma el nombre completo
                if (string.IsNullOrEmpty(persona.firma))
                { persona.firma = persona.nombreCompleto; }

                persona.ID = Guid.NewGuid().ToString();
                entidadAux.autores.Add(persona);
            }

            foreach (Persona persona in entidadAux.autores)
            {
                persona.distincts = new HashSet<string>(entidadAux.autores.Select(x => x.ID).Except(new List<string> { persona.ID }));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Traducciones.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosTraducciones(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnTitleBean> listadoTraducciones = item.GetListaElementosPorIDCampo<CvnItemBeanCvnTitleBean>("060.010.010.350");
            foreach (CvnItemBeanCvnTitleBean traduccion in listadoTraducciones)
            {
                Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion);
                string valor = traduccion.GetTraduccion();
                string propiedad = Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion;
                UtilitySecciones.CheckProperty(IDOtro, entidadAux, valor, propiedad);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.400");
            string propIdHandle = Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadCientificaTecnologica.pubDocumentosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadCientificaTecnologica.pubDocumentosNombreOtroPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.160");
            string propiedadISBN = Variables.ActividadCientificaTecnologica.pubDocumentosPubISBN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// 060.010.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetTrabajosCongresos(ConfigService mConfiguracion, List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus, [Optional] List<string> listaDOI, [Optional] bool preimportar)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoTrabajosCongresos = listadoDatos.Where(x => x.Code.Equals("060.010.020.000")).ToList();
            if (listadoTrabajosCongresos.Count > 0)
            {
                foreach (CvnItemBean item in listadoTrabajosCongresos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    entidadAux.properties_cv = new List<Property>();
                    entidadAux.id = Guid.NewGuid().ToString();

                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_TRABAJOS_CONGRESOS";
                    }

                    string doi = TrabajosCongresosComprobarDOI(item);
                    if (!string.IsNullOrEmpty(doi))
                    {
                        listaDOI.Add(doi);
                    }

                    //Carga normal de los datos
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.020.030")))
                    {
                        // Añado las etiquetas enriquecidas
                        string tituloPublicacion = item.GetStringPorIDCampo("060.010.020.030");
                        tituloPublicacion = Regex.Replace(tituloPublicacion, "<.*?>", string.Empty);
                        ObjEnriquecimiento objEnriquecimiento = new ObjEnriquecimiento(tituloPublicacion);

                        if (!preimportar)
                        {
                            //Categorias
                            Dictionary<string, string> dicTopics = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "thematic");
                            TrabajosCongresosTopics(dicTopics, entidadAux);
                            //Etiquetas
                            Dictionary<string, string> dicEtiquetas = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "specific");
                            TrabajosCongresosEtiquetas(dicEtiquetas, entidadAux);
                        }

                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property("http://w3id.org/roh/scientificActivityDocument", mResourceApi.GraphsUrl + "items/scientificactivitydocument_SAD2")
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo, item.GetStringPorIDCampo("060.010.020.030")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubActa, item.GetStringBooleanPorIDCampo("060.010.020.200")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFormaContribucion, item.GetTipoPublicacionPorIDCampo("060.010.020.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubTitulo, item.GetStringPorIDCampo("060.010.020.230")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNombre, item.GetStringPorIDCampo("060.010.020.370")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubVolumen, item.GetVolumenPorIDCampo("060.010.020.240")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.020.240")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.020.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.020.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPais, item.GetPaisPorIDCampo("060.010.020.270")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubEditorial, item.GetStringPorIDCampo("060.010.020.260")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubCCAA, item.GetRegionPorIDCampo("060.010.020.280")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubFecha, item.GetStringDatetimePorIDCampo("060.010.020.300")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubURL, item.GetStringPorIDCampo("060.010.020.310")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubDepositoLegal, item.GetValueCvnExternalPKBean("060.010.020.330")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosNombreCongreso, item.GetStringPorIDCampo("060.010.020.100")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaCelebracion, item.GetStringDatetimePorIDCampo("060.010.020.190")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaFin, item.GetStringDatetimePorIDCampo("060.010.020.380")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadCelebracion, item.GetStringPorIDCampo("060.010.020.180")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisCelebracion, item.GetPaisPorIDCampo("060.010.020.150")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAACelebracion, item.GetRegionPorIDCampo("060.010.020.160")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEvento, item.GetTipoEventoPorIDCampo("060.010.020.010")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEventoOtros, item.GetStringPorIDCampo("060.010.020.020")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosComiteExterno, item.GetStringBooleanPorIDCampo("060.010.020.210")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeo, item.GetGeographicRegionPorIDCampo("060.010.020.080")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeoOtros, item.GetStringPorIDCampo("060.010.020.090"))
                        ));
                        entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoParticipacion, item.GetTipoParticipacionDocumentoPorIDCampo("060.010.020.050")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencion, item.GetTipoInscripcionEventoPorIDCampo("060.010.020.060")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencionOtros, item.GetStringPorIDCampo("060.010.020.070")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("060.010.020.390"))
                        ));
                        TrabajosCongresosAutores(item, entidadAux);
                        TrabajosCongresosIDPublicacion(item, entidadAux);
                        TrabajosCongresosISSN(item, entidadAux);
                        TrabajosCongresosISBN(item, entidadAux);
                        TrabajosCongresosEntidadOrganizadora(item, entidadAux);
                        TrabajosCongresosCitasINRECS(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        private string TrabajosCongresosComprobarDOI(CvnItemBean item)
        {
            string idDOIValue = "";
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.400");
            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                if (!string.IsNullOrEmpty(identificador.Type) && identificador.Type.Equals("040") && !string.IsNullOrEmpty(identificador.Value))
                {
                    idDOIValue = identificador.Value.Replace("http://dx.doi.org/", "").Replace("http://doi.org/", "");
                    idDOIValue = idDOIValue.Replace("https://dx.doi.org/", "").Replace("https://doi.org/", "");
                    idDOIValue = idDOIValue.Replace("doi:", "").Replace("DOI:", "");
                    idDOIValue = idDOIValue.Trim();
                }
            }
            return idDOIValue;
        }

        /// <summary>
        /// Añade el valor de las citas Inrecs del documento
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void TrabajosCongresosCitasINRECS(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoCitas = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.010.020.430");
            foreach (CvnItemBeanCvnCodeGroup codeGroup in listadoCitas)
            {
                if (codeGroup.CvnString != null && codeGroup.CvnString.Length > 0 && codeGroup.CvnDouble != null && codeGroup.CvnDouble.Length > 0
                    && codeGroup.CvnString.First().Value.Equals("020") && codeGroup.CvnString.First().Code.Equals("060.010.020.430")
                    && codeGroup.CvnDouble.First().Code.Equals("060.010.020.440"))
                {
                    string valueInrecs = codeGroup.CvnDouble.First().Value;
                    entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCitasInrecs, valueInrecs)
                    ));
                }
            }
        }

        /// <summary>
        /// Añade los topics/categorias enriquecidos del documento.
        /// </summary>
        /// <param name="dicTopics"></param>
        /// <param name="entidadAux"></param>
        private void TrabajosCongresosTopics(Dictionary<string, string> dicTopics, Entity entidadAux)
        {
            if (dicTopics == null || dicTopics.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> topics in dicTopics)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string topic = UtilitySecciones.ObtenerTopics(topics.Key);

                List<string> topicList = new List<string>();
                topicList.AddRange(UtilitySecciones.GetPadresTesauro(topic));

                foreach (string topicIn in topicList)
                {
                    string topicInsert = UtilitySecciones.StringGNOSSID(entityPartAux, topicIn);
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAreasTematicasEnriquecidas, topicInsert)
                    ));
                }
            }
        }

        /// <summary>
        /// Añade las etiquetas enriquecidas del documento.
        /// </summary>
        /// <param name="dicEtiquetas"></param>
        /// <param name="entidadAux"></param>
        private void TrabajosCongresosEtiquetas(Dictionary<string, string> dicEtiquetas, Entity entidadAux)
        {
            if (dicEtiquetas == null || dicEtiquetas.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> etiquetas in dicEtiquetas)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string etiquetasValor = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Key);
                string etiquetasScore = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Value);

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTextosEnriquecidosScore, etiquetasScore),
                    new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTextosEnriquecidosTitulo, etiquetasValor)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.020.040");

            entidadAux.autores = new List<Persona>();
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                Persona persona = new Persona(autor.GetNombreAutor(), autor.GetPrimerApellidoAutor(), autor.GetSegundoApellidoAutor(), autor.GetFirmaAutor());

                //Si no tiene nombre no lo añado
                if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(persona.nombreCompleto) && !string.IsNullOrEmpty(persona.firma))
                {
                    persona.nombreCompleto = persona.firma;
                    persona.nombre = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).Count() > 1)
                    {
                        persona.primerApellido = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                }
                //Si no tiene firma le añado como firma el nombre completo
                if (string.IsNullOrEmpty(persona.firma))
                { persona.firma = persona.nombreCompleto; }

                persona.ID = Guid.NewGuid().ToString();
                entidadAux.autores.Add(persona);
            }

            foreach (Persona persona in entidadAux.autores)
            {
                persona.distincts = new HashSet<string>(entidadAux.autores.Select(x => x.ID).Except(new List<string> { persona.ID }));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.400");
            string propIdHandle = Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadCientificaTecnologica.trabajosCongresosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadCientificaTecnologica.trabajosCongresosNombreOtroPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.320");
            string propiedadISBN = Variables.ActividadCientificaTecnologica.trabajosCongresosPubISBN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosISSN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISSN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.320");
            string propiedadISSN = Variables.ActividadCientificaTecnologica.trabajosCongresosPubISSN;

            UtilitySecciones.InsertaISSN(listadoISSN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.020.110"),
                Variables.ActividadCientificaTecnologica.trabajosCongresosEntidadOrganizadoraNombre,
                Variables.ActividadCientificaTecnologica.trabajosCongresosEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.020.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.020.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.010.020.140"))
            ));

            //Añado Pais, Region y ciudad
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("060.010.020.360")),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("060.010.020.340")),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("060.010.020.350"))
            ));
        }

        /// <summary>
        /// 060.010.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetTrabajosJornadasSeminarios(ConfigService mConfiguracion, List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus, [Optional] bool preimportar)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoComitesTrabajosJornadasSeminarios = listadoDatos.Where(x => x.Code.Equals("060.010.030.000")).ToList();
            if (listadoComitesTrabajosJornadasSeminarios.Count > 0)
            {
                foreach (CvnItemBean item in listadoComitesTrabajosJornadasSeminarios)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_TRABAJOS_JORNADAS_SEMINARIOS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    entidadAux.properties_cv = new List<Property>();
                    entidadAux.id = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.030.010")))
                    {
                        // Añado las etiquetas enriquecidas
                        string tituloPublicacion = item.GetStringPorIDCampo("060.010.030.010");
                        tituloPublicacion = Regex.Replace(tituloPublicacion, "<.*?>", string.Empty);
                        ObjEnriquecimiento objEnriquecimiento = new ObjEnriquecimiento(tituloPublicacion);

                        //Categorias
                        if (!preimportar)
                        {
                            Dictionary<string, string> dicTopics = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "thematic");
                            TrabajosJornadasSeminariosTopics(dicTopics, entidadAux);
                            //Etiquetas
                            Dictionary<string, string> dicEtiquetas = objEnriquecimiento.getDescriptores(mConfiguracion, objEnriquecimiento, "specific");
                            TrabajosJornadasSeminariosEtiquetas(dicEtiquetas, entidadAux);
                        }

                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property("http://w3id.org/roh/scientificActivityDocument", mResourceApi.GraphsUrl + "items/scientificactivitydocument_SAD3")
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo, item.GetStringPorIDCampo("060.010.030.010")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongreso, item.GetStringBooleanPorIDCampo("060.010.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTipo, item.GetTipoPublicacionPorIDCampo("060.010.030.190")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTitulo, item.GetStringPorIDCampo("060.010.030.200")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNombre, item.GetStringPorIDCampo("060.010.030.350")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubVolumen, item.GetVolumenPorIDCampo("060.010.030.210")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.030.210")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPais, item.GetPaisPorIDCampo("060.010.030.240")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubEditorial, item.GetStringPorIDCampo("060.010.030.230")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubCCAA, item.GetRegionPorIDCampo("060.010.030.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFecha, item.GetStringDatetimePorIDCampo("060.010.030.270")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubURL, item.GetStringPorIDCampo("060.010.030.280")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubDepositoLegal, item.GetValueCvnExternalPKBean("060.010.030.300")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemNombreEvento, item.GetStringPorIDCampo("060.010.030.070")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemFechaCelebracion, item.GetStringDatetimePorIDCampo("060.010.030.160")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFechaFinCelebracion, item.GetStringDatetimePorIDCampo("060.010.030.370")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadCelebracion, item.GetStringPorIDCampo("060.010.030.150")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisCelebracion, item.GetPaisPorIDCampo("060.010.030.120")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAACelebracion, item.GetRegionPorIDCampo("060.010.030.130")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEvento, item.GetTipoEventoSeminarioPorIDCampo("060.010.030.020")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEventoOtros, item.GetStringPorIDCampo("060.010.030.030")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongresoExterno, item.GetStringBooleanPorIDCampo("060.010.030.180")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeo, item.GetGeographicRegionPorIDCampo("060.010.030.050")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeoOtros, item.GetStringPorIDCampo("060.010.030.060"))
                        ));
                        entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemIntervencion, item.GetTipoInscripcionSeminarioPorIDCampo("060.010.030.040")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorCorrespondencia, item.GetStringBooleanPorIDCampo("060.010.030.390"))
                        ));
                        TrabajosJornadasSeminariosAutores(item, entidadAux);
                        TrabajosJornadasSeminariosIDPublicacion(item, entidadAux);
                        TrabajosJornadasSeminariosISBN(item, entidadAux);
                        TrabajosJornadasSeminariosISSN(item, entidadAux);
                        TrabajosJornadasSeminariosEntidadOrganizadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Añade los topics/categorias enriquecidos del documento.
        /// </summary>
        /// <param name="dicTopics"></param>
        /// <param name="entidadAux"></param>
        private void TrabajosJornadasSeminariosTopics(Dictionary<string, string> dicTopics, Entity entidadAux)
        {
            if (dicTopics == null || dicTopics.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> topics in dicTopics)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string topic = UtilitySecciones.ObtenerTopics(topics.Key);

                List<string> topicList = new List<string>();
                topicList.AddRange(UtilitySecciones.GetPadresTesauro(topic));

                foreach (string topicIn in topicList)
                {
                    string topicInsert = UtilitySecciones.StringGNOSSID(entityPartAux, topicIn);
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAreasTematicasEnriquecidas, topicInsert)
                    ));
                }
            }
        }

        /// <summary>
        /// Añade las etiquetas enriquecidas del documento.
        /// </summary>
        /// <param name="dicEtiquetas"></param>
        /// <param name="entidadAux"></param>
        private void TrabajosJornadasSeminariosEtiquetas(Dictionary<string, string> dicEtiquetas, Entity entidadAux)
        {
            if (dicEtiquetas == null || dicEtiquetas.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> etiquetas in dicEtiquetas)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                string etiquetasValor = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Key);
                string etiquetasScore = UtilitySecciones.StringGNOSSID(entityPartAux, etiquetas.Value);

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTextosEnriquecidosScore, etiquetasScore),
                    new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTextosEnriquecidosTitulo, etiquetasValor)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.030.310");
            entidadAux.autores = new List<Persona>();

            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                Persona persona = new Persona(autor.GetNombreAutor(), autor.GetPrimerApellidoAutor(), autor.GetSegundoApellidoAutor(), autor.GetFirmaAutor());

                //Si no tiene nombre no lo añado
                if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(persona.nombreCompleto) && !string.IsNullOrEmpty(persona.firma))
                {
                    persona.nombreCompleto = persona.firma;
                    persona.nombre = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).Count() > 1)
                    {
                        persona.primerApellido = persona.firma.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                }
                //Si no tiene firma le añado como firma el nombre completo
                if (string.IsNullOrEmpty(persona.firma))
                { persona.firma = persona.nombreCompleto; }

                persona.ID = Guid.NewGuid().ToString();
                entidadAux.autores.Add(persona);
            }

            foreach (Persona persona in entidadAux.autores)
            {
                persona.distincts = new HashSet<string>(entidadAux.autores.Select(x => x.ID).Except(new List<string> { persona.ID }));
            }
        }

        /// <summary>
        /// /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.030.400");
            string propIdHandle = Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadCientificaTecnologica.trabajosJornSemIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadCientificaTecnologica.trabajosJornSemNombreOtroPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.030.290");
            string propiedadISBN = Variables.ActividadCientificaTecnologica.trabajosJornSemPubISBN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosISSN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISSN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.030.290");
            string propiedadISSN = Variables.ActividadCientificaTecnologica.trabajosJornSemPubISSN;

            UtilitySecciones.InsertaISSN(listadoISSN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.030.080"),
                Variables.ActividadCientificaTecnologica.trabajosJornSemEntidadOrganizadoraNombre,
                Variables.ActividadCientificaTecnologica.trabajosJornSemEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.030.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.030.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.010.030.110"))
            ));

            //Añado Pais, Region y ciudad
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisEntidadOrganizadora, item.GetPaisPorIDCampo("060.010.030.320")),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("060.010.030.330")),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadEntidadOrganizadora, item.GetStringPorIDCampo("060.010.030.340"))
            ));
        }

        /// <summary>
        /// 060.010.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetOtrasActividadesDivulgacion(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrasActividadesDivulgacion = listadoDatos.Where(x => x.Code.Equals("060.010.040.000")).ToList();
            if (listadoOtrasActividadesDivulgacion.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrasActividadesDivulgacion)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_OTRAS_ACTIVIDADES_DIVULGACION";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.040.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "060.010.040.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTitulo, item.GetStringPorIDCampo("060.010.040.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIntervencion, item.GetTipoIntervencionPorIDCampo("060.010.040.040")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIntervencionOtros, item.GetStringPorIDCampo("060.010.040.050")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPaisEntidadOrg, item.GetPaisPorIDCampo("060.010.040.320")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCCAAEntidadOrg, item.GetRegionPorIDCampo("060.010.040.330")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCiudadEntidadOrg, item.GetStringPorIDCampo("060.010.040.340")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubActaCongreso, item.GetStringBooleanPorIDCampo("060.010.040.180")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubTipo, item.GetTipoPublicacionPorIDCampo("060.010.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubTitulo, item.GetStringPorIDCampo("060.010.040.210")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubNombre, item.GetStringPorIDCampo("060.010.040.360")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubVolumen, item.GetVolumenPorIDCampo("060.010.040.220")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.040.220")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.040.230")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.040.230")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulResponsableEditorial, item.GetStringPorIDCampo("060.010.040.240")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPais, item.GetPaisPorIDCampo("060.010.040.250")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubCCAA, item.GetRegionPorIDCampo("060.010.040.260")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha, item.GetStringDatetimePorIDCampo("060.010.040.280")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubURL, item.GetStringPorIDCampo("060.010.040.290")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubDepositoLegal, item.GetValueCvnExternalPKBean("060.010.040.310")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorCorrespondencia, item.GetStringBooleanPorIDCampo("060.010.040.390"))
                        ));
                        OtrasActividadesDivulgacionEvento(item, entidadAux);
                        OtrasActividadesDivulgacionEntidad(item, entidadAux);
                        OtrasActividadesDivulgacionAutores(item, entidadAux);
                        OtrasActividadesDivulgacionIDPublicacion(item, entidadAux);
                        OtrasActividadesDivulgacionISBN(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>
        /// pertenecientes a los Eventos
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionEvento(CvnItemBean item, Entity entidadAux)
        {
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulNombreEvento, item.GetStringPorIDCampo("060.010.040.080")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulFechaCelebracion, item.GetStringDatetimePorIDCampo("060.010.040.170")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCiudadCelebracion, item.GetStringPorIDCampo("060.010.040.160")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPaisCelebracion, item.GetPaisPorIDCampo("060.010.040.130")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCCAACelebracion, item.GetRegionPorIDCampo("060.010.040.140")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEvento, item.GetTipoEventoPorIDCampo("060.010.040.020")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEventoOtros, item.GetStringPorIDCampo("060.010.040.030")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubActaAdmisionExt, item.GetStringBooleanPorIDCampo("060.010.040.190")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAmbitoEvento, item.GetGeographicRegionPorIDCampo("060.010.040.060")),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAmbitoEventoOtros, item.GetStringPorIDCampo("060.010.040.070"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.040.350");

            string propiedadAutorFirma = Variables.ActividadCientificaTecnologica.otrasActDivulAutorFirma;
            string propiedadAutorOrden = Variables.ActividadCientificaTecnologica.otrasActDivulAutorOrden;
            string propiedadAutorNombre = Variables.ActividadCientificaTecnologica.otrasActDivulAutorNombre;
            string propiedadAutorPrimerApellido = Variables.ActividadCientificaTecnologica.otrasActDivulAutorPrimerApellido;
            string propiedadAutorSegundoApellido = Variables.ActividadCientificaTecnologica.otrasActDivulAutorSegundoApellido;

            UtilitySecciones.InsertaAutorProperties(listadoAutores, entidadAux, propiedadAutorFirma, propiedadAutorOrden,
                propiedadAutorNombre, propiedadAutorPrimerApellido, propiedadAutorSegundoApellido);

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.040.400");
            string propIdHandle = Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadCientificaTecnologica.otrasActDivulIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadCientificaTecnologica.otrasActDivulNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.040.300");
            string propiedadISBN = Variables.ActividadCientificaTecnologica.otrasActDivulPubISBN;
            string propiedadISSN = Variables.ActividadCientificaTecnologica.otrasActDivulPubISSN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
            UtilitySecciones.InsertaISSN(listadoISBN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.040.090"),
                Variables.ActividadCientificaTecnologica.otrasActDivulEntidadOrgNombre,
                Variables.ActividadCientificaTecnologica.otrasActDivulEntidadOrg, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.040.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.040.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEntidadOrg, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEntidadOrgOtros, item.GetStringPorIDCampo("060.010.040.120"))
            ));
        }

        /// <summary>
        /// Comités científicos, técnicos y/o asesores - 060.020.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetComitesCTA(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoComitesCTA = listadoDatos.Where(x => x.Code.Equals("060.020.010.000")).ToList();
            if (listadoComitesCTA.Count > 0)
            {
                foreach (CvnItemBean item in listadoComitesCTA)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_COMITES_CTA";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.020.010.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTATitulo, item.GetStringPorIDCampo("060.020.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAPaisRadicacion, item.GetPaisPorIDCampo("060.020.010.020")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACCAARadicacion, item.GetRegionPorIDCampo("060.020.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACiudadRadicacion, item.GetStringPorIDCampo("060.020.010.050")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.020.010.190")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.020.010.180")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.020.010.170")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAAmbitoActividad, item.GetGeographicRegionPorIDCampo("060.020.010.100")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAAmbitoActividadOtros, item.GetStringPorIDCampo("060.020.010.110")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAFechaInicio, item.GetStringDatetimePorIDCampo("060.020.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.010.160"))
                        ));
                        ComitesCTAEntidadAfiliacion(item, entidadAux);
                        ComitesCTACodigosUnesco(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ComitesCTAEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad de Afiliación
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.010.060"),
                Variables.ActividadCientificaTecnologica.comitesCTAEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.comitesCTAEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.010.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.010.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacion, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.020.010.090"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO de especialización primaria, secundaria y terciaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ComitesCTACodigosUnesco(CvnItemBean item, Entity entidadAux)
        {
            //Añado los códigos UNESCO de especialización primaria
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.120");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoPrimaria, entidadAux, Variables.ActividadCientificaTecnologica.comitesCTACodUnescoPrimaria);

            //Añado los códigos UNESCO de especialización secundaria
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.130");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoSecundaria, entidadAux, Variables.ActividadCientificaTecnologica.comitesCTACodUnescoSecundaria);

            //Añado los códigos UNESCO de especialización terciaria
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.140");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoTerciaria, entidadAux, Variables.ActividadCientificaTecnologica.comitesCTACodUnescoTerciaria);

        }


        /// <summary>
        /// 060.020.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetOrganizacionIDI(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOrganizacionIDI = listadoDatos.Where(x => x.Code.Equals("060.020.030.000")).ToList();
            if (listadoOrganizacionIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoOrganizacionIDI)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_ORGANIZACION_IDI";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.030.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.020.030.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.orgIDITituloActividad, item.GetStringPorIDCampo("060.020.030.010")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDITipoActividad, item.GetStringPorIDCampo("060.020.030.020")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIPaisActividad, item.GetPaisPorIDCampo("060.020.030.030")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICCAAActividad, item.GetRegionPorIDCampo("060.020.030.040")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICiudadActividad, item.GetStringPorIDCampo("060.020.030.060")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIPaisEntidadConvocante, item.GetPaisPorIDCampo("060.020.030.180")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICCAAEntidadConvocante, item.GetRegionPorIDCampo("060.020.030.190")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICiudadEntidadConvocante, item.GetStringPorIDCampo("060.020.030.200")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacion, item.GetTipoParticipacionActividadPorIDCampo("060.020.030.110")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacionOtros, item.GetStringPorIDCampo("060.020.030.120")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunion, item.GetGeographicRegionPorIDCampo("060.020.030.130")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunionOtros, item.GetStringPorIDCampo("060.020.030.140")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIFechaInicio, item.GetStringDatetimePorIDCampo("060.020.030.160")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionMes, item.GetDurationMesPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionDia, item.GetDurationDiaPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDINumeroAsistentes, item.GetStringDoublePorIDCampo("060.020.030.150"))
                        ));
                        OrganizacionIDIEntidadConvocante(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Convocante.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OrganizacionIDIEntidadConvocante(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Convocante
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.030.070"),
                Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre,
                Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocante, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.030.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.030.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocante, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocanteOtros, item.GetStringPorIDCampo("060.020.030.100"))
            ));
        }

        /// <summary>
        /// 060.020.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetGestionIDI(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoGestionIDI = listadoDatos.Where(x => x.Code.Equals("060.020.040.000")).ToList();
            if (listadoGestionIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoGestionIDI)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_GESTION_IDI";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.040.060")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.020.040.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIFunciones, item.GetStringPorIDCampo("060.020.040.010")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.040.020")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.040.030")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.040.050")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDINombreActividad, item.GetStringPorIDCampo("060.020.040.060")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestion, item.GetTipoTipologiaGestionPorIDCampo("060.020.040.070")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestionOtros, item.GetStringPorIDCampo("060.020.040.080")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio, item.GetStringDatetimePorIDCampo("060.020.040.130")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionMes, item.GetDurationMesPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionDia, item.GetDurationDiaPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDISistemaAcceso, item.GetSistemaActividadPorIDCampo("060.020.040.150")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDISistemaAccesoOtros, item.GetStringPorIDCampo("060.020.040.160")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPromedioPresupuesto, item.GetStringDoublePorIDCampo("060.020.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDINumPersonas, item.GetStringDoublePorIDCampo("060.020.040.180")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIObjetivosEvento, item.GetStringPorIDCampo("060.020.040.240")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPerfilGrupo, item.GetTipoPerfilGrupoPorIDCampo("060.020.040.190")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorial, item.GetGeographicRegionPorIDCampo("060.020.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorialOtros, item.GetStringPorIDCampo("060.020.040.210")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITareasConcretas, item.GetStringPorIDCampo("060.020.040.220"))
                        ));
                        GestionIDIEntidadRealizacion(item, entidadAux);
                        GestionIDIPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void GestionIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.040.230");

            string propiedadPalabrasClave = Variables.ActividadCientificaTecnologica.gestionIDIPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void GestionIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Realizacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.040.090"),
                Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre,
                Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.040.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.040.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("060.020.040.120"))
            ));
        }


        /// <summary>
        /// 060.020.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetForosComites(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoForosComites = listadoDatos.Where(x => x.Code.Equals("060.020.050.000")).ToList();
            if (listadoForosComites.Count > 0)
            {
                foreach (CvnItemBean item in listadoForosComites)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_FOROS_COMITES";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.020.050.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesNombre, item.GetStringPorIDCampo("060.020.050.010")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.050.020")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.050.030")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.050.050")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadOrganizadora, item.GetPaisPorIDCampo("060.020.050.170")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("060.020.050.180")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadOrganizadora, item.GetStringPorIDCampo("060.020.050.190")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCategoriaProfesional, item.GetStringPorIDCampo("060.020.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadRepresentada, item.GetPaisPorIDCampo("060.020.050.200")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadRepresentada, item.GetRegionPorIDCampo("060.020.050.210")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadRepresentada, item.GetStringPorIDCampo("060.020.050.220")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesFechaInicio, item.GetStringDatetimePorIDCampo("060.020.050.150")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.050.160"))
                        ));
                        ForosComitesEntidadOrganizadoraORepresentada(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ForosComitesEntidadOrganizadoraORepresentada(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.050.060"),
                Variables.ActividadCientificaTecnologica.forosComitesEntidadOrganizadoraNombre,
                Variables.ActividadCientificaTecnologica.forosComitesEntidadOrganizadora, entidadAux);

            //Añado otros Entidad Organizadora, o el ID de una preseleccion
            string valorTipoEO = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.050.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoEntidadOrganizadora, valorTipoEO),
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.020.050.090"))
            ));


            //Añado la referencia si existe Entidad Representada
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.050.110"),
                Variables.ActividadCientificaTecnologica.forosComitesOrganismoRepresentadoNombre,
                Variables.ActividadCientificaTecnologica.forosComitesOrganismoRepresentado, entidadAux);

            //Añado otros Entidad Representada, o el ID de una preseleccion
            string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.050.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoOrganismoRepresentado, valorTipoER),
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoOrganismoRepresentadoOtros, item.GetStringPorIDCampo("060.020.050.140"))
            ));
        }

        /// <summary>
        /// 060.020.060.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEvalRevIDI(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEvalRevIDI = listadoDatos.Where(x => x.Code.Equals("060.020.060.000")).ToList();
            if (listadoEvalRevIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoEvalRevIDI)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_EVAL_REV_IDI";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.060.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "060.020.060.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFunciones, item.GetStringPorIDCampo("060.020.060.010")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDINombre, item.GetStringPorIDCampo("060.020.060.020")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.060.030")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.060.040")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.060.190")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIModalidadActividad, item.GetModalidadActividadPorIDCampo("060.020.060.060")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIModalidadActividadOtros, item.GetStringPorIDCampo("060.020.060.070")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFechaInicio, item.GetStringDatetimePorIDCampo("060.020.060.120")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.060.130")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFrecuenciaActividad, item.GetStringDoublePorIDCampo("060.020.060.140")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDISistemaAcceso, item.GetSistemaActividadPorIDCampo("060.020.060.150")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDISistemaAccesoOtros, item.GetStringPorIDCampo("060.020.060.160")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIAmbito, item.GetGeographicRegionPorIDCampo("060.020.060.170")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIAmbitoOtros, item.GetStringPorIDCampo("060.020.060.180"))
                        ));
                        EvalRevIDIEntidadRealizacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void EvalRevIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.060.080"),
                Variables.ActividadCientificaTecnologica.evalRevIDIEntidadNombre,
                Variables.ActividadCientificaTecnologica.evalRevIDIEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.060.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.060.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.evalRevIDITipoEntidad, valorTipoER),
                new Property(Variables.ActividadCientificaTecnologica.evalRevIDITipoEntidadOtros, item.GetStringPorIDCampo("060.020.060.110"))
            ));
        }

        /// <summary>
        /// 060.010.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEstanciasIDI(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEstanciasIDI = listadoDatos.Where(x => x.Code.Equals("060.010.050.000")).ToList();
            if (listadoEstanciasIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoEstanciasIDI)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_ESTANCIAS_IDI";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.210")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.010.050.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.010.050.050")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.010.050.060")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.010.050.080")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion, item.GetStringDatetimePorIDCampo("060.010.050.090")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionAnioEntidadRealizacion, item.GetDurationAnioPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionMesEntidadRealizacion, item.GetDurationMesPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionDiaEntidadRealizacion, item.GetDurationDiaPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIObjetivoEstancia, item.GetObjetivoPorIDCampo("060.010.050.110")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIObjetivoEstanciaOtros, item.GetStringPorIDCampo("060.010.050.120")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIPaisEntidadFinanciadora, item.GetPaisPorIDCampo("060.010.050.250")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICCAAEntidadFinanciadora, item.GetRegionPorIDCampo("060.010.050.260")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICiudadEntidadFinanciadora, item.GetStringPorIDCampo("060.010.050.270")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDINombrePrograma, item.GetStringPorIDCampo("060.010.050.200")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables, item.GetStringPorIDCampo("060.010.050.210")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICapacidadesAdquiridas, item.GetStringPorIDCampo("060.010.050.220")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIResultadosRelevantes, item.GetStringPorIDCampo("060.010.050.230")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.010.050.290"))
                        ));
                        EstanciasIDICodigosUnesco(item, entidadAux);
                        EstanciasIDIEntidadRealizacion(item, entidadAux);
                        EstanciasIDIPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstanciasIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.240");

            string propiedadPalabrasClave = Variables.ActividadCientificaTecnologica.estanciasIDIPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDIPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO de especialización primaria, secundaria y terciaria.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstanciasIDICodigosUnesco(CvnItemBean item, Entity entidadAux)
        {
            //Añado los códigos UNESCO de especialización primaria
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.130");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoPrimaria, entidadAux, Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoPrimaria);

            //Añado los códigos UNESCO de especialización secundaria
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.140");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoSecundaria, entidadAux, Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoSecundaria);

            //Añado los códigos UNESCO de especialización terciaria
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.150");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoTerciaria, entidadAux, Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoTerciaria);

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void EstanciasIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Compruebo que existe entidad de realizacion
            if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.010.050.010")))
            {
                //Añado la referencia si existe
                UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.050.010"),
                    Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre,
                    Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacion, entidadAux);

                //Añado otros Entidad Realizacion, o el ID de una preseleccion
                string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.040")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.050.030");

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadRealizacion, valorTipoER),
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadRealizacionOtros, item.GetStringPorIDCampo("060.010.050.040"))
                ));

                //Añado Facultad, instituto, centro
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFacultadEscuela, item.GetNameEntityBeanPorIDCampo("060.010.050.280"))
                ));
            }

            //Compruebo que existe entidad de financiación
            if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.010.050.160")))
            {
                //Añado otros Entidad Financiacion, o el ID de una preseleccion
                string valorTipoEF = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.190")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.050.180");

                //Añado la referencia si existe
                UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.050.160"),
                    Variables.ActividadCientificaTecnologica.estanciasIDIEntidadFinanciadoraNombre,
                    Variables.ActividadCientificaTecnologica.estanciasIDIEntidadFinanciadora, entidadAux);

                //Añado tipo entidad
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadFinanciadora, valorTipoEF),
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("060.010.050.190"))
                ));
            }
        }

        /// <summary>
        /// 060.030.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetAyudasBecas(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAyudasBecas = listadoDatos.Where(x => x.Code.Equals("060.030.010.000")).ToList();
            if (listadoAyudasBecas.Count > 0)
            {
                foreach (CvnItemBean item in listadoAyudasBecas)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_AYUDA_BECAS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.010.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasNombre, item.GetStringPorIDCampo("060.030.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasPaisConcede, item.GetPaisPorIDCampo("060.030.010.020")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasCCAAConcede, item.GetRegionPorIDCampo("060.030.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasCiudadConcede, item.GetStringPorIDCampo("060.030.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidad, item.GetFinalidadPorIDCampo("060.030.010.060")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidadOtros, item.GetStringPorIDCampo("060.030.010.070")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasImporte, item.GetStringDoublePorIDCampo("060.030.010.120")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.010.130")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionMes, item.GetDurationMesPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionDia, item.GetDurationDiaPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.030.010.160"))
                         ));
                        AyudasBecasEntidadConcede(item, entidadAux);
                        AyudasBecasEntidadRealizacion(item, entidadAux);
                        AyudaBecasPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void AyudaBecasPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.030.010.050");

            string propiedadPalabrasClave = Variables.ActividadCientificaTecnologica.ayudasBecasPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void AyudasBecasEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.010.180")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.010.180"),
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadRealizacionNombre,
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadRealizacion, entidadAux);

            //Añado Facultad, instituto, centro
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFacultadEscuela, item.GetNameEntityBeanPorIDCampo("060.030.010.170"))
            ));

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void AyudasBecasEntidadConcede(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.010.080")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.010.080"),
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre,
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcede, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.010.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.010.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcede, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcedeOtros, item.GetStringPorIDCampo("060.030.010.110"))
            ));
        }

        /// <summary>
        /// 060.020.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrosModosColaboracion(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrosModosColaboracion = listadoDatos.Where(x => x.Code.Equals("060.020.020.000")).ToList();
            if (listadoOtrosModosColaboracion.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrosModosColaboracion)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_OTROS_MODOS_COLABORACION";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.020.140")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.020.020.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabModoRelacion, item.GetRelacionPorIDCampo("060.020.020.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabModoRelacionOtros, item.GetStringPorIDCampo("060.020.020.020")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabPaisRadicacion, item.GetPaisPorIDCampo("060.020.020.030")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabCCAARadicacion, item.GetRegionPorIDCampo("060.020.020.040")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabCiudadRadicacion, item.GetStringPorIDCampo("060.020.020.060")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabFechaInicio, item.GetStringDatetimePorIDCampo("060.020.020.120")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionMes, item.GetDurationMesPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionDia, item.GetDurationDiaPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion, item.GetStringPorIDCampo("060.020.020.140")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabResultadosRelevantes, item.GetStringPorIDCampo("060.020.020.150"))
                         ));
                        OtrosModosColaboracionAutores(item, entidadAux);
                        OtrosModosColaboracionEntidadesParticipantes(item, entidadAux);
                        OtrosModosColaboracionPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrosModosColaboracionPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.020.160");

            string propiedadPalabrasClave = Variables.ActividadCientificaTecnologica.otrasColabPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores,
        /// pertenecientes a los Autores.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrosModosColaboracionAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.020.020.070");

            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                if (!string.IsNullOrEmpty(autor.GivenName))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabFirmaInvestigador);
                    Property propertyOrden = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabOrdenInvestigador);
                    Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabNombreInvestigador);
                    Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabPrimApellInvestigador);
                    Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabSegApellInvestigador);

                    //Añado firma investigador
                    string valorFirma = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetFirmaAutor());
                    string propiedadFirma = Variables.ActividadCientificaTecnologica.otrasColabFirmaInvestigador;
                    UtilitySecciones.CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadFirma);

                    //Añado orden
                    string valorOrden = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetOrdenAutor());
                    string propiedadOrden = Variables.ActividadCientificaTecnologica.otrasColabOrdenInvestigador;
                    UtilitySecciones.CheckProperty(propertyOrden, entidadAux, valorOrden, propiedadOrden);

                    //Añado nombre investigador
                    string valorNombre = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetNombreAutor());
                    string propiedadNombre = Variables.ActividadCientificaTecnologica.otrasColabNombreInvestigador;
                    UtilitySecciones.CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadNombre);

                    //Añado primer apellido investigador
                    string valorPrimerApellido = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor());
                    string propiedadPrimerApellido = Variables.ActividadCientificaTecnologica.otrasColabPrimApellInvestigador;
                    UtilitySecciones.CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadPrimerApellido);

                    //Añado segundo apellido investigador
                    string valorSegundoApellido = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor());
                    string propiedadSegundoApellido = Variables.ActividadCientificaTecnologica.otrasColabSegApellInvestigador;
                    UtilitySecciones.CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadSegundoApellido);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de las Entidades Participantes,
        /// pertenecientes al listado <paramref name="listadoEntidadParticipante"/>.
        /// </summary>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrosModosColaboracionEntidadesParticipantes(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadParticipante = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.020.020.080");

            foreach (CvnItemBeanCvnCodeGroup entidadParticipante in listadoEntidadParticipante)
            {
                if (!string.IsNullOrEmpty(entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080")))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    Property propertyEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantes);
                    Property propertyEntidadParticipanteNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantesNombre);
                    Property propertyPaisEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabPaisEntidadParticipante);
                    Property propertyRegionEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabCCAAEntidadParticipante);
                    Property propertyCiudadEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabCiudadEntidadParticipante);

                    //Añado titulo entidad participante
                    string valorTituloEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080"));
                    string propiedadTituloEP = Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantesNombre;
                    UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorTituloEP, propiedadTituloEP);

                    //Añado referencia entidad participante
                    string nombreEP = entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080");
                    if (string.IsNullOrEmpty(nombreEP))
                    {
                        string referenciaEP = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombreEP);
                        string valorEP = UtilitySecciones.StringGNOSSID(entityPartAux, referenciaEP);
                        string propiedadEP = Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantes;
                        UtilitySecciones.CheckProperty(propertyEntidadParticipante, entidadAux, valorEP, propiedadEP);
                    }

                    //Añado pais entidad participante
                    string valorPaisEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetPaisPorIDCampo("060.020.020.170"));
                    string propiedadPaisEP = Variables.ActividadCientificaTecnologica.otrasColabPaisEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyPaisEntidadParticipante, entidadAux, valorPaisEP, propiedadPaisEP);

                    //Añado region entidad participante
                    string valorRegionEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetRegionPorIDCampo("060.020.020.180"));
                    string propiedadRegionEP = Variables.ActividadCientificaTecnologica.otrasColabCCAAEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyRegionEntidadParticipante, entidadAux, valorRegionEP, propiedadRegionEP);

                    //Añado ciudad entidad participante
                    string valorCiudadEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.020.020.190"));
                    string propiedadCiudadEP = Variables.ActividadCientificaTecnologica.otrasColabCiudadEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyCiudadEntidadParticipante, entidadAux, valorCiudadEP, propiedadCiudadEP);

                    Property propertyTipoEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad);
                    Property propertyTipoEntidadParticipanteOtros = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros);

                    //Añado tipo entidad participante
                    string valorOtroTipoEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.020.020.110"));
                    string propiedadOtroTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros;

                    string valorTipoEP = !string.IsNullOrEmpty(valorOtroTipoEP) ? UtilitySecciones.StringGNOSSID(entityPartAux, mResourceApi.GraphsUrl + "items/organizationtype_OTHERS") : UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetOrganizationCvnCodeGroup("060.020.020.100"));
                    string propiedadTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad;

                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipante, entidadAux, valorTipoEP, propiedadTipoEP);
                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipanteOtros, entidadAux, valorOtroTipoEP, propiedadOtroTipoEP);

                }
            }
        }

        /// <summary>
        /// 060.030.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetSociedadesAsociaciones(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoSociedadesAsociaciones = listadoDatos.Where(x => x.Code.Equals("060.030.020.000")).ToList();
            if (listadoSociedadesAsociaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoSociedadesAsociaciones)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_SOCIEDADES_ASOCIACIONES";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.020.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.020.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesNombre, item.GetStringPorIDCampo("060.030.020.010")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesPaisRadicacion, item.GetPaisPorIDCampo("060.030.020.020")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCCAARadicacion, item.GetRegionPorIDCampo("060.030.020.030")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCiudadRadicacion, item.GetStringPorIDCampo("060.030.020.180")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.030.020.150")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.030.020.160")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.030.020.170")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCategoriaProfesional, item.GetStringPorIDCampo("060.030.020.100")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesTamanio, item.GetStringDoublePorIDCampo("060.030.020.110")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesFechaInicio, item.GetStringDatetimePorIDCampo("060.030.020.120")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.030.020.130"))
                         ));
                        SociedadesAsociacionesEntidadAfiliacion(item, entidadAux);
                        SociedadesAsociacionesPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SociedadesAsociacionesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.030.020.090");

            string propiedadPalabrasClave = Variables.ActividadCientificaTecnologica.sociedadesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void SociedadesAsociacionesEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad de Afiliacion no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.020.050")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.020.050"),
                Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.020.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.020.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.030.020.080"))
            ));
        }

        /// <summary>
        /// 060.030.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetConsejos(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoConsejos = listadoDatos.Where(x => x.Code.Equals("060.030.030.000")).ToList();
            if (listadoConsejos.Count > 0)
            {
                foreach (CvnItemBean item in listadoConsejos)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_CONSEJOS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.030.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.030.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.consejosNombre, item.GetStringPorIDCampo("060.030.030.010")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosPaisRadicacion, item.GetPaisPorIDCampo("060.030.030.020")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCCAARadicacion, item.GetRegionPorIDCampo("060.030.030.030")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCiudadRadicacion, item.GetStringPorIDCampo("060.030.030.160")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.030.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.030.030.180")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.030.030.190")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosTareasDesarrolladas, item.GetStringPorIDCampo("060.030.030.090")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCategoriaProfesional, item.GetStringPorIDCampo("060.030.030.100")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosTamanioSociedad, item.GetStringDoublePorIDCampo("060.030.030.110")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosAmbito, item.GetGeographicRegionPorIDCampo("060.030.030.120")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosAmbitoOtros, item.GetStringPorIDCampo("060.030.030.130")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosFechaInicio, item.GetStringDatetimePorIDCampo("060.030.030.140")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.030.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionMes, item.GetDurationMesPorIDCampo("060.030.030.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionDias, item.GetDurationDiaPorIDCampo("060.030.030.150"))
                        ));
                        ConsejosEntidadAfiliacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ConsejosEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.030.050")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.030.050"),
                Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.030.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.030.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.consejosTipoEntidadAfiliacion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.consejosTipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.030.030.080"))
            ));
        }

        /// <summary>
        /// 060.030.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetRedesCooperacion(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoRedesCooperacion = listadoDatos.Where(x => x.Code.Equals("060.030.040.000")).ToList();
            if (listadoRedesCooperacion.Count > 0)
            {
                foreach (CvnItemBean item in listadoRedesCooperacion)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_REDES_COOPERACION";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.040.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.040.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopNombre, item.GetStringPorIDCampo("060.030.040.010")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopIdentificacion, item.GetStringPorIDCampo("060.030.040.020")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopNumInvestigadores, item.GetStringDoublePorIDCampo("060.030.040.030")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopPaisRadicacion, item.GetPaisPorIDCampo("060.030.040.040")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCCAARadicacion, item.GetRegionPorIDCampo("060.030.040.050")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCiudadRadicacion, item.GetStringPorIDCampo("060.030.040.180")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopPaisEntidadSeleccion, item.GetPaisPorIDCampo("060.030.040.190")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCCAAEntidadSeleccion, item.GetRegionPorIDCampo("060.030.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCiudadEntidadSeleccion, item.GetStringPorIDCampo("060.030.040.210")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopTareas, item.GetStringPorIDCampo("060.030.040.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopFechaInicio, item.GetStringDatetimePorIDCampo("060.030.040.160")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionMes, item.GetDurationMesPorIDCampo("060.030.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionDias, item.GetDurationDiaPorIDCampo("060.030.040.170"))
                        ));
                        RedesCooperacionEntidadSeleccion(item, entidadAux);
                        RedesCooperacionEntidadesParticipantes(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Selección.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>        
        private void RedesCooperacionEntidadSeleccion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.040.110")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.040.110"),
                Variables.ActividadCientificaTecnologica.redesCoopEntidadSeleccionNombre,
                Variables.ActividadCientificaTecnologica.redesCoopEntidadSeleccion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.040.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.040.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccionOtros, item.GetStringPorIDCampo("060.030.040.140"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de las Entidades Participantes,
        /// pertenecientes al listado <paramref name="listadoEntidadParticipante"/>.
        /// </summary>
        /// <param name="listadoEntidadParticipante">listadoEntidadParticipante</param>
        /// <param name="entidadAux">entidadAux</param>
        private void RedesCooperacionEntidadesParticipantes(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadParticipante = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.030.040.070");

            foreach (CvnItemBeanCvnCodeGroup entidadParticipante in listadoEntidadParticipante)
            {
                if (!string.IsNullOrEmpty(entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.030.040.070")))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                    //Añado Nombre Entidad Participante
                    Property propertyEntidadParticipanteNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipanteNombre);

                    string valorNombreEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.030.040.070"));
                    string propiedadNombreEP = Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipanteNombre;

                    UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorNombreEP, propiedadNombreEP);

                    //Añado referencia Entidad Participante
                    Property propertyEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipante);
                    string nombre = entidadParticipante.CvnEntityBean.Name;
                    if (!string.IsNullOrEmpty(nombre))
                    {
                        string referenciaEP = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombre);
                        string valorEP = (referenciaEP != null) ? UtilitySecciones.StringGNOSSID(entityPartAux, referenciaEP) : entityPartAux + "";
                        string propiedadEP = Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipante;

                        UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorEP, propiedadEP);
                    }

                    //Añado Tipo Entidad Participante
                    Property propertyTipoEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipante);
                    Property propertyTipoEntidadParticipanteOtros = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipanteOtros);

                    string valorOtroTipoEP = UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.030.040.100"));
                    string propiedadOtroTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros;

                    string valorTipoEP = !string.IsNullOrEmpty(valorOtroTipoEP) ? UtilitySecciones.StringGNOSSID(entityPartAux, mResourceApi.GraphsUrl + "items/organizationtype_OTHERS") : UtilitySecciones.StringGNOSSID(entityPartAux, entidadParticipante.GetOrganizationCvnCodeGroup("060.030.040.090"));
                    string propiedadTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad;

                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipante, entidadAux, valorTipoEP, propiedadTipoEP);
                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipanteOtros, entidadAux, valorOtroTipoEP, propiedadOtroTipoEP);

                }
            }
        }

        /// <summary>
        /// 060.030.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetPremiosMenciones(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPremios = listadoDatos.Where(x => x.Code.Equals("060.030.050.000")).ToList();
            if (listadoPremios.Count > 0)
            {
                foreach (CvnItemBean item in listadoPremios)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_PREMIOS_MENCIONES";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.050.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.050.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesDescripcion, item.GetStringPorIDCampo("060.030.050.010")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesPais, item.GetPaisPorIDCampo("060.030.050.020")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesCCAA, item.GetRegionPorIDCampo("060.030.050.030")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesCiudad, item.GetStringPorIDCampo("060.030.050.110")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesReconocimientosLigados, item.GetStringPorIDCampo("060.030.050.090")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.050.100"))
                        ));
                        PremiosMencionesEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PremiosMencionesEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.050.050")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.050.050"),
                Variables.ActividadCientificaTecnologica.premiosMencionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.premiosMencionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.050.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.050.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.050.080"))
            ));
        }

        /// <summary>
        /// 060.030.060.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrasDistinciones(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDistinciones = listadoDatos.Where(x => x.Code.Equals("060.030.060.000")).ToList();
            if (listadoDistinciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoDistinciones)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_OTRAS_DISTINCIONES";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.060.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.060.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesDescripcion, item.GetStringPorIDCampo("060.030.060.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesPais, item.GetPaisPorIDCampo("060.030.060.020")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesCCAA, item.GetRegionPorIDCampo("060.030.060.030")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesCiudad, item.GetStringPorIDCampo("060.030.060.120")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbito, item.GetGeographicRegionPorIDCampo("060.030.060.090")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbitoOtros, item.GetStringPorIDCampo("060.030.060.100")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.060.110"))
                        ));
                        OtrasDistincionesEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasDistincionesEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.060.050")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.060.050"),
                Variables.ActividadCientificaTecnologica.otrasDistincionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.otrasDistincionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.060.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.060.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.060.080"))
            ));
        }

        /// <summary>
        /// 060.030.070.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetPeriodosActividadInvestigadora(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAcreditaciones = listadoDatos.Where(x => x.Code.Equals("060.030.070.000")).ToList();
            if (listadoAcreditaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoAcreditaciones)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_PERIODOS_ACTIVIDAD_INVESTIGADORA";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringDoublePorIDCampo("060.030.070.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.070.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraNumeroTramos, item.GetStringDoublePorIDCampo("060.030.070.010")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraPaisEntidad, item.GetPaisPorIDCampo("060.030.070.020")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraCCAAEntidad, item.GetRegionPorIDCampo("060.030.070.030")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraCiudadEntidad, item.GetStringPorIDCampo("060.030.070.120")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraAmbito, item.GetGeographicRegionPorIDCampo("060.030.070.090")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraAmbitoOtros, item.GetStringPorIDCampo("060.030.070.100")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraFechaObtencion, item.GetStringDatetimePorIDCampo("060.030.070.110"))
                        ));
                        PeriodosActividadInvestigadoraEntidadAfiliacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Acreditante.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PeriodosActividadInvestigadoraEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.070.050")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.070.050"),
                Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidadNombre,
                Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.070.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.070.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraTipoEntidadOtros, item.GetStringPorIDCampo("060.030.070.080"))
            ));
        }

        /// <summary>
        /// 060.030.090.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetAcreditacionesObtenidas(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAcreditaciones = listadoDatos.Where(x => x.Code.Equals("060.030.090.000")).ToList();
            if (listadoAcreditaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoAcreditaciones)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_ACREDITACIONES_OBTENIDAS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.090.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.090.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesDescripcion, item.GetStringPorIDCampo("060.030.090.010")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesPaisEntidad, item.GetPaisPorIDCampo("060.030.090.020")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesCCAAEntidad, item.GetRegionPorIDCampo("060.030.090.030")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesCiudadEntidad, item.GetStringPorIDCampo("060.030.090.120")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesFechaObtencion, item.GetStringDatetimePorIDCampo("060.030.090.050")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesNumeroTramos, item.GetStringDoublePorIDCampo("060.030.090.100")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesFechaReconocimiento, item.GetStringDatetimePorIDCampo("060.030.090.110"))
                        ));
                        AcreditacionesObtenidasEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Compruebo si existe la entidad en BBDD y en caso de que exista añado la referencia a la misma
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void AcreditacionesObtenidasEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.090.060")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.090.060"),
                Variables.ActividadCientificaTecnologica.acreditacionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.acreditacionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.090.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.090.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.acreditacionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.acreditacionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.090.090"))
            ));
        }


        /// <summary>
        /// 060.030.100.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrosMeritos(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtros = listadoDatos.Where(x => x.Code.Equals("060.030.100.000")).ToList();
            if (listadoOtros.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtros)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_OTROS_MERITOS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();

                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.100.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.100.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTextoLibre, item.GetStringPorIDCampo("060.030.100.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosPaisEntidad, item.GetPaisPorIDCampo("060.030.100.070")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosCCAAEntidad, item.GetRegionPorIDCampo("060.030.100.090")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosCiudadEntidad, item.GetStringPorIDCampo("060.030.100.080")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.100.040"))
                        ));
                        OtrosMeritosEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Compruebo si existe la entidad en BBDD y en caso de que exista añado la referencia a la misma
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrosMeritosEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.100.020")))
            { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.100.020"),
                Variables.ActividadCientificaTecnologica.otrosMeritosEntidadNombre,
                Variables.ActividadCientificaTecnologica.otrosMeritosEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.100.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.100.060");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTipoEntidadOtros, item.GetStringPorIDCampo("060.030.100.050"))
            ));
        }
    }
}
