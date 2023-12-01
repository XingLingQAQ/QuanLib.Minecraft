﻿using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.ResourcePack.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.Command.Models
{
    public class KillCommand : CountCommandBase
    {
        public KillCommand()
        {
            Input = TextTemplate.Parse("kill %s");
            Output = LanguageManager.Instance["commands.kill.success.single"];
            CountOutput = LanguageManager.Instance["commands.kill.success.multiple"];
        }

        public override TextTemplate Input { get; }

        public override TextTemplate Output { get; }

        public override TextTemplate CountOutput { get; }

        public bool TrySendCommand(CommandSender sender, string target, out int result)
        {
            ArgumentException.ThrowIfNullOrEmpty(target, nameof(target));

            return base.TrySendCommand(sender, new object[] { target }, out result);
        }

        public override bool TryParseResult(string[] outargs, [MaybeNullWhen(false)] out int result)
        {
            return base.TryParseResult(outargs, 1, 0, out result);
        }
    }
}
