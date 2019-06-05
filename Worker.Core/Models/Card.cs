using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Worker.Core.Models
{
    public class Card : SoftEntity<long>, IExternal
    {
        public string Uid { get; set; }
        /// <summary>
        ///     Uid hash
        /// </summary>
        public string Md5 { get; set; }
        public Employer Employer { get; set; }
        public long EmployerId { get; set; }
        public string ExternalId { get; set; }
    }
}
