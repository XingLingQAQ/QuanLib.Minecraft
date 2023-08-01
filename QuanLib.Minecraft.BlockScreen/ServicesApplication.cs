﻿using QuanLib.Minecraft.BlockScreen.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen
{
    public abstract class ServicesApplication : Application
    {
        protected ServicesApplication(string arguments) : base(arguments)
        {

        }

        public abstract IRootForm RootForm { get; }
    }
}