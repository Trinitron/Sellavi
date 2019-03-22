using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SellAvi.Model.DataBaseModels
{
    [DataContract]
    [Table("AvitoUsers")]
    public class AvitoUser
    {
        public int Id { get; set; }
        [DataMember] [Index(IsUnique = true)] public int ProfileId { get; set; }

        [Required]
        [DataMember]
        [StringLength(4000)]
        [Index(IsUnique = true)]
        public string UserName { get; set; }

        [StringLength(4000)] public string UserPassword { get; set; }

        [StringLength(4000)] public string UserCookie { get; set; }

        [DataMember] [StringLength(4000)] public string CompanyName { get; set; }

        [DataMember] [StringLength(4000)] public string CompanyManager { get; set; }

        [DataMember] [StringLength(4000)] public string CompanyEmail { get; set; }

        [DataMember] [StringLength(4000)] public string CompanyPhones { get; set; }


        #region NotMapped Calculated Fields

        [NotMapped]
        public ICollection<string> SplitedCompanyPhones => CompanyPhones != null ? CompanyPhones.Split(';') : null;

        #endregion
    }
}