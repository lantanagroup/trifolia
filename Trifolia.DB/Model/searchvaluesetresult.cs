using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [EdmComplexTypeAttribute(NamespaceName = "Trifolia.DB", Name = "SearchValueSetResult")]
    [DataContractAttribute(IsReference = true)]
    [Serializable()]
    public partial class SearchValueSetResult : ComplexObject
    {
        #region Factory Method

        /// <summary>
        /// Create a new SearchValueSetResult object.
        /// </summary>
        /// <param name="totalItems">Initial value of the TotalItems property.</param>
        /// <param name="id">Initial value of the Id property.</param>
        /// <param name="oid">Initial value of the Oid property.</param>
        /// <param name="name">Initial value of the Name property.</param>
        /// <param name="isComplete">Initial value of the IsComplete property.</param>
        /// <param name="hasPublishedIg">Initial value of the HasPublishedIg property.</param>
        /// <param name="canEditPublishedIg">Initial value of the CanEditPublishedIg property.</param>
        public static SearchValueSetResult CreateSearchValueSetResult(
            int totalItems, 
            int id,
            string name,
            string oid, 
            string code,
            string description,
            bool? intensional,
            string intensionalDefinition,
            string source,
            bool isComplete,
            bool hasPublishedIg,
            bool canEditPublishedIg)
        {
            SearchValueSetResult searchValueSetResult = new SearchValueSetResult()
            {
                TotalItems = totalItems,
                Id = id,
                Name = name,
                Oid = oid,
                Code = code,
                Description = description,
                Intensional = intensional,
                IntensionalDefinition = intensionalDefinition,
                SourceUrl = source,
                IsComplete = isComplete,
                HasPublishedIg = hasPublishedIg,
                CanEditPublishedIg = canEditPublishedIg
            };

            return searchValueSetResult;
        }

        #endregion

        #region Simple Properties

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.Int32 TotalItems
        {
            get
            {
                return _TotalItems;
            }
            set
            {
                OnTotalItemsChanging(value);
                ReportPropertyChanging("TotalItems");
                _TotalItems = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TotalItems");
                OnTotalItemsChanged();
            }
        }
        private global::System.Int32 _TotalItems;
        partial void OnTotalItemsChanging(global::System.Int32 value);
        partial void OnTotalItemsChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.Int32 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                OnIdChanging(value);
                ReportPropertyChanging("Id");
                _Id = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Id");
                OnIdChanged();
            }
        }
        private global::System.Int32 _Id;
        partial void OnIdChanging(global::System.Int32 value);
        partial void OnIdChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.String Oid
        {
            get
            {
                return _Oid;
            }
            set
            {
                OnOidChanging(value);
                ReportPropertyChanging("Oid");
                _Oid = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Oid");
                OnOidChanged();
            }
        }
        private global::System.String _Oid;
        partial void OnOidChanging(global::System.String value);
        partial void OnOidChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging("Name");
                _Name = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Name");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public global::System.String Code
        {
            get
            {
                return _Code;
            }
            set
            {
                OnCodeChanging(value);
                ReportPropertyChanging("Code");
                _Code = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Code");
                OnCodeChanged();
            }
        }
        private global::System.String _Code;
        partial void OnCodeChanging(global::System.String value);
        partial void OnCodeChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public global::System.String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                OnDescriptionChanging(value);
                ReportPropertyChanging("Description");
                _Description = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Description");
                OnDescriptionChanged();
            }
        }
        private global::System.String _Description;
        partial void OnDescriptionChanging(global::System.String value);
        partial void OnDescriptionChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> Intensional
        {
            get
            {
                return _Intensional;
            }
            set
            {
                OnIntensionalChanging(value);
                ReportPropertyChanging("Intensional");
                _Intensional = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Intensional");
                OnIntensionalChanged();
            }
        }
        private Nullable<global::System.Boolean> _Intensional;
        partial void OnIntensionalChanging(Nullable<global::System.Boolean> value);
        partial void OnIntensionalChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public global::System.String IntensionalDefinition
        {
            get
            {
                return _IntensionalDefinition;
            }
            set
            {
                OnIntensionalDefinitionChanging(value);
                ReportPropertyChanging("IntensionalDefinition");
                _IntensionalDefinition = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("IntensionalDefinition");
                OnIntensionalDefinitionChanged();
            }
        }
        private global::System.String _IntensionalDefinition;
        partial void OnIntensionalDefinitionChanging(global::System.String value);
        partial void OnIntensionalDefinitionChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = true)]
        [DataMemberAttribute()]
        public global::System.String SourceUrl
        {
            get
            {
                return _SourceUrl;
            }
            set
            {
                OnSourceUrlChanging(value);
                ReportPropertyChanging("SourceUrl");
                _SourceUrl = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SourceUrl");
                OnSourceUrlChanged();
            }
        }
        private global::System.String _SourceUrl;
        partial void OnSourceUrlChanging(global::System.String value);
        partial void OnSourceUrlChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.Boolean IsComplete
        {
            get
            {
                return _IsComplete;
            }
            set
            {
                OnIsCompleteChanging(value);
                ReportPropertyChanging("IsComplete");
                _IsComplete = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IsComplete");
                OnIsCompleteChanged();
            }
        }
        private global::System.Boolean _IsComplete;
        partial void OnIsCompleteChanging(global::System.Boolean value);
        partial void OnIsCompleteChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.Boolean HasPublishedIg
        {
            get
            {
                return _HasPublishedIg;
            }
            set
            {
                OnHasPublishedIgChanging(value);
                ReportPropertyChanging("HasPublishedIg");
                _HasPublishedIg = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("HasPublishedIg");
                OnHasPublishedIgChanged();
            }
        }
        private global::System.Boolean _HasPublishedIg;
        partial void OnHasPublishedIgChanging(global::System.Boolean value);
        partial void OnHasPublishedIgChanged();

        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty = false, IsNullable = false)]
        [DataMemberAttribute()]
        public global::System.Boolean CanEditPublishedIg
        {
            get
            {
                return _CanEditPublishedIg;
            }
            set
            {
                OnCanEditPublishedIgChanging(value);
                ReportPropertyChanging("CanEditPublishedIg");
                _CanEditPublishedIg = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("CanEditPublishedIg");
                OnCanEditPublishedIgChanged();
            }
        }
        private global::System.Boolean _CanEditPublishedIg;
        partial void OnCanEditPublishedIgChanging(global::System.Boolean value);
        partial void OnCanEditPublishedIgChanged();

        #endregion

    }
}
