﻿using EPiServer.Core;
using Lorem.Test.Framework.Optimizely.CMS.Commands;
using Lorem.Test.Framework.Optimizely.CMS.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lorem.Test.Framework.Optimizely.CMS.Builders
{
    public class PageBuilder<T>
        : FixtureBuilder<T>, IPageBuilder<T> where T : PageData
    {
        private List<PageData> _pages = new List<PageData>();

        public PageBuilder(Fixture fixture)
            : base(fixture)
        {
        }

        public PageBuilder(Fixture fixture, IEnumerable<PageData> pages)
            : base(fixture, pages)
        {
        }

        public IPageBuilder<T> Create(Action<T> build = null)
        {
            return Create<T>(build);
        }

        public IPageBuilder<TPageType> Create<TPageType>(Action<TPageType> build = null) 
            where TPageType : PageData
        {
            Create(GetParent(), build: build);

            return new PageBuilder<TPageType>(Fixture, _pages);
        }

        public IPageBuilder<T> CreateMany(int total)
        {
            return CreateMany<T>(total, (_)=> {});
        }

        public IPageBuilder<T> CreateMany(int total, Action<T, int> build)
        {
            return CreateMany<T>(total, build);
        }

        public IPageBuilder<T> CreateMany(int total, Action<T> build)
        {
            return CreateMany<T>(total, (p, _) => build?.Invoke(p));
        }

        public IPageBuilder<TPageType> CreateMany<TPageType>(int total) where TPageType : PageData
        {
            return CreateMany<TPageType>(total, (_, __) => { });
        }

        public IPageBuilder<TPageType> CreateMany<TPageType>(int total, Action<TPageType> build)
            where TPageType : PageData
        {
            return CreateMany<TPageType>(total, (p, _) => build?.Invoke(p));
        }

        public IPageBuilder<TPageType> CreateMany<TPageType>(int total, Action<TPageType, int> build) 
            where TPageType : PageData
        {
            if(Fixture.Latest.Count > 0)
            {
                foreach (var parent in Fixture.Latest)
                {
                    for (int index = 0; index < total; index++)
                    {
                        if (build == null)
                        {
                            Create<TPageType>(parent.ContentLink, null);
                            continue;
                        }

                        Create<TPageType>(parent.ContentLink, build: p => build.Invoke(p, index));
                    }
                }

                return new PageBuilder<TPageType>(Fixture, _pages);
            }


            for (int index = 0; index < total; index++)
            {
                if (build == null)
                {
                    Create<TPageType>(ContentReference.RootPage, null);
                    continue;
                }

                Create<TPageType>(ContentReference.RootPage, build: p => build.Invoke(p, index));
            }


            return new PageBuilder<TPageType>(Fixture, _pages);
        }

        public IPageBuilder<T> CreatePath(int depth)
        {
            return CreatePath<T>(depth, (_)=> { });
        }

        public IPageBuilder<T> CreatePath(int depth, Action<T> build)
        {
            return CreatePath<T>(depth, build);
        }

        public IPageBuilder<T> CreatePath(int depth, Action<T, int> build)
        {
            return CreatePath<T>(depth, build);
        }

        public IPageBuilder<TPageType> CreatePath<TPageType>(int depth)
            where TPageType : PageData
        {
            return CreatePath<TPageType>(depth, (_) => { });
        }

        public IPageBuilder<TPageType> CreatePath<TPageType>(int depth, Action<TPageType> build)
            where TPageType : PageData
        {
            var parent = GetParent();

            for (int index = 0; index < depth; index++)
            {
                if(_pages.Count > 0)
                {
                    parent = _pages.Last().ContentLink;
                }

                Create(parent, build:build);
            }

            return new PageBuilder<TPageType>(Fixture, _pages);
        }

        public IPageBuilder<TPageType> CreatePath<TPageType>(int depth, Action<TPageType, int> build)
            where TPageType: PageData
        {
            var parent = GetParent();

            for (int index = 0; index < depth; index++)
            {
                if (_pages.Count > 0)
                {
                    parent = _pages.Last().ContentLink;
                }

                Create<TPageType>(parent, build: p=> build.Invoke(p, index));
            }

            return new PageBuilder<TPageType>(Fixture, _pages);
        }


        public IPageBuilder<T> Update(Action<T> build)
        {
            foreach (var content in Fixture.Latest)
            {
                var command = new UpdateContent(content);

                if (build != null)
                {
                    command.Build = p => build.Invoke((T)p);
                }

                Add((PageData)command.Execute());
            }

            _pages.ForEach(p => Fixture.Add(p));

            return new PageBuilder<T>(Fixture);
        }

        public IPageBuilder<T> Update<TPageType>(Action<TPageType> build) where TPageType : PageData
        {
            foreach (var content in Fixture.Contents.OfType<TPageType>())
            {
                foreach (var culture in Fixture.Cultures)
                {
                    Update(
                        content,
                        culture,
                        p => build.Invoke((TPageType)p)
                    );
                }
            }

            _pages.ForEach(p => Fixture.Add(p));

            return new PageBuilder<T>(Fixture);
        }

        public IPageBuilder<T> Update<TPageType>(Action<TPageType, IEnumerable<T>> build) where TPageType : PageData
        {
            foreach (var content in Fixture.Contents.OfType<TPageType>())
            {
                foreach (var culture in Fixture.Cultures)
                {
                    Update(
                        content,
                        culture,
                        p => build.Invoke((TPageType)p, Fixture.GetLatest(culture).Select(c => (T)c))
                    );
                }
            }

            _pages.ForEach(p => Fixture.Add(p));
            return new PageBuilder<T>(Fixture);
        }

        private void Update(PageData page, CultureInfo culture, Action<object> build = null)
        {
            var command = new UpdateContent(page)
            {
                Culture = culture,
                Build = build
            };

            PageData content = (PageData)command.Execute();

            Add(content);
        }

        public IPageBuilder<T> Update(T page)
        {
            var command = new UpdateContent(page);
            command.Execute();

            return this;
        }

        public IPageBuilder<T> Upload<TMediaType>(IEnumerable<string> files, Action<TMediaType, T> build)
            where TMediaType : MediaData
        {
            foreach(T current in Fixture.Latest.Select(p => (T)p).ToList())
            {
                var builder = new MediaBuilder<TMediaType>(Fixture);

                var media = builder.Upload<TMediaType>(
                    files.PickRandom(),
                    current.ContentLink,
                    m => build.Invoke(m, current)
                ).First();

                Update(current);
                Add(current);

                foreach (var culture in current.ExistingLanguages.Where(l => !l.Equals(current.Language)))
                {
                    var currentAsCulture = Fixture.Get<T>(current, culture);

                    build.Invoke(media, currentAsCulture);

                    Update(currentAsCulture);
                }
            }

            return new PageBuilder<T>(Fixture, _pages);
        }

        private ContentReference GetParent()
        {
            ContentReference parent = ContentReference.RootPage;

            var page = Fixture.Latest.LastOrDefault(p => p is PageData);

            if (page != null)
            {
                parent = page.ContentLink;
            }

            return parent;
        }

        private void Create<TPageType>(ContentReference parent, CultureInfo culture = null, Action<TPageType> build = null)
            where TPageType : PageData
        {
            TPageType page = default;

            if(Fixture.Cultures.Count == 0)
            {
                throw new InvalidOperationException("Need atleast one culture");
            }

            List<CultureInfo> cultures = new List<CultureInfo>(Fixture.Cultures);

            if(culture != null)
            {
                cultures.Clear();
                cultures.Add(culture);
            }

            foreach(var c in cultures)
            {
                if(page is null)
                {
                    var command = new CreatePage(
                        Fixture.GetContentType(typeof(TPageType)),
                        parent,
                        IpsumGenerator.Generate(1, 3, false)
                    );

                    command.Culture = c;
                    command.Build = CreateBuild(build);

                    page = (TPageType)command.Execute();
                    Add(page);

                    continue;
                }

                if (build == null)
                {
                    Update(page, c, null);
                    continue;
                }

                Update((TPageType)(PageData)page, c, p => build.Invoke((TPageType)(PageData)p));
            }
        }

        private Action<object> CreateBuild<TPageType>(Action<TPageType> build)
            where TPageType : PageData
        {
            return p =>
            {
                foreach (var builder in Fixture.GetBuilders<TPageType>())
                {
                    builder.Invoke(p);
                }

                build?.Invoke((TPageType)p);
            };
        }

        private void Add(PageData page) 
        {
            _pages = _pages.Where(p => !p.ContentGuid.Equals(page.ContentGuid)).ToList();
            _pages.Add(page);
        }
    }
}
