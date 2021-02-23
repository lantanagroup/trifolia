namespace Trifolia.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("user")]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            ManagingGroups = new HashSet<GroupManager>();
            AccessManagerImplemntationGuides = new HashSet<ImplementationGuide>();
            Permissions = new HashSet<ImplementationGuidePermission>();
            Templates = new HashSet<Template>();
            Groups = new HashSet<UserGroup>();
            Roles = new HashSet<UserRole>();
            AccessRequests = new HashSet<ImplementationGuideAccessRequest>();
        }

        [Column("id")]
        public int Id { get; set; }

        [Column("userName")]
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Column("firstName")]
        [Required]
        [StringLength(125)]
        public string FirstName { get; set; }

        [Column("lastName")]
        [Required]
        [StringLength(125)]
        public string LastName { get; set; }

        [Column("email")]
        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("phone")]
        [Required]
        [StringLength(50)]
        public string Phone { get; set; }

        [Column("okayToContact")]
        public bool? OkayToContact { get; set; }

        [Column("externalOrganizationName")]
        [StringLength(50)]
        public string ExternalOrganizationName { get; set; }

        [Column("externalOrganizationType")]
        [StringLength(50)]
        public string ExternalOrganizationType { get; set; }

        [Column("umlsApiKey")]
        [StringLength(255)]
        public string UMLSApiKey { get; set; }

        [Column("apiKey")]
        [StringLength(255)]
        public string ApiKey { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GroupManager> ManagingGroups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuide> AccessManagerImplemntationGuides { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuidePermission> Permissions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template> Templates { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserGroup> Groups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRole> Roles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ImplementationGuideAccessRequest> AccessRequests { get; set; }
    }
}
