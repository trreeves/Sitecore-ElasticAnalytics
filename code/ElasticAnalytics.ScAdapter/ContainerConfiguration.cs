namespace ElasticAnalytics.ScAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Abstractions;

    public static class ContainerConfiguration
    {
        private static IFactory scFactory;

        private static IElasticAnalyticsIoCContainer elasticAnalyticsIoCContainer;

        private static IFactory ScFactory
        {
            get
            {
                if (scFactory == null)
                {
                    scFactory = new FactoryWrapper();
                }

                return scFactory;
            }
        }

        public static IElasticAnalyticsIoCContainer AppDomainInstance // call a spade a spade
        {
            get
            {
                if (elasticAnalyticsIoCContainer == null)
                {
                    // create the container
                    elasticAnalyticsIoCContainer = (IElasticAnalyticsIoCContainer)ScFactory.CreateObject("/elasticAnalytics/container", true);

                    // configure/initialize it
                    var settings = elasticAnalyticsIoCContainer.Resolve<ISettings>();
                    elasticAnalyticsIoCContainer.Initialize(GetContainerConfigTags(settings).ToArray());
                }

                return elasticAnalyticsIoCContainer;
            }
        }

        private static IEnumerable<string> GetContainerConfigTags(ISettings settings)
        {
            var configurationTagsSetting = settings.GetSetting("ElasticAnalytics.Configuration.Tags");

            IEnumerable<string> configurationTags = Enumerable.Empty<string>();
            if (!string.IsNullOrWhiteSpace(configurationTagsSetting))
            {
                configurationTags = configurationTagsSetting.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var cleanedTags = new[] { "default" }.Concat(configurationTags).Select(t => t.ToLower()).Distinct();

            return cleanedTags;
        }
    }
}
