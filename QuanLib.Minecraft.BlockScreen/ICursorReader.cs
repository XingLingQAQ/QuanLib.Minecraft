﻿using QuanLib.Minecraft.BlockScreen.Event;
using QuanLib.Minecraft.Data;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen
{
    public interface ICursorReader
    {
        /// <summary>
        /// 光标当前的位置
        /// </summary>
        public Point CurrentPosition { get; }

        /// <summary>
        /// 当前正在使用光标的玩家
        /// </summary>
        public string CurrentPlayer { get; }

        /// <summary>
        /// 与光标关联的物品
        /// </summary>
        public Item? CurrentItem { get; }

        /// <summary>
        /// 当光标移动时
        /// </summary>
        public event EventHandler<ICursorReader, CursorEventArgs> CursorMove;

        /// <summary>
        /// 当光标右键点击时
        /// </summary>
        public event EventHandler<ICursorReader, CursorEventArgs> RightClick;

        /// <summary>
        /// 当光标左键点击时
        /// </summary>
        public event EventHandler<ICursorReader, CursorEventArgs> LeftClick;

        /// <summary>
        /// 与光标关联的物品变化时触发
        /// </summary>
        public event EventHandler<ICursorReader, CursorItemEventArgs> CursorItemChanged;
    }
}