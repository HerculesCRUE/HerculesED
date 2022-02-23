using Hercules.ED.DisambiguationEngine.Models;
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

        private string mTipo { get; set; }
        public string tipo
        {
            get
            {
                return mTipo;
            }
            set
            {
                if (value == null)
                {
                    mTipo = string.Empty;
                }
                else
                {
                    mTipo = value;
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

        private static DisambiguationDataConfig configTitulo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.5f
        };

        private static DisambiguationDataConfig configTipo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f
        };

        private static DisambiguationDataConfig configDOI = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static DisambiguationDataConfig configIdFigshare = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
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
                property = "tipo",
                config = configTipo,
                value = tipo
            });

            data.Add(new DisambiguationData()
            {
                property = "idFigshare",
                config = configIdFigshare,
                value = idFigshare
            });

            return data;
        }
    }
}

