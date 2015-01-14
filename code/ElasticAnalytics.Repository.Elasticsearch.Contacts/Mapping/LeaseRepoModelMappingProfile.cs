namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.Mapping
{
    using System;

    using AutoMapper;

    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel;
    using ElasticAnalytics.Utils.AutoMapper;

    public class LeaseRepoModelMappingProfile : Profile
    {
        public override string ProfileName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        protected override void Configure()
        {
            // Elastic -> ES
            this.CreateMap<ElasticLeaseOwner, EsLeaseOwner>();

            this.CreateMap<ElasticLease, EsLease>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(ec => ec.ResourceId));

            // ES -> Elastic

            this.CreateMap<EsLeaseOwner, ElasticLeaseOwner>()
                .ConstructUsing((ctx, lo) => new ElasticLeaseOwner(lo.Id, lo.Type));

            this.CreateMap<EsLease, ElasticLease>()
                .ConstructUsing(
                    (ctx, l) => new ElasticLease(Guid.Parse(l._id), l.Expires, ctx.Engine.Map<EsLeaseOwner, ElasticLeaseOwner>(l.Owner), l.Version));

            this.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            this.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
