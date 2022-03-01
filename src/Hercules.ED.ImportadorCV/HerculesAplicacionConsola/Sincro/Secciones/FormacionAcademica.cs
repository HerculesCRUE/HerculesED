using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Models.Entity;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    class FormacionAcademica : SeccionBase
    {
        /// <summary>
        /// Añade en la lista de propiedades de la entidad las propiedades en las 
        /// que los valores no son nulos, en caso de que los valores sean nulos se omite
        /// dicha propiedad.
        /// </summary>
        /// <param name="list"></param>
        private List<Property> AddProperty(params Property[] list)
        {
            List<Property> listado = new List<Property>();
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i].values[0]))
                {
                    listado.Add(list[i]);
                }
            }
            return listado;
        }

        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        public FormacionAcademica(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("020");
        }

        /// <summary>
        /// Dada una cadena de GUID concatenados y finalizando en "|" y un string en caso de que 
        /// el string no sea nulo los concatena, sino devuelve null.
        /// </summary>
        /// <param name="entityAux">GUID concatenado con "|"</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>String de concatenar los parametros, o nulo si el valor es vacio</returns>
        private string StringGNOSSID(string entityAux, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                return entityAux + valor;
            }
            return null;
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque 
        /// "Estudios de 1º y 2º ciclo, y antiguos ciclos
        /// (licenciados, Diplomados, Ingenieros Superiores, Ingenieros Técnicos, Arquitectos)".
        /// Con codigo identificativo "020.010.010.000".
        /// </summary>
        public void SincroEstudiosCiclos()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEstudiosCiclos(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque "Doctorados".
        /// Con codigo identificativo "020.010.020.000".
        /// </summary>
        public void SincroDoctorados()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetDoctorados(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al bloque 
        /// "Otra formación universitaria de posgrado Formación especializada".
        /// Con codigo identificativo "020.010.030.000".
        /// </summary>
        public void SincroOtraFormacionPosgrado()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtraFormacionPosgrado(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Formación especializada, continuada, técnica, profesionalizada, de reciclaje y actualización".
        /// Con codigo identificativo "020.020.000.000".
        /// </summary>
        public void SincroFormacionEspecializada()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionEspecializada(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Formación sanitaria especializada".
        /// Con codigo identificativo "020.030.000.000".
        /// </summary>
        public void SincroFormacionSanitaria()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionSanitaria(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Formación sanitaria en I+D".
        /// Con codigo identificativo "020.040.000.000".
        /// </summary>
        public void SincroFormacionSanitariaIMasD()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionSanitariaIMasD(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Cursos y semin. mejora docente".
        /// Con codigo identificativo "020.050.000.000".
        /// </summary>
        public void SincroCursosMejoraDocente()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCursosMejoraDocente(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al subapartado 
        /// "Conocimiento de idiomas".
        /// Con codigo identificativo "020.060.000.000".
        /// </summary>
        public void SincroConocimientoIdiomas()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetConocimientoIdiomas(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 020.010.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEstudiosCiclos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEstudiosCiclos = listadoDatos.Where(x => x.Code.Equals("020.010.010.000")).ToList();
            if (listadoEstudiosCiclos.Count > 0)
            {
                foreach (CvnItemBean item in listadoEstudiosCiclos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.010.010.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.estudiosCicloTitulacion, item.GetStringPorIDCampo("020.010.010.010")),//TODO
                            new Property(Variables.FormacionAcademica.estudiosCicloTitulacionOtros, item.GetStringPorIDCampo("020.010.010.020")),
                            new Property(Variables.FormacionAcademica.estudiosCicloNombreTitulo, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.010.030")?.Name),
                            new Property(Variables.FormacionAcademica.estudiosCicloPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.010.050")),
                            new Property(Variables.FormacionAcademica.estudiosCicloCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.010.060")),
                            new Property(Variables.FormacionAcademica.estudiosCicloCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.010.080")),
                            new Property(Variables.FormacionAcademica.estudiosCicloEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.010.010.090")),
                            new Property(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacion, item.GetStringPorIDCampo("020.010.010.110")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.estudiosCicloTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.010.120")),//TODO
                            new Property(Variables.FormacionAcademica.estudiosCicloFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.010.130")),
                            new Property(Variables.FormacionAcademica.estudiosCicloNotaMedia, item.GetStringPorIDCampo("020.010.010.140")),//TODO
                            new Property(Variables.FormacionAcademica.estudiosCicloTituloExtranjero, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.010.150")?.Name),
                            new Property(Variables.FormacionAcademica.estudiosCicloFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.010.170")),
                            new Property(Variables.FormacionAcademica.estudiosCicloTituloHomologado, item.GetStringBooleanPorIDCampo("020.010.010.180")),
                            new Property(Variables.FormacionAcademica.estudiosCicloPremio, item.GetStringPorIDCampo("020.010.010.190")),//TODO
                            new Property(Variables.FormacionAcademica.estudiosCicloPremioOtros, item.GetStringPorIDCampo("020.010.010.200"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.010.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetDoctorados(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDoctorados = listadoDatos.Where(x => x.Code.Equals("020.010.020.000")).ToList();
            if (listadoDoctorados.Count > 0)
            {
                foreach (CvnItemBean item in listadoDoctorados)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.020.010")?.Name))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.doctoradosProgramaDoctorado, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.010.020.010")?.Name),
                            new Property(Variables.FormacionAcademica.doctoradosEntidadTitulacionDEA, item.GetNameEntityBeanPorIDCampo("020.010.020.040")),
                            new Property(Variables.FormacionAcademica.doctoradosFechaObtencionDEA, item.GetStringDatetimePorIDCampo("020.010.020.050")),
                            new Property(Variables.FormacionAcademica.doctoradosPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.020.060")),
                            new Property(Variables.FormacionAcademica.doctoradosCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.020.070")),
                            new Property(Variables.FormacionAcademica.doctoradosCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.020.090")),
                            new Property(Variables.FormacionAcademica.doctoradosEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.010.020.100")),
                            new Property(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacion, item.GetStringPorIDCampo("020.010.020.120")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.020.130")),//TODO
                            new Property(Variables.FormacionAcademica.doctoradosFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.020.140")),
                            new Property(Variables.FormacionAcademica.doctoradosFechaMencionDocUE, item.GetStringDatetimePorIDCampo("020.010.020.150")),
                            new Property(Variables.FormacionAcademica.doctoradosTituloTesis, item.GetStringPorIDCampo("020.010.020.160")),
                            new Property(Variables.FormacionAcademica.doctoradosDirectorTesis, item.GetStringPorIDCampo("020.010.020.170")),//TODO - familybean
                            new Property(Variables.FormacionAcademica.doctoradosCodirectorTesis, item.GetStringPorIDCampo("020.010.020.180")),//rep//TODO -autor
                            new Property(Variables.FormacionAcademica.doctoradosCalificacionObtenida, item.GetStringPorIDCampo("020.010.020.190")),
                            new Property(Variables.FormacionAcademica.doctoradosDoctoradoUE, item.GetStringBooleanPorIDCampo("020.010.020.200")),
                            new Property(Variables.FormacionAcademica.doctoradosMencionCalidad, item.GetStringBooleanPorIDCampo("020.010.020.210")),
                            new Property(Variables.FormacionAcademica.doctoradosPremioExtraordinario, item.GetStringBooleanPorIDCampo("020.010.020.220")),
                            new Property(Variables.FormacionAcademica.doctoradosFechaObtencion, item.GetStringDatetimePorIDCampo("020.010.020.230")),
                            new Property(Variables.FormacionAcademica.doctoradosTituloHomologado, item.GetStringBooleanPorIDCampo("020.010.020.240")),
                            new Property(Variables.FormacionAcademica.doctoradosFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.020.250"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.010.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetOtraFormacionPosgrado(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtraFormacionPosgrado = listadoDatos.Where(x => x.Code.Equals("020.010.030.000")).ToList();
            if (listadoOtraFormacionPosgrado.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtraFormacionPosgrado)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.010.030.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.otraFormacionTipoFormacion, item.GetStringPorIDCampo("020.010.030.010")),//TODO
                            new Property(Variables.FormacionAcademica.otraFormacionTituloPosgrado, item.GetNameEntityBeanPorIDCampo("020.010.030.020")),
                            new Property(Variables.FormacionAcademica.otraFormacionPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.010.030.040")),
                            new Property(Variables.FormacionAcademica.otraFormacionCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.010.030.050")),
                            new Property(Variables.FormacionAcademica.otraFormacionCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.010.030.070")),
                            new Property(Variables.FormacionAcademica.otraFormacionEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.010.030.080")),
                            new Property(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacion, item.GetStringPorIDCampo("020.010.030.100")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.010.030.110")),//TODO
                            new Property(Variables.FormacionAcademica.otraFormacionFechaTitulacion, item.GetStringDatetimePorIDCampo("020.010.030.120")),
                            new Property(Variables.FormacionAcademica.otraFormacionCalificacionObtenida, item.GetStringPorIDCampo("020.010.030.130")),
                            new Property(Variables.FormacionAcademica.otraFormacionFacultadEscuela, item.GetNameEntityBeanPorIDCampo("020.010.030.140")),
                            new Property(Variables.FormacionAcademica.otraFormacionTituloHomologado, item.GetStringBooleanPorIDCampo("020.010.030.150")),
                            new Property(Variables.FormacionAcademica.otraFormacionFechaHomologacion, item.GetStringDatetimePorIDCampo("020.010.030.160"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.020.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetFormacionEspecializada(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionSanitariaIMasD = listadoDatos.Where(x => x.Code.Equals("020.020.000.000")).ToList();
            if (listadoFormacionSanitariaIMasD.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionSanitariaIMasD)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.020.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.formacionEspeTipoFormacion, item.GetStringPorIDCampo("020.020.000.010")),//TODO
                            new Property(Variables.FormacionAcademica.formacionEspeTipoFormacionOtros, item.GetStringPorIDCampo("020.020.000.020")),
                            new Property(Variables.FormacionAcademica.formacionEspeTituloFormacion, item.GetStringPorIDCampo("020.020.000.030")),
                            new Property(Variables.FormacionAcademica.formacionEspePaisEntidadTitulacion, item.GetPaisPorIDCampo("020.020.000.040")),
                            new Property(Variables.FormacionAcademica.formacionEspeCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.020.000.050")),
                            new Property(Variables.FormacionAcademica.formacionEspeCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.020.000.070")),
                            new Property(Variables.FormacionAcademica.formacionEspeEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.020.000.080")),
                            new Property(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacion, item.GetStringPorIDCampo("020.020.000.100")),//TODO
                            new Property(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.020.000.110")),//TODO
                            new Property(Variables.FormacionAcademica.formacionEspeObjetivosEntidad, item.GetStringPorIDCampo("020.020.000.120")),
                            new Property(Variables.FormacionAcademica.formacionEspeNombreResponsable, item.GetStringPorIDCampo("020.020.000.130")),//TODO - funcion familybean
                            new Property(Variables.FormacionAcademica.formacionEspeDuracionHoras, item.GetDurationHorasPorIDCampo("020.020.000.140")),
                            new Property(Variables.FormacionAcademica.formacionEspeFechaFinalizacion, item.GetStringDatetimePorIDCampo("020.020.000.150"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.030.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetFormacionSanitaria(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionSanitariaIMasD = listadoDatos.Where(x => x.Code.Equals("020.030.000.000")).ToList();
            if (listadoFormacionSanitariaIMasD.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionSanitariaIMasD)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.030.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTituloEspe, item.GetStringPorIDCampo("020.030.000.010")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTituloSubespe, item.GetStringPorIDCampo("020.030.000.020")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspePaisEntidadRealizacion, item.GetPaisPorIDCampo("020.030.000.030")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeCCAAEntidadRealizacion, item.GetRegionPorIDCampo("020.030.000.040")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeCiudadEntidadRealizacion, item.GetStringPorIDCampo("020.030.000.060")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("020.030.000.070")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTipoEntidadRealizacion, item.GetStringPorIDCampo("020.030.000.090")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("020.030.000.100")),//TODO
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.030.000.110")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspePaisEntidadTitulacion, item.GetPaisPorIDCampo("020.030.000.190")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.030.000.200")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.030.000.210")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTipoEntidadTitulacion, item.GetStringPorIDCampo("020.030.000.130")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.030.000.140")),//TODO
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeFechaInicio, item.GetStringDatetimePorIDCampo("020.030.000.150")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeFechaFinal, item.GetStringDatetimePorIDCampo("020.030.000.160")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspePermanenciaAnio, item.GetDurationAnioPorIDCampo("020.030.000.170")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspePermanenciaMes, item.GetDurationMesPorIDCampo("020.030.000.170")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspePermanenciaDia, item.GetDurationDiaPorIDCampo("020.030.000.170")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaEspeFechaConvalidacion, item.GetStringDatetimePorIDCampo("020.030.000.180"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.040.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetFormacionSanitariaIMasD(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionSanitariaIMasD = listadoDatos.Where(x => x.Code.Equals("020.040.000.000")).ToList();
            if (listadoFormacionSanitariaIMasD.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionSanitariaIMasD)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.040.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDNombre, item.GetStringPorIDCampo("020.040.000.010")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDPaisEntidadTitulacion, item.GetPaisPorIDCampo("020.040.000.020")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCCAAEntidadTitulacion, item.GetRegionPorIDCampo("020.040.000.030")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCiudadEntidadTitulacion, item.GetStringPorIDCampo("020.040.000.050")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("020.040.000.060")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDTipoEntidadRealizacion, item.GetStringPorIDCampo("020.040.000.080")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("020.040.000.090")),//TODO
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDPaisEntidadRealizacion, item.GetPaisPorIDCampo("020.040.000.220")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCCAAEntidadRealizacion, item.GetRegionPorIDCampo("020.040.000.230")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCiudadEntidadRealizacion, item.GetStringPorIDCampo("020.040.000.240")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("020.040.000.100")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDTipoEntidadTitulacion, item.GetStringPorIDCampo("020.040.000.120")),//TODO - funcion other
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("020.040.000.130")),//TODO
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDDepartamento, item.GetStringPorIDCampo("020.040.000.140")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDServicio, item.GetStringPorIDCampo("020.040.000.150")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDSeccion, item.GetStringPorIDCampo("020.040.000.160")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDUnidad, item.GetStringPorIDCampo("020.040.000.170")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDDuracionAnio, item.GetDurationAnioPorIDCampo("020.040.000.180")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDDuracionMes, item.GetDurationMesPorIDCampo("020.040.000.180")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDDuracionDia, item.GetDurationDiaPorIDCampo("020.040.000.180")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDFechaInicio, item.GetStringDatetimePorIDCampo("020.040.000.190")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDFechaFinal, item.GetStringDatetimePorIDCampo("020.040.000.200")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCalificacionObtenida, item.GetStringPorIDCampo("020.040.000.210")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDInteresDocencia, item.GetStringPorIDCampo("020.040.000.250")),
                            new Property(Variables.FormacionAcademica.formacionSanitariaIDCategoriaProfesional, item.GetStringPorIDCampo("020.040.000.260"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        /// <summary>
        /// 020.050.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetCursosMejoraDocente(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCursosMejoraDocente = listadoDatos.Where(x => x.Code.Equals("020.050.000.000")).ToList();
            if (listadoCursosMejoraDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoCursosMejoraDocente)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("020.050.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.cursosSeminariosTitulo, item.GetStringPorIDCampo("020.050.000.010")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivos, item.GetStringPorIDCampo("020.050.000.020")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("020.050.000.030")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("020.050.000.040")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("020.050.000.060")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("020.050.000.070")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadora, item.GetStringPorIDCampo("020.050.000.090")),//TODO - funcion others
                            new Property(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("020.050.000.100")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosDuracionHoras, item.GetDurationHorasPorIDCampo("020.050.000.110")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosFechaFinal, item.GetStringDatetimePorIDCampo("020.050.000.120")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosPerfilDestinatarios, item.GetStringPorIDCampo("020.050.000.130")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosFechaInicio, item.GetStringDatetimePorIDCampo("020.050.000.140")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosFacultadEscuela, item.GetNameEntityBeanPorIDCampo("020.050.000.150")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesAnio, item.GetDurationAnioPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesMes, item.GetDurationMesPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosMesesDia, item.GetDurationDiaPorIDCampo("020.050.000.170")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosProgramaFinanciacion, item.GetStringPorIDCampo("020.050.000.180")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosTareasContrastables, item.GetStringPorIDCampo("020.050.000.190")),
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivoEstancia, item.GetStringPorIDCampo("020.050.000.200")),//TODO
                            new Property(Variables.FormacionAcademica.cursosSeminariosObjetivoEstanciaOtros, item.GetStringPorIDCampo("020.050.000.210"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
       
        /// <summary>
        /// 020.060.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetConocimientoIdiomas(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoConocimientoIdiomas = listadoDatos.Where(x => x.Code.Equals("020.060.000.000")).ToList();
            if (listadoConocimientoIdiomas.Count > 0)
            {
                foreach (CvnItemBean item in listadoConocimientoIdiomas)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.060.000.010").Name))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasIdioma, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("020.060.000.010").Name),//TODO
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionAuditiva, item.GetStringPorIDCampo("020.060.000.120")),
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasComprensionLectura, item.GetStringPorIDCampo("020.060.000.130")),
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasInteraccionOral, item.GetStringPorIDCampo("020.060.000.140")),
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionOral, item.GetStringPorIDCampo("020.060.000.150")),
                            new Property(Variables.FormacionAcademica.conocimientoIdiomasExpresionEscrita, item.GetStringPorIDCampo("020.060.000.160"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

    }
}
