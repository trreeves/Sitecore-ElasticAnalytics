namespace ElasticAnalytics.ScAdapter.Mapping
{
    using System;

    using AutoMapper;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.ScAdapter.FacetModelSerialization;
    using ElasticAnalytics.Utils.AutoMapper;

    using Newtonsoft.Json.Linq;

    using Sitecore.Analytics.DataAccess;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;

    using AuthenticationLevel = ElasticAnalytics.Model.Contact.AuthenticationLevel;

    public class ContactModelMappingProfile : Profile
    {
        private readonly IElementJsonConverter elementConverter;

        public ContactModelMappingProfile(IElementJsonConverter elementConverter)
        {
            if (elementConverter == null)
            {
                throw new ArgumentNullException("elementConverter");
            }

            this.elementConverter = elementConverter;
        }

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
            this.CreateMap<IContactIdentifiers, ElasticContactIdentification>()
                    .ConstructUsing(
                    (ctx, i) => new ElasticContactIdentification(
                        i.Identifier,
                        (AuthenticationLevel)((int)i.AuthenticationLevel),
                        (IdentificationLevel)((int)i.IdentificationLevel)));

            this.CreateMap<IContact, ElasticContact>()
                .ConstructUsing((ctx, scContact) =>  
                    new ElasticContact(
                                scContact.Id.Guid,
                                ctx.Engine.Map<IContactIdentifiers, ElasticContactIdentification>(scContact.Identifiers)))
                .ForMember(c => c.Metadata, cfg => cfg.Ignore())
                .ForMember(c => c.Facets, cfg => cfg.Ignore())
                .AfterMap((scContact, elasticContact) =>
                        {
                            scContact.SerializeAsFacet(c => c.System, elasticContact.Metadata, this.elementConverter);
                            scContact.SerializeAsFacet(c => c.Tags, elasticContact.Metadata, this.elementConverter);
                            scContact.SerializeAsFacet(c => c.Extensions, elasticContact.Metadata, this.elementConverter);

                            foreach (var facetName in scContact.Facets.Keys)
                            {
                                elasticContact.Facets.Add(
                                    new JProperty(
                                        facetName, 
                                        this.elementConverter.Serialize(scContact.Facets[facetName])));
                            }
                        });

            // Elastic -> SC
            this.CreateMap<ElasticContact, IContact>()
                .AfterMap((elasticContact, scContact) =>
                    {
                        // identifiers
                        scContact.Identifiers.PopulateAsFacetMember(i => i.Identifier, elasticContact.Identification.Identity);
                        scContact.Identifiers.PopulateAsFacetMember(i => i.IdentificationLevel, (ContactIdentificationLevel)(int)elasticContact.Identification.IdentityLevel);
                        scContact.Identifiers.PopulateAsFacetMember(i => i.AuthenticationLevel, (Sitecore.Analytics.Model.AuthenticationLevel)(int)elasticContact.Identification.AuthLevel);

                        // metadata
                        scContact.DeserializeAsFacet(c => c.System, elasticContact.Metadata, this.elementConverter);
                        scContact.DeserializeAsFacet(c => c.Tags, elasticContact.Metadata, this.elementConverter);
                        scContact.DeserializeAsFacet(c => c.Extensions, elasticContact.Metadata, this.elementConverter);

                        foreach (var facetProperty in elasticContact.Facets.Properties())
                        {
                            var facetObj = scContact.Facets[facetProperty.Name];
                            if (facetProperty.Value != null && facetObj != null && facetProperty.Value is JObject)
                            {
                                this.elementConverter.Deserialize(facetProperty.Value as JObject, facetObj);    
                            }
                        }
                    });

            this.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            this.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
