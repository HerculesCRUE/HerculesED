﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportExportCV.Models.FuentesExternas
{
    public class Publication
    {
        public List<string> problema { get; set; }
        public string typeOfPublication { get; set; } //no es un atributo de la ontologia!!
        public string title { get; set; }
        public List<FreetextKeywords> freetextKeywords { get; set; }
        public string Abstract { get; set; }
        public string language { get; set; }
        public string doi { get; set; }
        public DateTimeValue dataIssued { get; set; }
        public HashSet<string> url { get; set; }
        public string pdf { get; set; }
        public List<Knowledge_enriquecidos> topics_enriquecidos { get; set; }
        public List<Knowledge_enriquecidos> freetextKeyword_enriquecidas { get; set; }
        public Person correspondingAuthor { get; set; }
        public List<Person> seqOfAuthors { get; set; }
        public List<KnowledgeAreas> hasKnowledgeAreas { get; set; }
        public string pageEnd { get; set; }
        public string pageStart { get; set; }
        public string volume { get; set; }
        public string articleNumber { get; set; }
        public bool? openAccess { get; set; }
        public List<string> IDs { get; set; }
        public string presentedAt { get; set; }
        //todo no creo que esto en nuestra ontologia sea un string y no esta contemplado de mommento rellenarlo! 
        public Source hasPublicationVenue { get; set; }
        public List<PublicationMetric> hasMetric { get; set; }
        public List<PubReferencias> bibliografia { get; set; }
        public HashSet<string> dataOriginList { get; set; }
        public string dataOrigin { get; set; }
    }
}
