namespace ElasticAnalytics.Model.Locking
{
    using System;

    public class ElasticLease : IVersionable
    {
        protected readonly Guid _id;

        protected readonly DateTime _expires;

        protected readonly ElasticLeaseOwner _owner;

        protected IVersionable versioning;

        public ElasticLease(
            Guid leasedResourceId, 
            DateTime expires, 
            ElasticLeaseOwner owner, 
            long version = -1)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            this._id = leasedResourceId;
            this._expires = expires;
            this._owner = owner;
            this.versioning = new VersioningMixin(version);
        }

        public virtual bool HasExpired(DateTime relativeTo)
        {
            return this._expires <= relativeTo;
        }

        public virtual bool IsOwnedBy(ElasticLeaseOwner owner)
        {
            return this.Owner.Id == owner.Id && this.Owner.Type == owner.Type;
        }

        public Guid ResourceId
        {
            get
            {
                return this._id;
            }
        }

        public virtual DateTime Expires
        {
            get
            {
                return this._expires;
            }
        }

        public virtual ElasticLeaseOwner Owner
        {
            get
            {
                return this._owner;
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
