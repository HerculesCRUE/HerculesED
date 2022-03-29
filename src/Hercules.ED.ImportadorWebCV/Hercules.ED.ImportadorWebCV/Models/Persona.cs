﻿using Hercules.ED.DisambiguationEngine.Models;
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
        
        private HashSet<string> mOrganizacion { get; set; }
        public HashSet<string> organizacion {
            get
            {
                return mOrganizacion;
            }
            set
            {
                if (value == null)
                {
                    mOrganizacion = new HashSet<string>();
                }
                else
                {
                    mOrganizacion = value;
                }
            }
        }
        private HashSet<string> mDepartamento { get; set; }
        public HashSet<string> departamento {
            get
            {
                return mDepartamento;
            }
            set
            {
                if (value == null)
                {
                    mDepartamento = new HashSet<string>();
                }
                else
                {
                    mDepartamento = value;
                }
            }
        }
        private HashSet<string> mGrupos { get; set; }
        public HashSet<string> grupos {
            get
            {
                return mGrupos;
            }
            set
            {
                if (value == null)
                {
                    mGrupos = new HashSet<string>();
                }
                else
                {
                    mGrupos = value;
                }
            }
        }
        private HashSet<string> mProyectosComp { get; set; }
        public HashSet<string> proyectosComp{
            get
            {
                return mProyectosComp;
            }
            set
            {
                if (value == null)
                {
                    mProyectosComp = new HashSet<string>();
                }
                else
                {
                    mProyectosComp = value;
                }
            }
        }
        private HashSet<string> mProyectosNoComp { get; set; }
        public HashSet<string> proyectosNoComp {
            get
            {
                return mProyectosNoComp;
            }
            set
            {
                if (value == null)
                {
                    mProyectosNoComp = new HashSet<string>();
                }
                else
                {
                    mProyectosNoComp = value;
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
            score = 0.5f,
            scoreMinus = 0.0f
        };

        private static readonly DisambiguationDataConfig configDepartamento = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f,
            scoreMinus = 0.0f
        };

        private static readonly DisambiguationDataConfig configGrupos = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f,
            scoreMinus = 0.0f
        };

        private static readonly DisambiguationDataConfig configProyectosComp = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f,
            scoreMinus = 0.0f
        };

        private static readonly DisambiguationDataConfig configProyectosNoComp = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f,
            scoreMinus = 0.0f
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
            this.coautores = new HashSet<string>();
            this.documentos = new HashSet<string>();
            this.departamento = new HashSet<string>();
            this.organizacion = new HashSet<string>();
            this.grupos = new HashSet<string>();
            this.proyectosComp = new HashSet<string>();
            this.proyectosNoComp = new HashSet<string>();
        }

        public Persona()
        {
            this.nombreCompleto = "";
            this.firma = "";
            this.coautores = new HashSet<string>();
            this.documentos = new HashSet<string>();
            this.departamento = new HashSet<string>();
            this.organizacion = new HashSet<string>();
            this.grupos = new HashSet<string>();
            this.proyectosComp = new HashSet<string>();
            this.proyectosNoComp = new HashSet<string>();
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
                    values = organizacion
                },

                new DisambiguationData()
                {
                    property = "departamento",
                    config = configDepartamento,
                    values = departamento
                },

                new DisambiguationData()
                {
                    property = "grupos",
                    config = configGrupos,
                    values = grupos
                },

                new DisambiguationData()
                {
                    property = "proyectosCompetitivos",
                    config = configProyectosComp,
                    values = proyectosComp
                },

                new DisambiguationData()
                {
                    property = "proyectosNoCompetitivos",
                    config = configProyectosNoComp,
                    values = proyectosNoComp
                }
            };


            return data;
        }

    }
}
