using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects
{
    public class DisambiguationRO : DisambiguableEntity
    {
        private string mTitle { get; set; }
        public string title
        {
            get
            {
                return mTitle;
            }
            set
            {
                if (value == null)
                {
                    mTitle = string.Empty;
                }
                else
                {
                    mTitle = value;
                }
            }
        }

        private string mDoi { get; set; }
        public string doi
        {
            get
            {
                return mDoi;
            }
            set
            {
                if (value == null)
                {
                    mDoi = string.Empty;
                }
                else
                {
                    mDoi = value;
                }
            }
        }

        private string mIdFigShare { get; set; }
        public string idFigshare
        {
            get
            {
                return mIdFigShare;
            }
            set
            {
                if (value == null)
                {
                    mIdFigShare = string.Empty;
                }
                else
                {
                    mIdFigShare = value;
                }
            }
        }

        private string mIdGitHub { get; set; }
        public string idGithub
        {
            get
            {
                return mIdGitHub;
            }
            set
            {
                if (value == null)
                {
                    mIdGitHub = string.Empty;
                }
                else
                {
                    mIdGitHub = value;
                }
            }
        }

        private string mIdZenodo { get; set; }
        public string idZenodo
        {
            get
            {
                return mIdZenodo;
            }
            set
            {
                if (value == null)
                {
                    mIdZenodo = string.Empty;
                }
                else
                {
                    mIdZenodo = value;
                }
            }
        }

        private HashSet<string> mAutores { get; set; }
        public HashSet<string> autores
        {
            get
            {
                return mAutores;
            }
            set
            {
                if (value == null)
                {
                    mAutores = new HashSet<string>();
                }
                else
                {
                    mAutores = value;
                }
            }
        }

        private static readonly DisambiguationDataConfig configTitulo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configDOI = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configIdFigshare = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };


        private static readonly DisambiguationDataConfig configIdGitHub = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configIdZenodo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };


        private static readonly DisambiguationDataConfig configAutores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "titulo",
                config = configTitulo,
                value = title
            });

            data.Add(new DisambiguationData()
            {
                property = "doi",
                config = configDOI,
                value = doi
            });

            data.Add(new DisambiguationData()
            {
                property = "idFigshare",
                config = configIdFigshare,
                value = idFigshare
            });

            data.Add(new DisambiguationData()
            {
                property = "idGitHub",
                config = configIdGitHub,
                value = idGithub
            });

            data.Add(new DisambiguationData()
            {
                property = "idZenodo",
                config = configIdGitHub,
                value = idGithub
            });

            data.Add(new DisambiguationData()
            {
                property = "autores",
                config = configAutores,
                values = autores
            });

            return data;
        }
    }
}

