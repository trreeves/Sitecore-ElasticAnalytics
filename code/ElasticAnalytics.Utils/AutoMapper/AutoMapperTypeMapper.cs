namespace ElasticAnalytics.Utils.AutoMapper
{
    using System;
    using System.Collections.Generic;

    using global::AutoMapper;
    using global::AutoMapper.Mappers;

    /// <summary>
    /// To be instantiated once and only once per app domain, at the start before any repository calls are made.
    /// </summary>
    public class AutoMapperTypeMapper : ITypeMapper
    {
        protected readonly IConfigurationProvider configProvider;

        protected readonly IMappingEngine engine;

        protected readonly IEnumerable<Profile> profiles;

        /// <summary>
        /// To be instantiated once and only once per app domain, at the start before any repository calls are made.
        /// </summary>
        public AutoMapperTypeMapper(IEnumerable<Profile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }

            this.configProvider = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            this.engine = new global::AutoMapper.MappingEngine(this.configProvider);
            this.profiles = profiles;
            this.Configure();
        }

        public virtual TDest Map<TSource, TDest>(TSource sourceObj)
        {
            return this.Engine.Map<TSource, TDest>(sourceObj);
        }

        public virtual TDest Map<TSource, TDest>(TSource sourceObj, TDest destObj)
        {
            return this.Engine.Map<TSource, TDest>(sourceObj, destObj);
        }

        public virtual IMappingEngine Engine
        {
            get
            {
                return this.engine;
            }
        }

        protected void Configure()
        {
            var config = (IConfiguration)this.configProvider;

            foreach (var profile in this.profiles)
            {
                config.AddProfile(profile);
            }

            config.Seal();
        }
    }
}

