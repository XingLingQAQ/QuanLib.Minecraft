﻿using QuanLib.Minecraft.BlockScreen.Controls;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.BuiltInApps
{
    public class Test02Form : WindowForm
    {
        public Test02Form()
        {
            button1 = new();
            comboButton1 = new();
            textBox1 = new();
            label1 = new();
            switch1 = new();
            switch2 = new();
        }

        private readonly Button button1;

        private readonly ComboButton<PlaneFacing> comboButton1;

        private readonly TextBox textBox1;

        private readonly Label label1;

        private readonly Switch switch1;

        private readonly Switch switch2;

        public override void Initialize()
        {
            base.Initialize();

            MCOS os = GetMCOS();
            string dir = PathManager.SystemResources_Textures_Control_Dir;

            ClientLocation = new(3, 3);
            BorderWidth = 8;
            Width = Application.MCOS.Screen.Width - 7;
            Height = Application.MCOS.Screen.Height - 10;

            Client_Panel.SubControls.Add(button1);
            button1.ClientLocation = new(5, 5);
            button1.Text = "Open";
            //button1.RightClick += Button1_RightClick;

            Client_Panel.SubControls.Add(textBox1);
            textBox1.ClientLocation = new(5, 25);
            textBox1.Text = "Text";

            Client_Panel.SubControls.Add(label1);
            label1.ClientLocation = new(5, 45);
            label1.Text = "lab";

            Client_Panel.SubControls.Add(switch1);
            switch1.ClientLocation = new(5, 65);

            Client_Panel.SubControls.Add(switch2);
            switch2.ClientLocation = new(5, 85);
            switch2.OnText = string.Empty;
            switch2.OffText = string.Empty;
            switch2.Skin.BackgroundImage = new(Path.Combine(dir, "OFF.png"), os.Screen.NormalFacing, switch2.ClientSize);
            switch2.Skin.BackgroundImage_Selected = new(Path.Combine(dir, "ON.png"), os.Screen.NormalFacing, switch2.ClientSize);
            switch2.Skin.BackgroundImage_Hover = new(Path.Combine(dir, "OFF过度.png"), os.Screen.NormalFacing, switch2.ClientSize);
            switch2.Skin.BackgroundImage_Hover_Selected = new(Path.Combine(dir, "ON过度.png"), os.Screen.NormalFacing, switch2.ClientSize);

            Client_Panel.SubControls.Add(comboButton1);
            comboButton1.ClientSize = new(110, 16);
            comboButton1.ClientLocation = Client_Panel.RightLayout(button1, 2, 3);
            comboButton1.Title = "方向";
            comboButton1.Items.Add(PlaneFacing.None);
            comboButton1.Items.Add(PlaneFacing.Top);
            comboButton1.Items.Add(PlaneFacing.Bottom);
            comboButton1.Items.Add(PlaneFacing.Right);
            comboButton1.Items.Add(PlaneFacing.Left);
        }

        int i = 0;
        private void Button1_RightClick(Point obj)
        {
            //((Test02App)GetApplication()).Run = false;


            switch (i)
            {
                case 0:
                    button1.MoveToRight(5);
                    break;
                case 1:
                    button1.MoveToBottom(5);
                    break;
                case 2:
                    button1.MoveToLeft(5);
                    break;
                case 3:
                    button1.MoveToTop(5);
                    break;
            }

            i++;
            if (i > 3)
                i = 0;
        }

        public override Frame RenderingFrame()
        {
            FrameBuilder fb = new(Frame.BuildFrame(Width, Height, ConcretePixel.ToBlockID(MinecraftColor.White)));
            CorrectSize(fb);
            return fb.ToFrame();
        }
    }
}