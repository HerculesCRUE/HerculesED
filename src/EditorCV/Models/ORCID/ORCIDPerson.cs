using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EditorCV.Models.ORCID
{
    /// <summary>
    /// ORCIDPerson
    /// </summary>
    [DataContract]
    public class ORCIDPerson
    {
        /// <summary>
        /// ORCIDPerson
        /// </summary>
        public ORCIDPerson()
        {

        }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        public Name name { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataContract]
        public class Name
        {
            /// <summary>
            /// Name
            /// </summary>
            [DataMember(Name = "given-names")]
            public NameValue given_names { get; set; }
            /// <summary>
            /// Name
            /// </summary>
            [DataMember(Name = "family-name")]
            public NameValue family_name { get; set; }
        }

        /// <summary>
        /// Name value
        /// </summary>
        [DataContract]
        public class NameValue
        {
            /// <summary>
            /// value
            /// </summary>
            [DataMember(Name = "value")]
            public string value { get; set; }
        }


    }
}
