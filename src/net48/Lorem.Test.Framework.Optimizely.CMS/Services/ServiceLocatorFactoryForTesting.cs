﻿using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;

[assembly: ServiceLocatorFactory(typeof(Lorem.Test.Framework.Optimizely.CMS.Services.ServiceLocatorFactoryForTesting))]
namespace Lorem.Test.Framework.Optimizely.CMS.Services
{
    public class ServiceLocatorFactoryForTesting
        : IServiceLocatorFactory
    {
        private readonly StructureMapServiceLocatorFactory _decorated;

        public ServiceLocatorFactoryForTesting()
        {
            _decorated = new StructureMapServiceLocatorFactory();
        }

        public IServiceLocator CreateLocator()
        {
            return new ServiceLocatorDecorator(_decorated.CreateLocator());
        }

        public IServiceConfigurationProvider CreateProvider()
        {
            return _decorated.CreateProvider();
        }
    }
}
