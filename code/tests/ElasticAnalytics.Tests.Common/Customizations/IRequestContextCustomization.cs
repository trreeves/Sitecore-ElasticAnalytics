using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticAnalytics.Tests.Common.Customizations
{
    using ElasticAnalytics.Service.Types;

    using Moq;

    using Ploeh.AutoFixture;

    public class IRequestContextCustomization : ICustomization
    {
        private readonly string _systemPrefix;

        private readonly string _instanceKey;

        public IRequestContextCustomization(string systemPrefix, string instanceKey)
        {
            _systemPrefix = systemPrefix;
            _instanceKey = instanceKey;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Register(() =>
                        Mock.Of<ISystemContext>(c =>
                            c.InstanceKey == _instanceKey &&
                            c.SystemPrefix == _systemPrefix));
        }
    }
}
