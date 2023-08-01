﻿using QuanLib.Minecraft.BlockScreen.UI;
using QuanLib.Minecraft.BlockScreen.BlockForms;
using QuanLib.Minecraft.BlockScreen.BlockForms.Utility;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.SystemApplications.DialogBox
{
    public abstract class DialogBoxForm : WindowForm
    {
        protected DialogBoxForm()
        {
            AllowDeselected = false;
            AllowResize = false;
            TitleBar.ButtonsToShow = FormButton.Close;

            _ButtonsToShow = DialogBoxReturnValue.Yes | DialogBoxReturnValue.No | DialogBoxReturnValue.Cancel;
            ButtonSize = new(32, 16);
        }

        public DialogBoxReturnValue ButtonsToShow
        {
            get => _ButtonsToShow;
            set
            {
                _ButtonsToShow = value;
                RefreshButtons();
            }
        }
        private DialogBoxReturnValue _ButtonsToShow;

        public Size ButtonSize { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            Client_Panel.Skin.SetAllBackgroundBlockID(ConcretePixel.ToBlockID(MinecraftColor.Lime));
        }

        public override void OnInitCompleted3()
        {
            base.OnInitCompleted3();

            RefreshButtons();
        }

        private void RefreshButtons()
        {
            Client_Panel.SubControls.Clear();
            if (ButtonsToShow.HasFlag(DialogBoxReturnValue.Yes))
            {
                Button button = new();
                button.Text = "是";
                button.ClientSize = ButtonSize;
                button.ClientLocation = Client_Panel.RightLayout(Client_Panel.SubControls.RecentlyAddedControl, 2, 18);
                button.RightClick += (sender, e) => Complete(DialogBoxReturnValue.Yes);
                Client_Panel.SubControls.Add(button);
            }
            if (ButtonsToShow.HasFlag(DialogBoxReturnValue.No))
            {
                Button button = new();
                button.Text = "否";
                button.ClientSize = ButtonSize;
                button.ClientLocation = Client_Panel.RightLayout(Client_Panel.SubControls.RecentlyAddedControl, 2, 18);
                button.RightClick += (sender, e) => Complete(DialogBoxReturnValue.No);
                Client_Panel.SubControls.Add(button);
            }
            if (ButtonsToShow.HasFlag(DialogBoxReturnValue.OK))
            {
                Button button = new();
                button.Text = "确认";
                button.ClientSize = ButtonSize;
                button.ClientLocation = Client_Panel.RightLayout(Client_Panel.SubControls.RecentlyAddedControl, 2, 18);
                button.RightClick += (sender, e) => Complete(DialogBoxReturnValue.OK);
                Client_Panel.SubControls.Add(button);
            }
            if (ButtonsToShow.HasFlag(DialogBoxReturnValue.Cancel))
            {
                Button button = new();
                button.Text = "取消";
                button.ClientSize = ButtonSize;
                button.ClientLocation = Client_Panel.RightLayout(Client_Panel.SubControls.RecentlyAddedControl, 2, 18);
                button.RightClick += (sender, e) => Complete(DialogBoxReturnValue.Cancel);
                Client_Panel.SubControls.Add(button);
            }

            int width = (Client_Panel.SubControls.RecentlyAddedControl?.RightLocation ?? 64) + 3;
            int height = 54;
            ClientSize = new(width, height);
        }

        private void Complete(DialogBoxReturnValue returnValue)
        {
            DialogBoxApp? app = GetProcess()?.Application as DialogBoxApp;
            if (app is not null)
                app.ReturnValue = returnValue;

            CloseForm();
        }
    }
}