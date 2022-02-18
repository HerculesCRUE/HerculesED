using Hercules.ED.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects
{
    public class DisambiguationPerson : DisambiguableEntity
    {
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

        private static DisambiguationDataConfig configCompleteName = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.algoritmoNombres,
            score = 0.9f
        };

        private static DisambiguationDataConfig configORCID = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static DisambiguationDataConfig configFigshare = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static DisambiguationDataConfig configGithub = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static DisambiguationDataConfig configCoautores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.1f
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
                property = "figshare",
                config = configFigshare,
                value = figShareId
            });

            data.Add(new DisambiguationData()
            {
                property = "github",
                config = configGithub,
                value = gitHubId
            });

            return data;
        }
    }
}
