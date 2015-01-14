namespace ElasticAnalytics.Service.Types.Services
{
    using System;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;

    public interface IElasticAnalyticsContactService
    {
        LockAttemptResult<ElasticContact> TryLoadAndLock(
            Guid id,
            ElasticLeaseOwner targetLeaseOwner,
            TimeSpan leaseDuration, 
            ISystemContext ctx);

        LockAttemptResult<ElasticContact> TryLoadAndLock(
            string identity,
            ElasticLeaseOwner targetLeaseOwner,
            TimeSpan leaseDuration,
            ISystemContext ctx);

        bool TryExtendLock(ElasticContact contact, TimeSpan leaseDuration, ISystemContext ctx);

        bool Obsolete(Guid id, Guid successor, ElasticLeaseOwner leaseOwner, ISystemContext ctx);

        void Delete(Guid id, ISystemContext ctx);

        bool Release(Guid id, ElasticLeaseOwner leaseOwner, ISystemContext ctx);

        bool Save(ElasticContact contact, ElasticLeaseOwner leaseOwner, bool release, ISystemContext ctx);

        ElasticContact LoadForReadOnly(string identity, ISystemContext ctx);

        ElasticContact LoadForReadOnly(Guid id, ISystemContext ctx);
    }
}
