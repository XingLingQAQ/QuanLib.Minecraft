﻿using QuanLib.Minecraft.BlockScreen.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.BlockScreen.BuiltInApps.DataScreen
{
    public class DataScreenForm : WindowForm
    {
        public DataScreenForm()
        {
            DayTimeSyncTime = 16;
            GameTimeSyncTime = 1200;
            DayTimeSyncCountdown = 0;
            GameTimeSyncCountdown = 0;

            DayTime_Label = new();
            GameTime_Label = new();
        }

        private readonly Label DayTime_Label;

        private readonly Label GameTime_Label;

        public int DayTimeSyncTime { get; set; }

        public int GameTimeSyncTime { get; set; }

        public int DayTimeSyncCountdown { get; private set; }

        public int GameTimeSyncCountdown { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            Skin.SetAllBackgroundBlockID(ConcretePixel.ToBlockID(MinecraftColor.LightBlue));
            Client_Panel.Skin.SetAllBackgroundBlockID(ConcretePixel.ToBlockID(MinecraftColor.LightBlue));
            BeforeFrame += DataForm_BeforeFrame;
            DataForm_BeforeFrame();

            Client_Panel.SubControls.Add(DayTime_Label);
            DayTime_Label.ClientLocation = new(2, 2);
            DayTime_Label.Skin.SetAllBackgroundBlockID(Skin.BackgroundBlockID);

            Client_Panel.SubControls.Add(GameTime_Label);
            GameTime_Label.ClientLocation = new(2, 20);
            GameTime_Label.Skin.SetAllBackgroundBlockID(Skin.BackgroundBlockID);
        }

        private void DataForm_BeforeFrame()
        {
            DayTimeSyncCountdown--;
            if (DayTimeSyncCountdown <= 0)
            {
                ServerCommandHelper command = GetMCOS().MinecraftServer.CommandHelper;
                int gameDays = command.GetGameDays().Result;
                TimeSpan dayTime = MinecraftUtil.DayTimeToTimeSpan(command.GetDayTime().Result);
                DayTime_Label.Text = $"游戏时间：{gameDays}日{(int)dayTime.TotalHours}时{dayTime.Minutes}分";
                DayTimeSyncCountdown = DayTimeSyncTime;
            }

            GameTimeSyncCountdown--;
            if (GameTimeSyncCountdown <= 0)
            {
                TimeSpan gameTime = MinecraftUtil.GameTicksToTimeSpan(GetMCOS().MinecraftServer.CommandHelper.GetGameTime().Result);
                GameTime_Label.Text = $"开服时长：{gameTime.Days}天{gameTime.Hours}小时{gameTime.Minutes}分钟";
                GameTimeSyncCountdown = GameTimeSyncTime;
            }
        }

        public override Frame RenderingFrame()
        {
            return Frame.BuildFrame(Width, Height, Skin.BackgroundBlockID);
        }
    }
}