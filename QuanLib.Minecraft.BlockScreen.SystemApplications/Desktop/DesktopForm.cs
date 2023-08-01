﻿using QuanLib.Minecraft.BlockScreen.UI;
using QuanLib.Minecraft.BlockScreen.BlockForms;
using QuanLib.Minecraft.BlockScreen.BlockForms.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using QuanLib.Minecraft.BlockScreen.Event;

namespace QuanLib.Minecraft.BlockScreen.SystemApplications.Desktop
{
    public class DesktopForm : Form
    {
        public DesktopForm()
        {
            AllowResize = false;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;
            Skin.SetAllBackgroundBlockID(ConcretePixel.ToBlockID(MinecraftColor.LightBlue));
            //Skin.SetAllBackgroundBlockID("minecraft:air");
        }

        public override void Initialize()
        {
            base.Initialize();

            ActiveLayoutAll();
        }

        public override void ActiveLayoutAll()
        {
            MCOS os = MCOS.GetMCOS();
            SubControls.Clear();
            foreach (var app in os.ApplicationManager.ApplicationList.Values)
                if (app.AppendToDesktop)
                    SubControls.Add(new DesktopIcon(app));
            this.FillLayout(0, SubControls, 0);
        }

        public override void OnLayout(Control sender, SizeChangedEventArgs e)
        {
            base.OnLayout(sender, e);

            ActiveLayoutAll();
        }
    }
}