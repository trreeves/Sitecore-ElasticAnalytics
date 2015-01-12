namespace ElasticAnalytics.ScAdapter.Tests.Customizations
{
    using System;
    using System.Collections.Generic;

    using Moq;

    using Ploeh.AutoFixture;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Analytics.Model.Framework;
    using Sitecore.Data;

    public class ContactFactoryCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<IContactFactory>(
                () =>
                    {
                        var factory = new Mock<IContactFactory>();
                        factory.Setup(f => f.Create(It.IsAny<ID>()))
                            .Returns(
                                (ID id) =>
                                    {
                                        // get facet map
                                        var contactFacetMap = new Dictionary<string, Type>()
                                                {
                                                    {"Personal", typeof(IContactPersonalInfo)},
                                                    {"Addresses", typeof(IContactAddresses)},
                                                    {"Emails", typeof(IContactEmailAddresses)},
                                                    {"Phone Numbers", typeof(IContactPhoneNumbers)},
                                                    {"Picture", typeof(IContactPicture)}
                                                };
                                                    //{"Communication Profile", typeof(IContactCoummunicationProfile)},
                                                    //{"Preferences", typeof(IContactPreferences)},
                                                    //{"SocialProfile", typeof(Sitecore.Social.Connector.Facets.Contact.SocialProfile.ISocialProfileFacet)}

                                        var result = ModelFactory.CreateContact(id, contactFacetMap);
                                        return result;
                                    });

                        return factory.Object;
                    });
        }
    }
}
