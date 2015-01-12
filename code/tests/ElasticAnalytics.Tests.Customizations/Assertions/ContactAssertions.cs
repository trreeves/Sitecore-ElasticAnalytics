namespace ElasticAnalytics.Tests.Customizations.Assertions
{
    using System;

    using ElasticAnalytics.Model.Contact;

    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;

    public class ContactAssertions : ReferenceTypeAssertions<ElasticContact, ContactAssertions>
    {
        public ContactAssertions(ElasticContact contact)
        {
            this.Subject = contact;
        }

        public AndConstraint<ContactAssertions> BeObsolete(
            string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(
                    this.Subject.Id != Guid.Empty && 
                    this.Subject.Identification == null &&
                    this.Subject.Obsolete == true &&
                    this.Subject.Successor != null)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected contact to be obsolete.");

            return new AndConstraint<ContactAssertions>(this);
        }

        /// <summary>
        /// Returns the type of the subject the assertion applies on.
        /// </summary>
        protected override string Context
        {
            get { return "ElasticContact"; }
        }
    }
}