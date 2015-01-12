namespace ElasticAnalytics.ScAdapter.Tests.Customizations
{
    using System;

    using Ploeh.AutoFixture;

    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Data;

    public class IContactCustomization : ICustomization
    {
        private readonly IContactFactory contactFactory;

        public IContactCustomization(
            IContactFactory contactFactory)
        {
            this.contactFactory = contactFactory;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<IContact>(c =>
                c.FromFactory(() =>
            {
                var contact = this.contactFactory.Create(ID.NewID);
                return contact;
            })
            .Do(contact =>
                    {
                        contact.Identifiers.IdentificationLevel = ContactIdentificationLevel.Known;
                        contact.Identifiers.AuthenticationLevel = AuthenticationLevel.None;
                        contact.Identifiers.Identifier = fixture.Create<string>("tester");

                        contact.System.Value = 10;
                        contact.System.VisitCount = 99;
                        contact.System.IntegrationId = Guid.NewGuid();
                        contact.System.IntegrationLabel = "testIntegrationLabel";

                        var personalFacet = contact.Facets["Personal"] as IContactPersonalInfo;
                        personalFacet.Title = "Mr";
                        personalFacet.FirstName = "Dave";
                        personalFacet.MiddleName = "T";
                        personalFacet.Surname = "Tester";
                        personalFacet.BirthDate = new DateTime(1993, 6, 23);
                        personalFacet.Gender = "Male";
                        personalFacet.JobTitle = "Acceptance Tester";
                        personalFacet.Nickname = "Davey T";

                        var addressesFacet = contact.Facets["Addresses"] as IContactAddresses;
                        addressesFacet.Preferred = "home";

                        var homeAddress = addressesFacet.Entries.Create("home");
                        homeAddress.StreetLine1 = "Sitecore House";
                        homeAddress.StreetLine2 = "1 Temple Way";
                        homeAddress.StreetLine3 = "Bristol Meads";
                        homeAddress.City = "Bristol";
                        homeAddress.StateProvince = "Avon";
                        homeAddress.Country = "United Kingdom";
                        homeAddress.PostalCode = "BS1 8SC";
                        homeAddress.Location.Latitude = 51.45f;
                        homeAddress.Location.Longitude = 2.5833f;

                        var awayAddress = addressesFacet.Entries.Create("away");
                        awayAddress.StreetLine1 = "Sitecore Away House";
                        awayAddress.StreetLine2 = "1 Temple AWay";
                        awayAddress.StreetLine3 = "Away Meads";
                        awayAddress.City = "Away Town";
                        awayAddress.StateProvince = "Awayshire";
                        awayAddress.Country = "United Kingdom";
                        awayAddress.PostalCode = "AW1 3SA";
                        awayAddress.Location.Latitude = 53.45f;
                        awayAddress.Location.Longitude = 4.5833f;

                        var phoneNumbers = contact.Facets["Phone Numbers"] as IContactPhoneNumbers;
                        var workNumber = phoneNumbers.Entries.Create("work");
                        workNumber.CountryCode = "+44";
                        workNumber.Extension = "01";
                        workNumber.Number = "0173965234";

                        // TODO: create some more facet data...

                        //contact.Extensions.Groups

                        //contact.Tags.Entries.Create("tag1");
                    }));
        }
    }
}
