namespace ElasticAnalytics.Repository.Elasticsearch.Contacts.Mapping
{
    using System;

    using AutoMapper;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Repository.Elasticsearch.Contacts.PersistenceModel;
    using ElasticAnalytics.Utils.AutoMapper;

    using Newtonsoft.Json.Linq;

    public class ContactRepoModelMappingProfile : Profile
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
            this.CreateMap<ElasticContactIdentification, EsContactIdentification>();

            this.CreateMap<ElasticContact, EsContact>()
                .ForMember(dest => dest._id, cfg => cfg.MapFrom(src => src.Id))
                .ForMember(dest => dest.Metadata, cfg => cfg.Ignore())
                .ForMember(dest => dest.Facets, cfg => cfg.Ignore())
                .AfterMap(
                    (src, dest) =>
                        {
                            dest.Metadata = src.Metadata.DeepClone() as JObject;
                            dest.Facets = src.Facets.DeepClone() as JObject;
                        });

            this.CreateMap<ElasticContactIdentityMap, EsContactIdentityMap>()
                .ForMember(dest => dest._id, cfg => cfg.MapFrom(em => em.Identity));

            // ES -> Elastic
            this.CreateMap<EsContactIdentification, ElasticContactIdentification>()
                .ConstructUsing((ctx, i) => new ElasticContactIdentification(
                    i.Identity,
                    i.AuthLevel,
                    i.IdentityLevel));

            this.CreateMap<EsContact, ElasticContact>()
               .ConstructUsing(
                   (ctx, c) =>
                   {
                       if (c.Obsolete)
                       {
                           return new ElasticContact(
                               Guid.Parse(c._id),
                               c.Successor);
                       }

                       return new ElasticContact(
                           Guid.Parse(c._id),
                           ctx.Engine.Map<EsContactIdentification, ElasticContactIdentification>(c.Identification));
                   })
                .ForMember(dest => dest.Metadata, cfg => cfg.Ignore())
                .ForMember(dest => dest.Facets, cfg => cfg.Ignore())
                .AfterMap(
                    (src, dest) =>
                        {
                            dest.Metadata = src.Metadata.DeepClone() as JObject;
                            dest.Facets = src.Facets.DeepClone() as JObject;
                        });

            this.CreateMap<EsContactIdentityMap, ElasticContactIdentityMap>()
                .ConstructUsing((ctx, i) => new ElasticContactIdentityMap(i._id, i.ContactId));

            this.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            this.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
