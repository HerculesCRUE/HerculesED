using System.Collections.Generic;

namespace EditorCV.Models.EnvioDSpace
{
    public class MetadataSend
    {
        public Rootobject rootObject;
        public MetadataSend(List<Metadata> listado)
        {
            this.rootObject = new Rootobject(listado);
        }

        public class Rootobject
        {
            public Rootobject(List<Metadata> listado)
            {
                this.metadata = listado.ToArray();
            }
            public Metadata[] metadata { get; set; }
        }

    }
}
