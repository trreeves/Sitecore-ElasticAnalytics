namespace ElasticAnalytics.ScAdapter.Adapter
{
    using System;

    using ElasticAnalytics.Service.Types;
    using ElasticAnalytics.Service.Types.Services;
    using ElasticAnalytics.Utils.AutoMapper;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;

    /// <summary>
    /// The integration point with existing Sitecore analytics system.  All work is 
    /// delegated out to other components for configurability and testability, as inheriting
    /// from the base class ties it down too much.
    /// </summary>
    public partial class ElasticAnalyticsDataAdapterProvider : DataAdapterProvider
    {
        private readonly IElasticAnalyticsContactService contactService;

        private readonly ITypeMapper modelMapper;

        private readonly ISystemContext ctx;

       // this is our composition root - the best we can hope for at the moment.
        public ElasticAnalyticsDataAdapterProvider() :
            this(ContainerConfiguration.AppDomainInstance)
        { }

       public ElasticAnalyticsDataAdapterProvider(IElasticAnalyticsIoCContainer container) :
            this(
                container.Resolve<IElasticAnalyticsContactService>(),
                container.Resolve<ITypeMapper>(),
                container.Resolve<ISystemContext>())
        { }

        public ElasticAnalyticsDataAdapterProvider(
            IElasticAnalyticsContactService contactService,
            ITypeMapper modelMapper,
            ISystemContext ctx)
        {
            if (contactService == null)
            {
                throw new ArgumentNullException("contactService");
            }

            if (modelMapper == null)
            {
                throw new ArgumentNullException("modelMapper");
            }

            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            this.contactService = contactService;
            this.modelMapper = modelMapper;
            this.ctx = ctx;
        }

        public override VisitorBase Visitors
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override VisitsBase Visits
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool GetClusterHealth(LeaseOwner leaseOwner)
        {
            throw new NotImplementedException();
        }

        public override void SetClusterHealth(LeaseOwner leaseOwner, DateTime time)
        {
            throw new NotImplementedException();
        }

        #region Not Implemented Yet
        
        public override void Update(IUpdatableObject container)
        {
            throw new NotImplementedException();
        }

        public override void Remove(IUpdatableObject container)
        {
            throw new NotImplementedException();
        }
        
        public override DataStorage<TEntity, TKey> GetDataStorage<TEntity, TKey>(string name)
        {
            throw new NotImplementedException();
        }

        public override DictionaryBase DictionaryData
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MaintenanceBase Maintenance
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override SystemHealthBase SystemHealth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override TrafficTypeBase TrafficType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override VisitorClassificationBase VisitorClassification
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
