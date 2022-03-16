using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportadorWebCV
{
    // NOTA: El código generado puede requerir, como mínimo, .NET Framework 4.5 o .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class cvnRootResultBean
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("CvnItemBean", Namespace = "http://codes.cvn.fecyt.es/beans", IsNullable = false)]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBean : CVNObject
    {
        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute("Code", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("CvnAuthorBean", typeof(CvnItemBeanCvnAuthorBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnBoolean", typeof(CvnItemBeanCvnBoolean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnCodeGroup", typeof(CvnItemBeanCvnCodeGroup))]
        [System.Xml.Serialization.XmlElementAttribute("CvnDateDayMonthYear", typeof(CvnItemBeanCvnDateDayMonthYear))]
        [System.Xml.Serialization.XmlElementAttribute("CvnDateMonthYear", typeof(CvnItemBeanCvnDateMonthYear))]
        [System.Xml.Serialization.XmlElementAttribute("CvnDateYear", typeof(CvnItemBeanCvnDateYear))]
        [System.Xml.Serialization.XmlElementAttribute("CvnDouble", typeof(CvnItemBeanCvnDouble))]
        [System.Xml.Serialization.XmlElementAttribute("CvnDuration", typeof(CvnItemBeanCvnDuration))]
        [System.Xml.Serialization.XmlElementAttribute("CvnEntityBean", typeof(CvnItemBeanCvnEntityBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnExternalPKBean", typeof(CvnItemBeanCvnExternalPKBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnFamilyNameBean", typeof(CvnItemBeanCvnFamilyNameBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnPageBean", typeof(CvnItemBeanCvnPageBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnPhoneBean", typeof(CvnItemBeanCvnPhoneBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnRichText", typeof(CvnItemBeanCvnRichText))]
        [System.Xml.Serialization.XmlElementAttribute("CvnString", typeof(CvnItemBeanCvnString))]
        [System.Xml.Serialization.XmlElementAttribute("CvnTitleBean", typeof(CvnItemBeanCvnTitleBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnVolumeBean", typeof(CvnItemBeanCvnVolumeBean))]
        [System.Xml.Serialization.XmlElementAttribute("CvnPhotoBean", typeof(CvnItemBeanCvnPhotoBean))]
        public List<CVNObject> Items { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnAuthorBean : CVNObject
    {
        /// <remarks/>
        public CvnItemBeanCvnAuthorBeanCvnFamilyNameBean CvnFamilyNameBean { get; set; }

        /// <remarks/>
        public string GivenName { get; set; }

        /// <remarks/>
        public string Signature { get; set; }

        /// <remarks/>
        public byte SignatureOrder { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SignatureOrderSpecified { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnAuthorBeanCvnFamilyNameBean : CVNObject
    {

        /// <remarks/>
        public string FirstFamilyName { get; set; }

        /// <remarks/>
        public string SecondFamilyName { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnBoolean : CVNObject
    {

        /// <remarks/>
        public bool Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroup : CVNObject
    {
        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnBoolean CvnBoolean { get; set; }

        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnDouble[] CvnDouble { get; set; }

        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnEntityBean CvnEntityBean { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CvnString")]
        public CvnItemBeanCvnCodeGroupCvnString[] CvnString { get; set; }

        /// <remarks/>
        public CvnItemBeanCvnCodeGroupCvnTitleBean CvnTitleBean { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnBoolean : CVNObject
    {

        /// <remarks/>
        public bool Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnDouble : CVNObject
    {

        /// <remarks/>
        public byte Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnEntityBean : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string Id { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnString : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnCodeGroupCvnTitleBean : CVNObject
    {

        /// <remarks/>
        public ushort Identification { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IdentificationSpecified { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateDayMonthYear : CVNObject
    {

        /// <remarks/>
        public System.DateTime Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateMonthYear : CVNObject
    {

        /// <remarks/>
        public System.DateTime Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDateYear : CVNObject
    {

        /// <remarks/>
        public System.DateTime Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDouble : CVNObject
    {

        /// <remarks/>
        public decimal Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnDuration : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnEntityBean : CVNObject
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string Id { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnFamilyNameBean : CVNObject
    {

        /// <remarks/>
        public string FirstFamilyName { get; set; }

        /// <remarks/>
        public string SecondFamilyName { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnPageBean : CVNObject
    {

        /// <remarks/>
        public string FinalPage { get; set; }

        /// <remarks/>
        public string InitialPage { get; set; }

    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnRichText : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnString : CVNObject
    {

        /// <remarks/>
        public string Value { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnTitleBean : CVNObject
    {

        /// <remarks/>
        public string Identification { get; set; }

        /// <remarks/>
        public string Name { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnVolumeBean : CVNObject
    {

        /// <remarks/>
        public string Volume { get; set; }

        /// <remarks/>
        public string Number { get; set; }
    }


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://codes.cvn.fecyt.es/beans")]
    public partial class CvnItemBeanCvnPhotoBean : CVNObject
    {

        /// <remarks/>
        public string BytesInBase64 { get; set; }

        /// <remarks/>
        public string MimeType { get; set; }
    }




}
