﻿using QuanLib.Core;
using QuanLib.Minecraft.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Senders
{
    public class CommandSender
    {
        public CommandSender(ITwowayCommandSender twowaySender, IOnewayCommandSender onewaySender)
        {
            TwowaySender = twowaySender ?? throw new ArgumentNullException(nameof(twowaySender));
            OnewaySender = onewaySender ?? throw new ArgumentNullException(nameof(onewaySender));

            CommandSent += OnCommandSent;
        }

        public ITwowayCommandSender TwowaySender { get; }

        public IOnewayCommandSender OnewaySender { get; }

        public event EventHandler<CommandSender, CommandInfoEventArgs> CommandSent;

        protected virtual void OnCommandSent(CommandSender sender, CommandInfoEventArgs e) { }

        public string SendCommand(string command)
        {
            string output = TwowaySender.SendCommand(command);
            CommandInfo commandInfo = new(command, output);
            CommandSent.Invoke(this, new(commandInfo));
            return output;
        }

        public async Task<string> SendCommandAsync(string command)
        {
            string output = await TwowaySender.SendCommandAsync(command);
            CommandInfo commandInfo = new(command, output);
            CommandSent.Invoke(this, new(commandInfo));
            return output;
        }
    }
}
