﻿using QuanLib.Minecraft.BlockScreen;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.Controls
{
    public class TextBox : Control
    {
        public TextBox()
        {
            ClientSize = new(64, 16);
            Skin.BackgroundBlockID_Selected = ConcretePixel.ToBlockID(MinecraftColor.LightBlue);
            Skin.BackgroundBlockID_Hover_Selected = ConcretePixel.ToBlockID(MinecraftColor.LightBlue);
            Skin.BorderBlockID_Selected = ConcretePixel.ToBlockID(MinecraftColor.Blue);
            Skin.BorderBlockID_Hover_Selected = ConcretePixel.ToBlockID(MinecraftColor.Blue);

            CursorEnter += TextBox_CursorEnter;
            CursorLeave += TextBox_CursorLeave;
            TextEditorUpdate += TextBox_TextEditorUpdate;
        }

        private void TextBox_TextEditorUpdate(Point position, string text)
        {
            Text = text;
        }

        private void TextBox_CursorEnter(Point position, CursorMode mode)
        {
            if (mode == CursorMode.TextEditor)
            {
                IsSelected = true;
                SetTextEditorInitialText();
                ResetTextEditor();
            }
        }

        private void TextBox_CursorLeave(Point position, CursorMode mode)
        {
            if (IsSelected)
                IsSelected = false;
        }
    }
}