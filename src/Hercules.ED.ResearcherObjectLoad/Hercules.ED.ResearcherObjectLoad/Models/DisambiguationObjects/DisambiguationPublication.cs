using Hercules.ED.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects
{
    public class DisambiguationPublication : DisambiguableEntity
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

        private string mWosId { get; set; }
        public string wosId
        {
            get
            {
                return mWosId;
            }
            set
            {
                if (value == null)
                {
                    mWosId = string.Empty;
                }
                else
                {
                    mWosId = value;
                }
            }
        }

        private string mScopusId { get; set; }
        public string scopusId
        {
            get
            {
                return mScopusId;
            }
            set
            {
                if (value == null)
                {
                    mScopusId = string.Empty;
                }
                else
                {
                    mScopusId = value;
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

        private static readonly DisambiguationDataConfig configWosId = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static readonly DisambiguationDataConfig configScopusId = new DisambiguationDataConfig()
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
                property = "title",
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
                property = "wosId",
                config = configWosId,
                value = wosId
            });

            data.Add(new DisambiguationData()
            {
                property = "scopusId",
                config = configScopusId,
                value = scopusId
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
