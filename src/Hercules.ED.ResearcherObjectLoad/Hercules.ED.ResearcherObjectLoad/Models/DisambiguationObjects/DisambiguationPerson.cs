using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects
{
    public class DisambiguationPerson : DisambiguableEntity
    {
        private string mIdGnoss { get; set; }
        public string idGnoss
        {
            get
            {
                return mIdGnoss;
            }
            set
            {
                if (value == null)
                {
                    mIdGnoss = string.Empty;
                }
                else
                {
                    mIdGnoss = value;
                }
            }
        }

        private string mCompleteName { get; set; }
        public string completeName
        {
            get
            {
                return mCompleteName;
            }
            set
            {
                if (value == null)
                {
                    mCompleteName = string.Empty;
                }
                else
                {
                    mCompleteName = value;
                }
            }
        }

        private string mOrcid { get; set; }
        public string orcid
        {
            get
            {
                return mOrcid;
            }
            set
            {
                if (value == null)
                {
                    mOrcid = string.Empty;
                }
                else
                {
                    mOrcid = value;
                }
            }
        }

        private string mGitHubId { get; set; }
        public string gitHubId
        {
            get
            {
                return mGitHubId;
            }
            set
            {
                if (value == null)
                {
                    mGitHubId = string.Empty;
                }
                else
                {
                    mGitHubId = value;
                }
            }
        }

        private string mFigShareId { get; set; }
        public string figShareId
        {
            get
            {
                return mFigShareId;
            }
            set
            {
                if (value == null)
                {
                    mFigShareId = string.Empty;
                }
                else
                {
                    mFigShareId = value;
                }
            }
        }

        private string mZenodoId { get; set; }
        public string zenodoId
        {
            get
            {
                return mZenodoId;
            }
            set
            {
                if (value == null)
                {
                    mZenodoId = string.Empty;
                }
                else
                {
                    mZenodoId = value;
                }
            }
        }

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
        public HashSet<string> organizacion
        {
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
        public HashSet<string> departamento
        {
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
        public HashSet<string> grupos
        {
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

        private HashSet<string> mProyectos { get; set; }
        public HashSet<string> proyectos
        {
            get
            {
                return mProyectos;
            }
            set
            {
                if (value == null)
                {
                    mProyectos = new HashSet<string>();
                }
                else
                {
                    mProyectos = value;
                }
            }
        }

        public DisambiguationPerson()
        {
            idGnoss = "";
            completeName = "";
            orcid = "";
            gitHubId = "";
            figShareId = "";
            zenodoId = "";
            distincts = new HashSet<string>();
            mCoautores = new HashSet<string>();
            mDocumentos = new HashSet<string>();
            mOrganizacion = new HashSet<string>();
            mDepartamento = new HashSet<string>();
            mGrupos = new HashSet<string>();
            mProyectos = new HashSet<string>();
        }

        private static readonly DisambiguationDataConfig configCompleteName = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.algoritmoNombres,
            score = 1f
        };

        private static readonly DisambiguationDataConfig configORCID = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configFigshare = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configGithub = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configZenodo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configIdGnoss = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configCoautores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        private static readonly DisambiguationDataConfig configDocumentos = new DisambiguationDataConfig()
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

        private static readonly DisambiguationDataConfig configGrupos = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        private static readonly DisambiguationDataConfig configProyectos = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "completeName",
                config = configCompleteName,
                value = completeName
            });

            data.Add(new DisambiguationData()
            {
                property = "idGnoss",
                config = configIdGnoss,
                value = idGnoss
            });

            data.Add(new DisambiguationData()
            {
                property = "orcid",
                config = configORCID,
                value = orcid
            });

            data.Add(new DisambiguationData()
            {
                property = "coautores",
                config = configCoautores,
                values = coautores
            });


            data.Add(new DisambiguationData()
            {
                property = "documentos",
                config = configDocumentos,
                values = documentos
            });

            data.Add(new DisambiguationData()
            {
                property = "github",
                config = configGithub,
                value = gitHubId
            });

            data.Add(new DisambiguationData()
            {
                property = "figshare",
                config = configFigshare,
                value = figShareId
            });

            data.Add(new DisambiguationData()
            {
                property = "zenodo",
                config = configZenodo,
                value = zenodoId
            });

            data.Add(new DisambiguationData()
            {
                property = "organizacion",
                config = configOrganizacion,
                values = organizacion
            });

            data.Add(new DisambiguationData()
            {
                property = "departamento",
                config = configDepartamento,
                values = departamento
            });

            data.Add(new DisambiguationData()
            {
                property = "grupos",
                config = configGrupos,
                values = grupos
            });

            data.Add(new DisambiguationData()
            {
                property = "proyectos",
                config = configProyectos,
                values = proyectos
            });
            return data;
        }
    }
}
