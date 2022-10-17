using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;
using static Models.Entity;

namespace ImportadorWebCV.Sincro.Secciones
{
    class FormacionAcademica : SeccionBase
    {
        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private List<CvnItemBean> listadoCvn = new List<CvnItemBean>();
        private readonly string RdfTypeTab = "http://w3id.org/roh/Qualifications";
        public FormacionAcademica(cvnRootResultBean cvn, string cvID, string personID, ConfigService configuracion) : base(cvn, cvID, personID, configuracion)
        {
            listadoDatos = mCvn.GetListadoBloque("020");
            listadoCvn = mCvn.cvnRootBean.ToList();
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque 
        /// "Estudios de 1º y 2º ciclo, y antiguos ciclos
        /// (licenciados, Diplomados, Ingenieros Superiores, Ingenieros Técnicos, Arquitectos)".
        /// Con codigo identificativo "020.010.010.000".
        /// </summary>
        public List<SubseccionItem> SincroEstudiosCiclos(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/firstSecondCycles", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "academicdegree";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#AcademicDegree";
            string rdfTypePrefix = "RelatedFirstSecondCycles";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEstudiosCiclos(listadoDatos, petitionStatus);
            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    EstudiosCiclos estudiosCiclos = new EstudiosCiclos();
                    estudiosCiclos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.estudiosCicloNombreTitulo)?.values.FirstOrDefault();
                    estudiosCiclos.entidadTitulacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.estudiosCicloEntidadTitulacionNombre)?.values.FirstOrDefault();
                    estudiosCiclos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.estudiosCicloFechaTitulacion)?.values.FirstOrDefault();
                    estudiosCiclos.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(estudiosCiclos.ID, estudiosCiclos);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = EstudiosCiclos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
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

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque "Doctorados".
        /// Con codigo identificativo "020.010.020.000".
        /// </summary>
        public List<SubseccionItem> SincroDoctorados(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/doctorates", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "academicdegree";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#AcademicDegree";
            string rdfTypePrefix = "RelatedDoctorates";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetDoctorados(listadoDatos, petitionStatus);
            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    Doctorados doctorados = new Doctorados();
                    doctorados.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.doctoradosProgramaDoctoradoNombre)?.values.FirstOrDefault();
                    doctorados.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.doctoradosFechaTitulacion)?.values.FirstOrDefault();
                    doctorados.entidadTitulacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.doctoradosEntidadTitulacionNombre)?.values.FirstOrDefault();
                    doctorados.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(doctorados.ID, doctorados);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = Doctorados.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
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

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque 
        /// "Otra formación universitaria de posgrado Formación especializada".
        /// Con codigo identificativo "020.010.030.000".
        /// </summary>
        public List<SubseccionItem> SincroOtraFormacionPosgrado(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/postgraduates", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "academicdegree";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#AcademicDegree";
            string rdfTypePrefix = "RelatedPostGraduates";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtraFormacionPosgrado(listadoDatos, petitionStatus);
            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado();
                    otraFormacionPosgrado.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.otraFormacionTipoFormacion)?.values.FirstOrDefault();
                    otraFormacionPosgrado.entidadTitulacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.otraFormacionEntidadTitulacionNombre)?.values.FirstOrDefault();
                    otraFormacionPosgrado.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.otraFormacionFechaTitulacion)?.values.FirstOrDefault();
                    otraFormacionPosgrado.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otraFormacionPosgrado.ID, otraFormacionPosgrado);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtraFormacionPosgrado.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
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

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Formación especializada, continuada, técnica, profesionalizada, de reciclaje y actualización".
        /// Con codigo identificativo "020.020.000.000".
        /// </summary>
        public List<SubseccionItem> SincroFormacionEspecializada(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/specialisedTraining", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "academicdegree";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#AcademicDegree";
            string rdfTypePrefix = "RelatedSpecialisedTrainings";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionEspecializada(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    FormacionEspecializada formacionEspecializada = new FormacionEspecializada();
                    formacionEspecializada.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.formacionEspeTituloFormacion)?.values.FirstOrDefault();
                    formacionEspecializada.entidadTitulacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.formacionEspeEntidadTitulacionNombre)?.values.FirstOrDefault();
                    formacionEspecializada.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.formacionEspeFechaFinalizacion)?.values.FirstOrDefault();
                    formacionEspecializada.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(formacionEspecializada.ID, formacionEspecializada);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = FormacionEspecializada.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
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

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Cursos y semin. mejora docente".
        /// Con codigo identificativo "020.050.000.000".
        /// </summary>
        public List<SubseccionItem> SincroCursosMejoraDocente(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/coursesAndSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "academicdegree";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#AcademicDegree";
            string rdfTypePrefix = "RelatedCoursesAndSeminars";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCursosMejoraDocente(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    CursosMejoraDocente cursosMejoraDocente = new CursosMejoraDocente();
                    cursosMejoraDocente.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.cursosSeminariosTitulo)?.values.FirstOrDefault();
                    cursosMejoraDocente.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.cursosSeminariosFechaInicio)?.values.FirstOrDefault();
                    cursosMejoraDocente.entidadOrganizadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadoraNombre)?.values.FirstOrDefault();
                    cursosMejoraDocente.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(cursosMejoraDocente.ID, cursosMejoraDocente);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = CursosMejoraDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
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

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Conocimiento de idiomas".
        /// Con codigo identificativo "020.060.000.000".
        /// </summary>
        public List<SubseccionItem> SincroConocimientoIdiomas(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications", "http://w3id.org/roh/languageSkills", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "languagecertificate";
            string propTitle = "http://w3id.org/roh/languageOfTheCertificate";
            string rdfType = "http://w3id.org/roh/LanguageCertificate";
            string rdfTypePrefix = "RelatedLanguageSkills";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetConocimientoIdiomas(listadoDatos, preimportar, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas();
                    conocimientoIdiomas.idioma = entityXML.properties.FirstOrDefault(x => x.prop == Variables.FormacionAcademica.conocimientoIdiomasIdioma)?.values.FirstOrDefault();
                    conocimientoIdiomas.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(conocimientoIdiomas.ID, conocimientoIdiomas);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ConocimientoIdiomas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
            
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD:listadoIdBBDD);
        }

        /// <summary>
        /// 020.010.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEstudiosCiclos(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEstudiosCiclos = listadoDatos.Where(x => x.Code.Equals("020.010.010.000")).ToList();
            if (listadoEstudiosCiclos.Count > 0)
            {
                foreach (CvnItemBean item in listadoEstudiosCiclos)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_ESTUDIOS_CICLOS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetNameTitleBeanPorIDCampo("020.010.010.030")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "020.010.010.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.FormacionAcademica.estudiosCicloPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.010.050")),
                            new Property(Variables.FormacionAcademica.estudiosCicloCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.010.060")),
                            new Property(Variables.FormacionAcademica.estudiosCicloCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.010.080")),
                            new Property(Variables.FormacionAcademica.estudiosCicloFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.010.130")),
                            new Property(Variables.FormacionAcademica.estudiosCicloNotaMedia, item.GetNotaMediaPorIDCampo("020.010.010.140")),
                            new Property(Variables.FormacionAcademica.estudiosCicloFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.010.170")),
                            new Property(Variables.FormacionAcademica.estudiosCicloTituloHomologado, item.GetStringBooleanPorIDCampo("020.010.010.180"))
                        ));
                        EstudiosCiclosTipoTitulacion(item, entidadAux);
                        EstudiosCiclosPremio(item, entidadAux);
                        EstudiosCiclosTitulacion(item, entidadAux);
                        EstudiosCiclosTituloExtrajero(item, entidadAux);
                        EstudiosCiclosEntidadTitulacion(item, entidadAux);

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
        /// pertenecientes al Tipo de titulación.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstudiosCiclosTipoTitulacion(CvnItemBean item, Entity entidadAux)
        {
            if (item.GetTipoGradoUniversitarioPorIDCampo("020.010.010.010") != null)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.estudiosCicloTipoTitulacion, item.GetTipoGradoUniversitarioPorIDCampo("020.010.010.010")),
                    new Property(Variables.FormacionAcademica.estudiosCicloTipoTitulacionOtros, item.GetStringPorIDCampo("020.010.010.020"))
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Premio.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstudiosCiclosPremio(CvnItemBean item, Entity entidadAux)
        {
            if (item.GetPremioPorIDCampo("020.010.010.190") != null)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.estudiosCicloPremio, item.GetPremioPorIDCampo("020.010.010.190")),
                    new Property(Variables.FormacionAcademica.estudiosCicloPremioOtros, item.GetStringPorIDCampo("020.010.010.200"))
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Título.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstudiosCiclosTitulacion(CvnItemBean item, Entity entidadAux)
        {
            Utility.AniadirTitulacion(mResourceApi, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.010.030"),
                Variables.FormacionAcademica.estudiosCicloNombreTitulo,
                Variables.FormacionAcademica.estudiosCicloTitulo, entidadAux);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Título extrajero.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstudiosCiclosTituloExtrajero(CvnItemBean item, Entity entidadAux)
        {
            Utility.AniadirTitulacion(mResourceApi, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.010.150"),
                Variables.FormacionAcademica.estudiosCicloTituloExtranjeroNombre,
                Variables.FormacionAcademica.estudiosCicloTituloExtranjero, entidadAux);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Titulación.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstudiosCiclosEntidadTitulacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad de Titulacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.010.010.090"),
                Variables.FormacionAcademica.estudiosCicloEntidadTitulacionNombre,
                Variables.FormacionAcademica.estudiosCicloEntidadTitulacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("020.010.010.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("020.010.010.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacion, valorTipo),
                new Property(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.010.120"))
            ));
        }

        /// <summary>
        /// 020.010.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetDoctorados(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDoctorados = listadoDatos.Where(x => x.Code.Equals("020.010.020.000")).ToList();
            if (listadoDoctorados.Count > 0)
            {
                foreach (CvnItemBean item in listadoDoctorados)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_DOCTORADOS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetNameTitleBeanPorIDCampo("020.010.020.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "020.010.020.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.FormacionAcademica.doctoradosFechaObtencionDEA, item.GetStringDatetimePorIDCampo("020.010.020.050")),
                            new Property(Variables.FormacionAcademica.doctoradosPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.020.060")),
                            new Property(Variables.FormacionAcademica.doctoradosCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.020.070")),
                            new Property(Variables.FormacionAcademica.doctoradosCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.020.090")),
                            new Property(Variables.FormacionAcademica.doctoradosFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.020.140")),
                            new Property(Variables.FormacionAcademica.doctoradosTituloTesis, item.GetStringPorIDCampo("020.010.020.160")),
                            new Property(Variables.FormacionAcademica.doctoradosCalificacionObtenida, item.GetStringPorIDCampo("020.010.020.190")),
                            new Property(Variables.FormacionAcademica.doctoradosMencionCalidad, item.GetStringBooleanPorIDCampo("020.010.020.210"))
                        ));
                        DoctoradosProgramaDoctorado(item, entidadAux);
                        DoctoradosEntidadTitulacion(item, entidadAux);
                        DoctoradosEntidadTitulacionDEA(item, entidadAux);
                        DoctoradosDirectorTesis(item, entidadAux);
                        DoctoradosCodirectoresTesis(item, entidadAux);
                        DoctoradosDoctoradoUE(item, entidadAux);
                        DoctoradosPremioExtraordinario(item, entidadAux);
                        DoctoradosTituloHomologado(item, entidadAux);

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
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosProgramaDoctorado(CvnItemBean item, Entity entidadAux)
        {
            string programaDoctorado = item.GetNameTitleBeanPorIDCampo("020.010.020.010");
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.doctoradosProgramaDoctoradoNombre, programaDoctorado),
                new Property(Variables.FormacionAcademica.doctoradosProgramaDoctorado, Utility.ReferenciaProgramaDoctorado(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.020.010")))
            ));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosDoctoradoUE(CvnItemBean item, Entity entidadAux)
        {
            string tituloDoctoradoUE = item.GetStringBooleanPorIDCampo("020.010.020.200");
            if (!string.IsNullOrEmpty(tituloDoctoradoUE))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.doctoradosDoctoradoUE, tituloDoctoradoUE),
                    new Property(Variables.FormacionAcademica.doctoradosFechaMencionDocUE, item.GetStringDatetimePorIDCampo("020.010.020.150"))
                ));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosPremioExtraordinario(CvnItemBean item, Entity entidadAux)
        {
            string tituloPremioExtraordinario = item.GetStringBooleanPorIDCampo("020.010.020.220");
            if (!string.IsNullOrEmpty(tituloPremioExtraordinario))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.doctoradosPremioExtraordinario, tituloPremioExtraordinario),
                    new Property(Variables.FormacionAcademica.doctoradosFechaObtencion, item.GetStringDatetimePorIDCampo("020.010.020.230"))
                ));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosTituloHomologado(CvnItemBean item, Entity entidadAux)
        {
            string tituloHomologado = item.GetStringBooleanPorIDCampo("020.010.020.240");
            if (!string.IsNullOrEmpty(tituloHomologado))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.doctoradosTituloHomologado, tituloHomologado),
                    new Property(Variables.FormacionAcademica.doctoradosFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.020.250"))
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al director de tesis.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosDirectorTesis(CvnItemBean item, Entity entidadAux)
        {
            CvnItemBeanCvnAuthorBean director = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("020.010.020.170");

            //Si el director es null no lo añado
            if (director == null) { return; }
            if (string.IsNullOrEmpty(director.GetNombreAutor())) { return; }

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.doctoradosDirectorTesisFirma, director.GetFirmaAutor()),
                new Property(Variables.FormacionAcademica.doctoradosDirectorTesisNombre, director.GetNombreAutor()),
                new Property(Variables.FormacionAcademica.doctoradosDirectorTesisPrimerApellido, director.GetPrimerApellidoAutor()),
                new Property(Variables.FormacionAcademica.doctoradosDirectorTesisSegundoApellido, director.GetSegundoApellidoAutor())
            ));

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los codirectores de la tesis.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosCodirectoresTesis(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoCodirectores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("020.010.020.180");

            string propiedadCodirectorTesisOrden = Variables.FormacionAcademica.doctoradosCodirectorTesisOrden;
            string propiedadCodirectorTesisFirma = Variables.FormacionAcademica.doctoradosCodirectorTesisFirma;
            string propiedadCodirectorTesisNombre = Variables.FormacionAcademica.doctoradosCodirectorTesisNombre;
            string propiedadCodirectorTesisPrimerApellido = Variables.FormacionAcademica.doctoradosCodirectorTesisPrimerApellido;
            string propiedadCodirectorTesisSegundoApellido = Variables.FormacionAcademica.doctoradosCodirectorTesisSegundoApellido;

            foreach (CvnItemBeanCvnAuthorBean codirector in listadoCodirectores)
            {
                //Si no tiene nombre continuo con el siguiente
                if (string.IsNullOrEmpty(codirector.GetNombreAutor())) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                Property propertyCodirectorTesisOrden = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodirectorTesisOrden);
                Property propertyCodirectorTesisFirma = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodirectorTesisFirma);
                Property propertyCodirectorTesisNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodirectorTesisNombre);
                Property propertyCodirectorTesisPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodirectorTesisPrimerApellido);
                Property propertyCodirectorTesisSegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodirectorTesisSegundoApellido);

                UtilitySecciones.CheckProperty(propertyCodirectorTesisOrden, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, codirector.GetOrdenAutor()), propiedadCodirectorTesisOrden);
                UtilitySecciones.CheckProperty(propertyCodirectorTesisFirma, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, codirector.GetFirmaAutor()), propiedadCodirectorTesisFirma);
                UtilitySecciones.CheckProperty(propertyCodirectorTesisNombre, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, codirector.GetNombreAutor()), propiedadCodirectorTesisNombre);
                UtilitySecciones.CheckProperty(propertyCodirectorTesisPrimerApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, codirector.GetPrimerApellidoAutor()), propiedadCodirectorTesisPrimerApellido);
                UtilitySecciones.CheckProperty(propertyCodirectorTesisSegundoApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, codirector.GetSegundoApellidoAutor()), propiedadCodirectorTesisSegundoApellido);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de titulación en la qu eobtuvo el Diploma de Estudios Avanzados.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosEntidadTitulacionDEA(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.FormacionAcademica.doctoradosEntidadTitulacionDEA, item.GetNameEntityBeanPorIDCampo("020.010.020.040"))
            */
            //Añado la referencia si existe Entidad de Titulacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.010.020.040"),
                Variables.FormacionAcademica.doctoradosEntidadTitulacionDEANombre,
                Variables.FormacionAcademica.doctoradosEntidadTitulacionDEA, entidadAux);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de titulación.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DoctoradosEntidadTitulacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad de Titulacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.010.020.100"),
                Variables.FormacionAcademica.doctoradosEntidadTitulacionNombre,
                Variables.FormacionAcademica.doctoradosEntidadTitulacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("020.010.020.130")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("020.010.020.120");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacion, valorTipo),
                new Property(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.020.130"))
            ));
        }

        /// <summary>
        /// 020.010.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetOtraFormacionPosgrado(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtraFormacionPosgrado = listadoDatos.Where(x => x.Code.Equals("020.010.030.000")).ToList();
            if (listadoOtraFormacionPosgrado.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtraFormacionPosgrado)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_OTRA_FORMACION_POSGRADO";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.030.020")?.Name))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "020.010.030.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.FormacionAcademica.otraFormacionTipoFormacion, item.GetTipoFormacion("020.010.030.010")),
                            new Property(Variables.FormacionAcademica.otraFormacionPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.030.040")),
                            new Property(Variables.FormacionAcademica.otraFormacionCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.030.050")),
                            new Property(Variables.FormacionAcademica.otraFormacionCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.030.070")),
                            new Property(Variables.FormacionAcademica.otraFormacionFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.030.120")),
                            new Property(Variables.FormacionAcademica.otraFormacionCalificacionObtenida, item.GetStringPorIDCampo("020.010.030.130")),
                            new Property(Variables.FormacionAcademica.otraFormacionTituloHomologado, item.GetStringBooleanPorIDCampo("020.010.030.150")),
                            new Property(Variables.FormacionAcademica.otraFormacionFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.030.160"))
                        ));
                        OtraFormacionPosgradoTitulacionPosgrado(item, entidadAux);
                        OtraFormacionPosgradoEntidadTitulacion(item, entidadAux);

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
        /// pertenecientes al Titulo de posgrado.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtraFormacionPosgradoTitulacionPosgrado(CvnItemBean item, Entity entidadAux)
        {
            if (!string.IsNullOrEmpty(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.030.020")?.Name))
            {
                string valor = "";
                if (!string.IsNullOrEmpty(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.030.020")?.Identification))
                {
                    valor = mResourceApi.GraphsUrl + "items/postgradedegree_" + item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.030.020")?.Identification;
                }

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.FormacionAcademica.otraFormacionTituloPosgrado, valor),
                    new Property(Variables.FormacionAcademica.otraFormacionTituloPosgradoNombre, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.030.020")?.Name)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Titulación(ademas de Facultad, instituto, centro).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtraFormacionPosgradoEntidadTitulacion(CvnItemBean item, Entity entidadAux)
        {
            //Si es nulo la entidad de titulacion no añado nada
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("020.010.030.080"))) { return; }

            //Añado la referencia si existe Entidad de Titulacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.010.030.080"),
                Variables.FormacionAcademica.otraFormacionEntidadTitulacionNombre,
                Variables.FormacionAcademica.otraFormacionEntidadTitulacion, entidadAux);

            //Añado Facultad, instituto, centro
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.otraFormacionFacultadEscuela, item.GetNameEntityBeanPorIDCampo("020.010.030.140"))
            ));

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("020.010.030.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("020.010.030.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacion, valorTipo),
                new Property(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.030.110"))
            ));
        }

        /// <summary>
        /// 020.020.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetFormacionEspecializada(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionEspecializada = listadoDatos.Where(x => x.Code.Equals("020.020.000.000")).ToList();
            if (listadoFormacionEspecializada.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionEspecializada)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_FORMACION_ESPECIALIZADA";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.020.000.030")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "020.020.000.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.FormacionAcademica.formacionEspeTipoFormacion, item.GetTipoFormacionActividad("020.020.000.010")),
                            new Property(Variables.FormacionAcademica.formacionEspeTipoFormacionOtros, item.GetStringPorIDCampo("020.020.000.020")),
                            new Property(Variables.FormacionAcademica.formacionEspeTituloFormacion, item.GetStringPorIDCampo("020.020.000.030")),
                            new Property(Variables.FormacionAcademica.formacionEspePaisEntidadTitulacion, item.GetPaisPorIDCampo("020.020.000.040")),
                            new Property(Variables.FormacionAcademica.formacionEspeCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.020.000.050")),
                            new Property(Variables.FormacionAcademica.formacionEspeCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.020.000.070")),
                            new Property(Variables.FormacionAcademica.formacionEspeObjetivosEntidad, item.GetStringPorIDCampo("020.020.000.120")),
                            new Property(Variables.FormacionAcademica.formacionEspeDuracionHoras, item.GetDurationHorasPorIDCampo("020.020.000.140")),
                            new Property(Variables.FormacionAcademica.formacionEspeFechaFinalizacion, item.GetStringDatetimePorIDCampo("020.020.000.150"))
                        ));
                        FormacionEspecializadaEntidadTitulacion(item, entidadAux);
                        FormacionEspecializadaResponsable(item, entidadAux);

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
        /// pertenecientes al Responsable.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void FormacionEspecializadaResponsable(CvnItemBean item, Entity entidadAux)
        {
            CvnItemBeanCvnAuthorBean autor = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("020.020.000.130");
            //Compruebo que el autor no es nulo.
            if (autor == null) { return; }

            //Añado el autor.
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.formacionEspeResponsableFirma, autor.GetFirmaAutor()),
                new Property(Variables.FormacionAcademica.formacionEspeResponsableNombre, autor.GetNombreAutor()),
                new Property(Variables.FormacionAcademica.formacionEspeResponsablePrimerApellido, autor.GetPrimerApellidoAutor()),
                new Property(Variables.FormacionAcademica.formacionEspeResponsableSegundoApellido, autor.GetSegundoApellidoAutor())
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Titulación(ademas de Facultad, instituto, centro).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void FormacionEspecializadaEntidadTitulacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad de Titulacion
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.020.000.080"),
                Variables.FormacionAcademica.formacionEspeEntidadTitulacionNombre,
                Variables.FormacionAcademica.formacionEspeEntidadTitulacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("020.020.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("020.020.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacion, valorTipo),
                new Property(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.020.000.110"))
            ));
        }


        /// <summary>
        /// 020.050.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetCursosMejoraDocente(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCursosMejoraDocente = listadoDatos.Where(x => x.Code.Equals("020.050.000.000")).ToList();
            if (listadoCursosMejoraDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoCursosMejoraDocente)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_CURSOS_MEJORA_DOCENTE";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.050.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "020.050.000.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.FormacionAcademica.cursosSeminariosTitulo, item.GetStringPorIDCampo("020.050.000.010")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivos, item.GetStringPorIDCampo("020.050.000.020")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("020.050.000.030")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("020.050.000.040")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("020.050.000.060")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosDuracionHoras, item.GetDurationHorasPorIDCampo("020.050.000.110")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosFechaFinal, item.GetStringDatetimePorIDCampo("020.050.000.120")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosPerfilDestinatarios, item.GetStringPorIDCampo("020.050.000.130")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosFechaInicio, item.GetStringDatetimePorIDCampo("020.050.000.140")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesAnio, item.GetDurationAnioPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesMes, item.GetDurationMesPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesDia, item.GetDurationDiaPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosProgramaFinanciacion, item.GetStringPorIDCampo("020.050.000.180")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosTareasContrastables, item.GetStringPorIDCampo("020.050.000.190")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivoEstancia, item.GetObjetivoPorIDCampo("020.050.000.200")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivoEstanciaOtros, item.GetStringPorIDCampo("020.050.000.210"))
                        ));
                        CursosSeminariosEntidadOrganizadora(item, entidadAux);

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
        /// pertenecientes al tipo de Entidad de Organizadora (ademas de Facultad, instituto, centro).        
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        public void CursosSeminariosEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("020.050.000.070"))) { return; }

            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("020.050.000.070"),
                Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadoraNombre,
                Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadora, entidadAux);

            //Añado Facultad, escuela, centro
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.cursosSeminariosFacultadEscuela, item.GetNameEntityBeanPorIDCampo("020.050.000.150"))
            ));

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("020.050.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("020.050.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("020.050.000.100"))
            ));
        }

        /// <summary>
        /// 020.060.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetConocimientoIdiomas(List<CvnItemBean> listadoDatos, [Optional] bool preimportar, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoConocimientoIdiomas = listadoDatos.Where(x => x.Code.Equals("020.060.000.000")).ToList();
            if (listadoConocimientoIdiomas.Count > 0)
            {
                foreach (CvnItemBean item in listadoConocimientoIdiomas)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "IMPORTACION_CONOCIMIENTO_IDIOMAS";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    CvnItemBeanCvnTitleBean idioma = item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.060.000.010");

                    //Si el TitleBean es nulo o no tiene identificador, no hago nada. 
                    if (idioma == null) 
                    {
                        listadoDatos.Remove(item);
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                        continue;
                    }
                    if (!string.IsNullOrEmpty(idioma.GetTraduccion()))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "020.060.000.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        if (preimportar)
                        {
                            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasIdioma, idioma.GetTraduccion()),
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionAuditiva, Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.120"))),
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionLectura, Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.130"))),
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasInteraccionOral, Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.140"))),
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionOral, Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.150"))),
                                new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionEscrita, Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.160")))
                            ));
                        }
                        else {
                            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasIdioma.Split("@@@").First(), idioma.GetTraduccion()),
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionAuditiva.Split("@@@").First(), Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.120"))),
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionLectura.Split("@@@").First(), Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.130"))),
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasInteraccionOral.Split("@@@").First(), Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.140"))),
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionOral.Split("@@@").First(), Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.150"))),
                                    new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionEscrita.Split("@@@").First(), Utility.GetNivelLenguaje(item.GetStringPorIDCampo("020.060.000.160")))
                                ));
                        }

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

    }
}
