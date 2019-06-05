using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Worker.Core.Models
{
    public abstract class Entity<PrimaryKeyT>
    {
        protected Entity()
        {
            CreationTime = DateTime.UtcNow;
        }
        public PrimaryKeyT Id { get; set; }
        public DateTime CreationTime { get; set; }
        [Timestamp] public byte[] Timestamp { get; set; }
    }

    public abstract class SoftEntity<PrimaryKeyT> : Entity<PrimaryKeyT>, ISoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTime DeleteTime { get; set; }

        public void SoftDelete()
        {
            this.IsDeleted = true;
            this.DeleteTime = DateTime.UtcNow;
        }
    }

    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime DeleteTime { get; set; }
    }

    public interface IExternal
    {
        /// <summary>
        ///     Id в исходной базе
        /// </summary>
        string ExternalId { get; set; }
    }
}
