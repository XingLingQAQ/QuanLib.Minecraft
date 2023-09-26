﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.GameResource
{
    public class AssetIndex
    {
        public AssetIndex(Model model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            if (!string.IsNullOrEmpty(model.hash))
                Hash = model.hash;
            else if (!string.IsNullOrEmpty(model.sha1))
                Hash = model.sha1;
            else
                throw new ArgumentException("哈希值不能为 null 或空", nameof(model));
            Size = model.size;
        }

        public AssetIndex(string hash, int size)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException($"“{nameof(hash)}”不能为 null 或空。", nameof(hash));

            Hash = hash;
            Size = size;
        }

        public string Hash { get; }

        public int Size { get; }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string hash { get; set; }

            public string sha1 { get; set; }

            public int size { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}