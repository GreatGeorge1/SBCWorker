using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Worker.Core.Models
{
    public class Employer : SoftEntity<long>, IExternal
    {
        /// <summary>
        ///     Уровень доступа
        /// </summary>
        [Required]
        [EnumDataType(typeof(AccessLevel))]
        public AccessLevel AccessLevel { get; set; }
        public string ExternalId { get ; set; }
        public ICollection<Card> Cards { get; set; }
        public ICollection<Fingerprint> Fingerprints { get; set; }
        public ICollection<Device> Devices { get; set; }
    }
}
