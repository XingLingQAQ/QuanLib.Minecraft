﻿using QuanLib.Minecraft.BlockScreen.Frame;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.BlockForms
{
    public class PictureBox : Control
    {
        static PictureBox()
        {
            DefaultResizeOptions = new()
            {
                Size = new(64, 36),
                Mode = ResizeMode.Pad,
                Position = AnchorPositionMode.Center,
                Sampler = KnownResamplers.Bicubic
            };
        }

        public PictureBox()
        {
            ResizeOptions = DefaultResizeOptions.Copy();

            AutoSize = true;
            ContentAnchor = AnchorPosition.Centered;
        }

        public static ResizeOptions DefaultResizeOptions { get; }

        public ImageFrame? Image
        {
            get => _ImageFrame;
            set
            {
                if (_ImageFrame != value)
                {
                    _ImageFrame?.Dispose();
                    _ImageFrame = value;
                    Skin.SetAllBackgroundImage(_ImageFrame);
                    if (AutoSize)
                        AutoSetSize();
                    RequestUpdateFrame();
                }
            }
        }
        private ImageFrame? _ImageFrame;

        public ResizeOptions ResizeOptions { get; }

        public override IFrame RenderingFrame()
        {
            ArrayFrame? frame = Skin.GetBackgroundImage()?.GetFrame();
            if (frame is null)
                return base.RenderingFrame();
            return frame;
        }

        public override void AutoSetSize()
        {
            if (Image is null)
                ClientSize = ResizeOptions.Size;
            else
                ClientSize = Image.FrameSize;
        }

        public void SetImage(Image<Rgba32> image)
        {
            Image = new(image, GetScreenPlaneSize().NormalFacing, ResizeOptions);
        }

        public bool TryReadImageFile(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                Image = new(SixLabors.ImageSharp.Image.Load<Rgba32>(File.ReadAllBytes(path)), GetScreenPlaneSize().NormalFacing, ResizeOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}