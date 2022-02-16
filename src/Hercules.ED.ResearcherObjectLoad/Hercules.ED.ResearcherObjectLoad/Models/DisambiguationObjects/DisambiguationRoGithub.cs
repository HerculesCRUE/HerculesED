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
        private string mIdRo { get; set; }
        public string idRo
        {
            get
            {
                return mIdRo;
            }
            set
            {
                if (value == null)
                {
                    mIdRo = string.Empty;
                }
                else
                {
                    mIdRo = value;
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


        private static DisambiguationDataConfig configTipo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f
        };

        private static DisambiguationDataConfig configIdRo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsIdentifiers
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
                property = "idRo",
                config = configIdRo,
                value = idRo
            });

            return data;
        }
    }
}
