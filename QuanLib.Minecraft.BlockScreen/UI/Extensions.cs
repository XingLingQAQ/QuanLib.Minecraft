﻿using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.UI
{
    public static class Extensions
    {
        public static int IndexOf<T>(this IReadOnlyControlCollection<T> source, T item) where T : class, IControl
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == item)
                    return i;
            }
            return -1;
        }

        public static IForm? GetForm(this IControl source)
        {
            IControl? result = source;
            while (true)
            {
                if (result is null)
                    return null;
                else if (result is IForm form)
                    return form;
                else
                    result = result.GenericParentContainer;
            }
        }

        public static IRootForm? GetRootForm(this IControl source)
        {
            IControl? result = source;
            while (true)
            {
                if (result is null)
                    return null;
                else if (result is IRootForm form)
                    return form;
                else
                    result = result.GenericParentContainer;
            }
        }

        public static Point GetRenderingLocation(this IControlRendering source)
        {
            return new(source.ClientLocation.X + source.BorderWidth, source.ClientLocation.Y + source.BorderWidth);
        }
    }
}
