using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Model
{
    public class VersioningMixin : IVersionable
    {
        protected long version;

        public VersioningMixin(long version = -1)
        {
            this.version = version;
        }

        public virtual void SetVersion(long newVersion)
        {
            if (newVersion < this.version)
            {
                throw new ArgumentOutOfRangeException("newVersion", "The version of an entity cannot be decremented. New version supplied : " + newVersion);
            }

            this.version = newVersion;
        }

        public virtual void IncrementVersion(long increment = 1)
        {
            if (this.version + increment <= this.version)
            {
                throw new ArgumentOutOfRangeException("increment", "The version of an entity must be incremented.  Increment supplied : " + increment);
            }

            this.version = this.version + increment;
        }

        public virtual long Version
        {
            get
            {
                return this.version;
            }
        }
    }
}
