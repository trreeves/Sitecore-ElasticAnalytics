namespace ElasticAnalytics.Tests.Common
{
    using System;

    using Castle.Windsor;

    using Ploeh.AutoFixture.Kernel;

    public class WindsorAdapter : ISpecimenBuilder
    {
        private readonly IWindsorContainer container;

        public WindsorAdapter(IWindsorContainer container)
        {
            this.container = container;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var t = request as Type;
            if (t == null || !this.container.Kernel.HasComponent(t))
                return new NoSpecimen(request);

            return this.container.Resolve(t);
        }
    }
}