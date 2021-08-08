﻿using Lorem.Testing.EPiServer.CMS.Utility;
using System;

namespace Lorem.Testing.EPiServer.CMS.Test.Services
{
    public class DefaultResources
        : Resources
    {
        public DefaultResources()
            : base(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"))
        {
        }
    }
}
