namespace ElasticAnalytics.Model.Contact
{
    using System;

    public class ElasticContactIdentityMap : IVersionable
    {
        protected readonly string identity;

        protected readonly Guid contactId;

        protected IVersionable versioning;

        public ElasticContactIdentityMap(string identity, Guid contactId, long version = -1)
        {
            this.identity = identity;
            this.contactId = contactId;
            this.versioning = new VersioningMixin(version);
        }

        public virtual string Identity
        {
            get
            {
                return this.identity;
            }
        }

        public virtual Guid ContactId
        {
            get
            {
                return this.contactId;
            }
        }

        // IVersionable

        public long Version
        {
            get
            {
                return this.versioning.Version;
            }
        }

        public virtual void SetVersion(long newVersion)
        {
            this.versioning.SetVersion(newVersion);
        }

        public virtual void IncrementVersion(long increment = 1)
        {
            this.versioning.IncrementVersion(increment);
        }
    }
}
