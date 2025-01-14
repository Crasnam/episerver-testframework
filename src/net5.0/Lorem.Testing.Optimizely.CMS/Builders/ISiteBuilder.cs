﻿using EPiServer.Core;

namespace Lorem.Testing.Optimizely.CMS.Builders
{
    public interface ISiteBuilder
        : IFixtureBuilder
    {
    }

    public interface ISiteBuilder<T>
        : ISiteBuilder where T : PageData
    {
        ISiteBuilder<T> CreateSite();

        ISiteBuilder<T> CreateSite(string name, string url);
    }
}
