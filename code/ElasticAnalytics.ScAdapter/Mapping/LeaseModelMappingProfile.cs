namespace ElasticAnalytics.ScAdapter.Mapping
{
    using AutoMapper;

    using ElasticAnalytics.Model.Locking;
    using ElasticAnalytics.Utils.AutoMapper;

    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;

    public class LeaseModelMappingProfile : Profile
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
            // SC -> Elastic

            this.CreateMap<LeaseOwner, ElasticLeaseOwner>()
                .ConstructUsing((ctx, lo) => new ElasticLeaseOwner(
                    lo.Identifier,
                    (ElasticAnalytics.Model.Locking.LeaseOwnerType)(int)lo.Type));

            this.CreateMap<LeaseData, ElasticLease>()
                .ConstructUsing((ctx, ld) => new ElasticLease(
                    ((IContact)ctx.Parent.SourceValue).Id.Guid,
                    ld.ExpirationTime,
                    ctx.Engine.Map<LeaseOwner, ElasticLeaseOwner>(ld.Owner)));

            // Elastic -> SC

            this.CreateMap<ElasticLeaseOwner, LeaseOwner>()
                .ConstructUsing((ctx, elo) => new LeaseOwner(
                    elo.Id,
                    (Sitecore.Analytics.Model.LeaseOwnerType)(int)elo.Type));

            this.CreateMap<ElasticLease, LeaseData>()
                .ForMember(dest => dest.ExpirationTime, opt => opt.MapFrom(el => el.Expires));

            this.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            this.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
