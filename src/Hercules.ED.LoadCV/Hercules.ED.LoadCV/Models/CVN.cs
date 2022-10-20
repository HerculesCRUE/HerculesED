using System.Collections.Generic;

namespace Hercules.ED.LoadCV.Models
{
    // NOTA: El código generado puede requerir, como mínimo, .NET Framework 4.5 o .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class cvnRootResultBean
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("CvnItemBean", Namespace = "http://codes.cvn.fecyt.es/beans", IsNullable = false)]
        public CvnItemBean[] cvnRootBean { get; set; }

        /// <remarks/>
        public byte errorCode { get; set; }

        /// <remarks/>
        public string errorMessage { get; set; }

        /// <remarks/>
        public ushort numElementos { get; set; }
    }

    public partial class CVNObject
    {
        /// <remarks/>
        public string Code { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBean : CVNObject
    {
        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute("Code", typeof(string))]
        [System.Xml.Serialization.XmlElement("CvnAuthorBean", typeof(CvnItemBeanCvnAuthorBean))]
        [System.Xml.Serialization.XmlElement("CvnBoolean", typeof(CvnItemBeanCvnBoolean))]
        [System.Xml.Serialization.XmlElement("CvnCodeGroup", typeof(CvnItemBeanCvnCodeGroup))]
        [System.Xml.Serialization.XmlElement("CvnDateDayMonthYear", typeof(CvnItemBeanCvnDateDayMonthYear))]
        [System.Xml.Serialization.XmlElement("CvnDateMonthYear", typeof(CvnItemBeanCvnDateMonthYear))]
        [System.Xml.Serialization.XmlElement("CvnDateYear", typeof(CvnItemBeanCvnDateYear))]
        [System.Xml.Serialization.XmlElement("CvnDouble", typeof(CvnItemBeanCvnDouble))]
        [System.Xml.Serialization.XmlElement("CvnDuration", typeof(CvnItemBeanCvnDuration))]
        [System.Xml.Serialization.XmlElement("CvnEntityBean", typeof(CvnItemBeanCvnEntityBean))]
        [System.Xml.Serialization.XmlElement("CvnExternalPKBean", typeof(CvnItemBeanCvnExternalPKBean))]
        [System.Xml.Serialization.XmlElement("CvnFamilyNameBean", typeof(CvnItemBeanCvnFamilyNameBean))]
        [System.Xml.Serialization.XmlElement("CvnPageBean", typeof(CvnItemBeanCvnPageBean))]
        [System.Xml.Serialization.XmlElement("CvnPhoneBean", typeof(CvnItemBeanCvnPhoneBean))]
        [System.Xml.Serialization.XmlElement("CvnRichText", typeof(CvnItemBeanCvnRichText))]
        [System.Xml.Serialization.XmlElement("CvnString", typeof(CvnItemBeanCvnString))]
        [System.Xml.Serialization.XmlElement("CvnTitleBean", typeof(CvnItemBeanCvnTitleBean))]
        [System.Xml.Serialization.XmlElement("CvnVolumeBean", typeof(CvnItemBeanCvnVolumeBean))]
        [System.Xml.Serialization.XmlElement("CvnPhotoBean", typeof(CvnItemBeanCvnPhotoBean))]
        public List<CVNObject> Items { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnAuthorBean : CVNObject
    {
        /// <remarks/>
        public CvnItemBeanCvnAuthorBeanCvnFamilyNameBean CvnFamilyNameBean { get; set; }

        /// <remarks/>
        public string GivenName { get; set; }

        /// <remarks/>
        public string Signature { get; set; }

        /// <remarks/>
        public int SignatureOrder { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool SignatureOrderSpecified { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnAuthorBeanCvnFamilyNameBean : CVNObject
    {

        /// <remarks/>
        public string FirstFamilyName { get; set; }

        /// <remarks/>
        public string SecondFamilyName { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnBoolean : CVNObject
    {

        /// <remarks/>
        public bool Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroup : CVNObject
    {
        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnBoolean CvnBoolean { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CvnDouble")]
        public CvnItemBeanCvnCodeGroupCvnDouble[] CvnDouble { get; set; }

        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnEntityBean CvnEntityBean { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CvnString")]
        public CvnItemBeanCvnCodeGroupCvnString[] CvnString { get; set; }

        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnTitleBean CvnTitleBean { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnBoolean : CVNObject
    {

        /// <remarks/>
        public bool Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnDouble : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnEntityBean : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string Id { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnString : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }


    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnTitleBean : CVNObject
    {

        /// <remarks/>
        public ushort Identification { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IdentificationSpecified { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateDayMonthYear : CVNObject
    {

        /// <remarks/>
        public DateTime Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateMonthYear : CVNObject
    {

        /// <remarks/>
        public DateTime Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateYear : CVNObject
    {

        /// <remarks/>
        public DateTime Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDouble : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDuration : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnEntityBean : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "integer")]
        public string Id { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnExternalPKBean : CVNObject
    {

        /// <remarks/>
        public string Type { get; set; }

        /// <remarks/>
        public string Value { get; set; }

        /// <remarks/>
        public string Others { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnFamilyNameBean : CVNObject
    {

        /// <remarks/>
        public string FirstFamilyName { get; set; }

        /// <remarks/>
        public string SecondFamilyName { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnPageBean : CVNObject
    {

        /// <remarks/>
        public string FinalPage { get; set; }

        /// <remarks/>
        public string InitialPage { get; set; }

    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnPhoneBean : CVNObject
    {

        /// <remarks/>
        public string Extension { get; set; }

        /// <remarks/>
        public string InternationalCode { get; set; }

        /// <remarks/>
        public string Number { get; set; }

        /// <remarks/>
        public byte Type { get; set; }

        /// <remarks/>
        public bool TypeSpecified { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnRichText : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnString : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnTitleBean : CVNObject
    {

        /// <remarks/>
        public string Identification { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnVolumeBean : CVNObject
    {

        /// <remarks/>
        public string Volume { get; set; }

        /// <remarks/>
        public string Number { get; set; }
    }


    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnPhotoBean : CVNObject
    {

        /// <remarks/>
        public string BytesInBase64 { get; set; }

        /// <remarks/>
        public string MimeType { get; set; }
    }




}
