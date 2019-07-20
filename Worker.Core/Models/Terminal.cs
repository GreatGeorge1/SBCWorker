using System;
using System.Collections.Generic;

namespace Worker.Core.Models
{
    public class Terminal : SoftEntity<long>, IExternal
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(AccessLevel))]
        public AccessLevel AccessLevel { get; set; }
        public string Alias { get; set; }
        public string Mac { get; set; }
        public string Version { get; set; }
        public DateTime LastActivity { get; set; }
        public string ExternalId { get; set; }
        public TerminalConfig Config { get; set; }
        public ICollection<Fingerprint> Fingerprints { get; set; } 

        public Controller Controller { get; set; }
        public long ControllerId { get; set; }
    }
}
