using Hercules.ED.DisambiguationEngine.Models;
using System.Collections.Generic;

namespace Hercules.ED.ImportadorWebCV.Models
{
    public class Persona : DisambiguableEntity
    {
        private HashSet<string> mCoautores { get; set; }
        public HashSet<string> coautores
        {
            get
            {
                return mCoautores;
            }
            set
            {
                if (value == null)
                {
                    mCoautores = new HashSet<string>();
                }
                else
                {
                    mCoautores = value;
                }
            }
        }
        
        private HashSet<string> mDocumentos { get; set; }
        public HashSet<string> documentos
        {
            get
            {
                return mDocumentos;
            }
            set
            {
                if (value == null)
                {
                    mDocumentos = new HashSet<string>();
                }
                else
                {
                    mDocumentos = value;
                }
            }
        }
        
        private string mOrganizacion { get; set; }
        public string organizacion {
            get
            {
                return mOrganizacion;
            }
            set
            {
                if (value == null)
                {
                    mOrganizacion = "";
                }
                else
                {
                    mOrganizacion = value;
                }
            }
        }
        private string mDepartamento { get; set; }
        public string departamento {
            get
            {
                return mDepartamento;
            }
            set
            {
                if (value == null)
                {
                    mDepartamento = "";
                }
                else
                {
                    mDepartamento = value;
                }
            }
        }

        private static readonly DisambiguationDataConfig configName = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.algoritmoNombres,
            score = 1f
        };
        private static readonly DisambiguationDataConfig configCoautores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        private static readonly DisambiguationDataConfig configOrganizacion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        private static readonly DisambiguationDataConfig configDepartamento = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        private static readonly DisambiguationDataConfig configDocumentos = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 1f
        };


        public string firma;
        public string nombreCompleto;
        public string nombre;
        public string primerApellido;
        public string segundoApellido;
        public string personid { get; set; }

        public Persona(string nombre, string primerApellido, string segundoApellido, string firma)
        {
            this.nombreCompleto = (nombre + " " + primerApellido + " " + segundoApellido).Trim();
            this.firma = firma;
            this.nombre = nombre;
            this.primerApellido = primerApellido;
            this.segundoApellido = segundoApellido;
        }

        public Persona()
        {
            this.nombreCompleto = "";
            this.firma = "";
        }

        public string NombreBuscar
        {
            get
            {
                string[] nombreSplit = nombreCompleto?.Split(" ");
                string[] firmaSplit = firma?.Split(" ");
                if (nombreSplit == null && firmaSplit == null)
                {
                    return "";
                }
                else if (nombreSplit == null)
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(firma);
                }
                else if (firmaSplit == null)
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(nombreCompleto);
                }
                else if (nombreSplit.Length > firmaSplit.Length)
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(nombreCompleto);
                }
                else if (firmaSplit.Length > nombreSplit.Length)
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(firma);
                }
                else if (nombreCompleto.Replace(" ", "").Length > firma.Replace(" ", "").Length)
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(nombreCompleto);
                }
                else
                {
                    return Disambiguation.ObtenerTextosNombresNormalizados(firma);
                }
            }
        }

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>
            {
                new DisambiguationData()
                {
                    property = "name",
                    config = configName,
                    value = NombreBuscar
                },

                new DisambiguationData()
                {
                    property = "documentos",
                    config = configDocumentos,
                    values = documentos
                },

                new DisambiguationData()
                {
                    property = "coautores",
                    config = configCoautores,
                    values = coautores
                },

                new DisambiguationData()
                {
                    property = "organizacion",
                    config = configOrganizacion,
                    value = organizacion
                },

                new DisambiguationData()
                {
                    property = "departamento",
                    config = configDepartamento,
                    value = departamento
                }
            };


            return data;
        }

    }
}
