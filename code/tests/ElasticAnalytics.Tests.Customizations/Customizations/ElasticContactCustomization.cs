namespace ElasticAnalytics.Tests.Customizations.Customizations
{
    using System;

    using ElasticAnalytics.Model.Contact;

    using Newtonsoft.Json.Linq;

    using Ploeh.AutoFixture;

    public class ElasticContactCustomization : ICustomization
    {
        private readonly IdentificationLevel _idLevel;

        public ElasticContactCustomization(IdentificationLevel idLevel = IdentificationLevel.Known)
        {
            this._idLevel = idLevel;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register<ElasticContact>(
                () =>
                    {
                        var contact = new ElasticContact(
                            Guid.NewGuid(),
                            new ElasticContactIdentification(
                                this._idLevel == IdentificationLevel.Known ? fixture.Create<string>("tester") : null,
                                AuthenticationLevel.None,
                                this._idLevel));

                        contact.Metadata.Add(
                            new JProperty("System", 
                                new JObject(
                                    new JProperty("Classification", 1),
                                    new JProperty("OverrideClassification", 2),
                                    new JProperty("VisitCount", 101),
                                    new JProperty("Value", 10),
                                    new JProperty("IntegrationId", Guid.NewGuid()),
                                    new JProperty("AuthenticationLevel", 0))));

                        return contact;
                    });
        }
    }
}
