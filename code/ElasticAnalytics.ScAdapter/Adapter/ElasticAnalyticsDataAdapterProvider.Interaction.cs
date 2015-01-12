namespace ElasticAnalytics.ScAdapter.Adapter
{
    using System;
    using System.Collections.Generic;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;

    public partial class ElasticAnalyticsDataAdapterProvider
    {
        public override IList<TInteraction> LoadInteractions<TInteraction>(InteractionLoadOptions options)
        {
            throw new NotImplementedException();
            //return _interactionService.LoadInteractions<TInteraction>(options);
        }

        public override IList<VisitData> LoadVisits(InteractionLoadOptions options)
        {
            throw new NotImplementedException();
            //return _interactionService.LoadVisits(options);
        }

        public override void MergeVisits(Guid dyingContact, Guid survivingContact)
        {
            throw new NotImplementedException();
            //_interactionService.MergeVisits(dyingContact, survivingContact);
        }
    }
}