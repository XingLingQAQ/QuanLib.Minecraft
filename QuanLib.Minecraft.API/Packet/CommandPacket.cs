﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.API.Packet
{
    public static class CommandPacket
    {
        public static RequestPacket CreateRequestPacket(string command, int id, bool needResponse = true)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentException($"“{nameof(command)}”不能为 null 或空。", nameof(command));

            return new(PacketKey.Command, PacketType.String, Encoding.UTF8.GetBytes(command), id, needResponse);
        }

        public static string ParseResponsePacket(ResponsePacket responsePacket)
        {
            if (responsePacket is null)
                throw new ArgumentNullException(nameof(responsePacket));

            return Encoding.UTF8.GetString(responsePacket.Data);
        }

        public static async Task<string> SendCommandAsync(this MinecraftApiClient client, string command)
        {
            RequestPacket request = CreateRequestPacket(command, client.GetNextID(), true);
            ResponsePacket response = await client.SendPacke(request);
            return ParseResponsePacket(response);
        }
    }
}
