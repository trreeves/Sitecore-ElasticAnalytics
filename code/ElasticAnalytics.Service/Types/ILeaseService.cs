namespace ElasticAnalytics.Service.Types
{
    using System;

    using ElasticAnalytics.Model.Locking;

    public interface ILeaseService
    {
        ElasticLease Get(Guid resourceId, ISystemContext ctx);

        /// <summary>
        /// Reliably locks the specified resource to the owner if not currently locked by another owner.  Lease is
        /// extended if needed, if already owned by current owner.
        /// </summary>
        bool TryObtainLock(ElasticLease challenger, ISystemContext ctx, out ElasticLease winner);

        /// <summary>
        /// A hard delete of any lock that exists for the specified resource.
        /// </summary>
        void Delete(Guid resourceId, ISystemContext ctx);

        /// <summary>
        /// Releases (deletes) a lock if the specified owner is allowed (i.e an active lock isn't held by another owner).
        /// </summary>
        bool Release(Guid resourceId, ElasticLeaseOwner leaseOwner, ISystemContext ctx);

        bool Extend(ElasticLease lease, TimeSpan leaseDuration, ISystemContext ctx, out ElasticLease currentLease);
    }
}