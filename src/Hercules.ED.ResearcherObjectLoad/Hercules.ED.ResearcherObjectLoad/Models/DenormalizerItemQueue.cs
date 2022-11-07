using System.Collections.Generic;

namespace Hercules.ED.ResearcherObjectLoad.Models
{
    public class DenormalizerItemQueue
    {
        public enum ItemType
        {
            person,
            group,
            project,
            document,
            researchobject
        }

        public ItemType TypeItem { get; set; }
        public HashSet<string> Items { get; set; }

        public DenormalizerItemQueue(ItemType itemType, HashSet<string> items)
        {
            TypeItem = itemType;
            Items = items;
        }
    }
}
