﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.ResourcePack
{
    public class ResourceEntry
    {
        public ResourceEntry(string modID)
        {
            if (string.IsNullOrEmpty(modID))
                throw new ArgumentException($"“{nameof(modID)}”不能为 null 或空。", nameof(modID));

            ModID = modID;
            Path = new(modID);
            BlockStates = new();
            BlockModels = new();
            BlockTextures = new();
            ItemModels = new();
            ItemTextures = new();
        }

        public string ModID { get; }

        public ResourcePath Path { get; }

        public Dictionary<string, ZipArchiveEntry> BlockStates { get; }

        public Dictionary<string, ZipArchiveEntry> BlockModels { get; }

        public Dictionary<string, ZipArchiveEntry> BlockTextures { get; }

        public Dictionary<string, ZipArchiveEntry> ItemModels { get; }

        public Dictionary<string, ZipArchiveEntry> ItemTextures { get; }

        public bool IsEmpty => !BlockStates.Any() && !BlockModels.Any() && !BlockTextures.Any() && !ItemModels.Any() && !ItemTextures.Any();
    }
}
