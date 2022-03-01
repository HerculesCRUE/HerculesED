using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Entity;
using Utils;
using Models;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    class SituacionProfesional : SeccionBase
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
        public SituacionProfesional(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
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
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Cargos y actividades desempeñados con anterioridad", con codigo identificativo 
        /// "010.010.000.000".
        /// </summary>
        public void SincroSituacionProfesionalActual()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetSituacionProfesionalActual(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Situación profesional actual", con codigo identificativo 
        /// "010.020.000.000".
        /// </summary>
        public void SincroCargosActividades()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCargosActividades(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "http://w3id.org/roh/XXXXXXXX";
                entityXML.ontology = "XXXXXXXXXXXX";
                entityXML.rdfType = "XXXXXXXXXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXXX", "RelatedXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 010.010.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetSituacionProfesionalActual(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoSituacionProfesionalActual = listadoDatos.Where(x => x.Code.Equals("010.010.000.000")).ToList();
            if (listadoSituacionProfesionalActual.Count > 0)
            {
                foreach (CvnItemBean item in listadoSituacionProfesionalActual)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringBooleanPorIDCampo("010.010.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.SituacionProfesional.situacionProfesionalGestionDocente, item.GetStringBooleanPorIDCampo("010.010.000.010")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadora, item.GetNameEntityBeanPorIDCampo("010.010.000.020")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadora, item.GetStringPorIDCampo("010.010.000.040")),//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadoraOtros, item.GetStringPorIDCampo("010.010.000.050")),//TODO - check
                            new Property(Variables.SituacionProfesional.situacionProfesionalFacultadEscuela, item.GetStringPorIDCampo("010.010.000.060")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalDepartamento, item.GetStringPorIDCampo("010.010.000.080")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalCiudadEntidadEmpleadora, item.GetStringPorIDCampo("010.010.000.100")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalPaisEntidadEmpleadora, item.GetPaisPorIDCampo("010.010.000.110")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalCCAAEntidadEmpleadora, item.GetRegionPorIDCampo("010.010.000.120")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalTelefonoFijo, item.GetStringPorIDCampo("010.010.000.140")),//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalFax, item.GetStringPorIDCampo("010.010.000.150")),//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalCorreoElectronico, item.GetStringPorIDCampo("010.010.000.160")),//rep
                            new Property(Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional, item.GetStringPorIDCampo("010.010.000.170")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalFechaInicio, item.GetStringDatetimePorIDCampo("010.010.000.180")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalModalidadContrato, item.GetStringPorIDCampo("010.010.000.190")),//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalModalidadContratoOtros, item.GetStringPorIDCampo("010.010.000.200")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalRegimenDedicacion, item.GetStringPorIDCampo("010.010.000.210")),//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalCodUnescoPrimaria, item.GetStringPorIDCampo("010.010.000.220")),//rep //TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalCodUnescoSecundaria, item.GetStringPorIDCampo("010.010.000.230")),//rep //TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalCodUnescoTerciaria, item.GetStringPorIDCampo("010.010.000.240")),//rep //TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalFuncionesDesempeñadas, item.GetStringPorIDCampo("010.010.000.250")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalPalabrasClave, item.GetStringPorIDCampo("010.010.000.260")),//rep//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalInteresDocencia, item.GetStringPorIDCampo("010.010.000.280")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestion, item.GetGeographicRegionPorIDCampo("010.010.000.290")),//TODO - Check
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.010.000.300"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        } 
        
        /// <summary>
        /// 010.020.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetCargosActividades(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCargosActividades = listadoDatos.Where(x => x.Code.Equals("010.20.000.000")).ToList();
            if (listadoCargosActividades.Count > 0)
            {
                foreach (CvnItemBean item in listadoCargosActividades)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringBooleanPorIDCampo("010.020.000.010")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.SituacionProfesional.cargosActividadesGestionDocente, item.GetStringBooleanPorIDCampo("010.020.000.010")),
                            new Property(Variables.SituacionProfesional.cargosActividadesEntidadEmpleadora, item.GetNameEntityBeanPorIDCampo("010.020.000.020")),
                            new Property(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadora, item.GetStringPorIDCampo("010.020.000.040")),
                            new Property(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadoraOtros, item.GetStringPorIDCampo("010.020.000.050")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFacultadEscuela, item.GetStringPorIDCampo("010.020.000.060")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDepartamento, item.GetNameEntityBeanPorIDCampo("010.020.000.080")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCiudadEntidadEmpleadora, item.GetStringPorIDCampo("010.020.000.100")),
                            new Property(Variables.SituacionProfesional.cargosActividadesPaisEntidadEmpleadora, item.GetPaisPorIDCampo("010.020.000.110")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCCAAEntidadEmpleadora, item.GetRegionPorIDCampo("010.020.000.120")),
                            new Property(Variables.SituacionProfesional.cargosActividadesTelefonoFijo, item.GetStringPorIDCampo("010.020.000.140")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesFax, item.GetStringPorIDCampo("010.020.000.150")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesCorreoElectronico, item.GetStringPorIDCampo("010.020.000.160")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCategoriaProfesional, item.GetStringPorIDCampo("010.020.000.170")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFechaInicio, item.GetStringDatetimePorIDCampo("010.020.000.180")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionAnio, item.GetDurationAnioPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionMes, item.GetDurationMesPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionDia, item.GetDurationDiaPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesModalidadContrato, item.GetStringPorIDCampo("010.020.000.200")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesModalidadContratoOtros, item.GetStringPorIDCampo("010.020.000.210")),//TODO -check
                            new Property(Variables.SituacionProfesional.cargosActividadesRegimenDedicacion, item.GetStringPorIDCampo("010.020.000.220")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesCodUnescoPrimaria, item.GetStringPorIDCampo("010.020.000.230")),//rep//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesCodUnescoSecundaria, item.GetStringPorIDCampo("010.020.000.240")),//rep//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesCodUnescoTerciaria, item.GetStringPorIDCampo("010.020.000.250")),//rep//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesFuncionesDesempeñadas, item.GetStringPorIDCampo("010.020.000.260")),
                            new Property(Variables.SituacionProfesional.cargosActividadesPalabrasClave, item.GetStringPorIDCampo("010.020.000.270")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesInteresDocencia, item.GetStringPorIDCampo("010.020.000.280")),
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestion, item.GetStringPorIDCampo("010.020.000.290")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.020.000.300")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("010.020.000.310"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
    }
}
