namespace ElasticAnalytics.Tests.Customizations.Assertions
{
    using System;

    using ElasticAnalytics.Model.Contact;
    using ElasticAnalytics.Model.Locking;

    using FluentAssertions;

    using Newtonsoft.Json;

    public static class ElasticAssertionsExtensions
    {
        /// <summary>
        /// Returns an <see cref="ContactAssertions"/> object that can be used to assert the
        /// current <see cref="ElasticContact"/>.
        /// </summary>
        public static ContactAssertions Should(this ElasticContact actualValue)
        {
            return new ContactAssertions(actualValue);
        }

        public static void ShouldBeEquivalentToContact(
            this ElasticContact subject, 
            ElasticContact expectation,
            Func<ElasticLease, bool> leaseAssertion = null)
        {
            subject.ShouldBeEquivalentTo(expectation, opt => opt
                .Excluding(c => c.Lease)
                .Excluding(c => c.Metadata)
                .Excluding(c => c.Facets));

            subject.Metadata.ToString(Formatting.Indented)
                .Should()
                .Be(expectation.Metadata.ToString(Formatting.Indented));

            if (leaseAssertion != null)
            {
                leaseAssertion(subject.Lease).Should().BeTrue();
            }
            else
            {
                subject.Lease.ShouldBeEquivalentTo(expectation.Lease, opt => opt.Excluding(l => l.Expires));
            }
        }
    }
}