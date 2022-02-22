using Hercules.ED.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects
{
    public class DisambiguationRoGithub : DisambiguableEntity
    {
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

        private static DisambiguationDataConfig configTipo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f
        };

        private static DisambiguationDataConfig configIdGitHub = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
        };

        private static DisambiguationDataConfig configTitulo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "tipo",
                config = configTipo,
                value = tipo
            });

            data.Add(new DisambiguationData()
            {
                property = "idGitHub",
                config = configIdGitHub,
                value = idGithub
            });

            data.Add(new DisambiguationData()
            {
                property = "titulo",
                config = configTitulo,
                value = title
            });

            return data;
        }
    }
}
