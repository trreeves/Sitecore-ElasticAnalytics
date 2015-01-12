using System;
using System.Collections.Generic;
using System.Linq;

namespace ElasticAnalytics.Container.Windsor
{
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using ElasticAnalytics.ScAdapter;

    public class ElasticAnalyticsWindsorContainer : IElasticAnalyticsIoCContainer
    {
        private IWindsorContainer container;

        private readonly IEnumerable<Func<string, IWindsorInstaller>> discoveryMechanisms;

        public ElasticAnalyticsWindsorContainer() :
            this(
                (tag) => FromAssembly.InDirectory(
                            new AssemblyFilter(".", "*ElasticAnalytics.Configuration*"), 
                            new TagFilteringInstallerFactory(tag))
            )
        {
        }

        public ElasticAnalyticsWindsorContainer(params Func<string, IWindsorInstaller>[] discoveryMechanisms)
        {
            this.discoveryMechanisms = discoveryMechanisms;
        }

        public virtual IElasticAnalyticsIoCContainer Initialize(params string[] tags)
        {
            this.container = new WindsorContainer();

            foreach (var tag in tags)
            {
                foreach (var discoveryMechanism in discoveryMechanisms)
                {
                    this.container.Install(discoveryMechanism(tag));
                }
            }

            return this;
        }

        public T Resolve<T>()
        {
            return this.container.Resolve<T>();
        }

        public IWindsorContainer Container
        {
            get
            {
                return this.container;
            }
        }
    }

    public class TagFilteringInstallerFactory : InstallerFactory
    {
        private readonly string tagName;

        public TagFilteringInstallerFactory(string tagName)
        {
            this.tagName = tagName;
        }

        public override IEnumerable<Type> Select(IEnumerable<Type> installerTypes)
        {
            return installerTypes.Where(
                    t => t.Name.EndsWith(this.tagName + "Installer", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
