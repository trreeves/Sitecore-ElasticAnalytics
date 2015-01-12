namespace ElasticAnalytics.Service.Types
{
    using System;

    using ElasticAnalytics.Model.Contact;

    public interface IContactIdentityMapService
    {
        ElasticContactIdentityMap Get(string identity, ISystemContext ctx);

        void Delete(string identity, ISystemContext ctx);

        bool MapIdentity(
            Guid targetContactId,
            ElasticContactIdentification targetIdentification,
            ElasticContactIdentification currentIdentification,
            ISystemContext ctx);
    }
}