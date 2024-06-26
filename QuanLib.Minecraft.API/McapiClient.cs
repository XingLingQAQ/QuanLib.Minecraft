﻿using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Minecraft.API.Packet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.Minecraft.API
{
    public class McapiClient : UnmanagedRunnable
    {
        public McapiClient(IPAddress address, ushort port, ILoggerGetter? loggerGetter = null) : base(loggerGetter)
        {
            ArgumentNullException.ThrowIfNull(address, nameof(address));

            _address = address;
            _port = port;
            _packetid = 0;
            _tasks = new();
            _client = new();
            _synchronized = new();

            ReceivedPacket += OnReceivedPacket;
            Connected += OnConnected;
        }
 
        private readonly IPAddress _address;

        private readonly ushort _port;

        private int _packetid;

        private readonly ConcurrentDictionary<int, NetworkTask> _tasks;

        private readonly TcpClient _client;

        private readonly Synchronized _synchronized;

        public event EventHandler<McapiClient, EventArgs<ResponsePacket>> ReceivedPacket;

        public event EventHandler<McapiClient, EventArgs> Connected;

        protected virtual void OnReceivedPacket(McapiClient sender, EventArgs<ResponsePacket> e)
        {
            if (_tasks.TryGetValue(e.Argument.ID, out var task))
            {
                task.Receive(e.Argument);
                _tasks.TryRemove(e.Argument.ID, out _);
            }
            else
            {

            }
        }

        protected virtual void OnConnected(McapiClient sender, EventArgs e) { }

        protected override void Run()
        {
            MemoryStream cache = new();
            byte[] buffer = new byte[4096];
            int total = 0;
            int current = 0;

            try
            {
                _client.Connect(_address, _port);
                NetworkStream stream = _client.GetStream();
                stream.ReadTimeout = Timeout.Infinite;
                Connected.Invoke(this, EventArgs.Empty);

                while (IsRunning)
                {
                    int length = total == 0 ? 4 : Math.Min(total - current, buffer.Length);
                    int readLength = stream.Read(buffer, 0, length);

                    current += readLength;
                    if (total == 0)
                    {
                        stream.ReadTimeout = 30 * 1000;

                        if (current < 4)
                            continue;

                        total = BitConverter.ToInt32(buffer, 0);
                        if (total < 4)
                            throw new IOException($"数据包长度{total}小于最小长度4");
                    }

                    cache.Write(buffer, 0, readLength);

                    //Console.WriteLine($"总长度{total}，已读取长度{current}");

                    if (current < total)
                        continue;

                    HandleDataPacket(cache.ToArray());

                    cache.Dispose();
                    cache = new();
                    total = 0;
                    current = 0;
                    stream.ReadTimeout = Timeout.Infinite;
                }
            }
            catch
            {
                if (IsRunning)
                    throw;
            }
        }

        protected override void DisposeUnmanaged()
        {
            _client.Dispose();
        }

        public async Task<ResponsePacket> SendRequestPacketAsync(RequestPacket request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            if (!request.NeedResponse)
                throw new ArgumentException("request.NeedResponse is false", nameof(request));

            NetworkTask task = new(ThreadSafeWriteAsync, request);
            _tasks.TryAdd(request.ID, task);
            task.Send();
            ResponsePacket? response = await task.WaitForCompleteAsync();
            if (response is null)
            {
                if (task.State == NetworkTaskState.Timeout)
                    throw new InvalidOperationException("MCAPI请求超时");
                else
                    throw new InvalidOperationException("MCAPI数据包发送或接收失败");
            }
            return response;
        }

        public async Task SendOnewayRequestPacketAsync(RequestPacket request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            if (request.NeedResponse)
                throw new ArgumentException("request.NeedResponse is true", nameof(request));

            byte[] datapacket = request.Serialize();
            await ThreadSafeWriteAsync(datapacket);
        }

        public async Task<bool> LoginAsync(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));

            SemaphoreSlim semaphore = new(0);
            Connected += Release;
            if (!_client.Connected)
                await semaphore.WaitAsync();
            Connected -= Release;

            var result = await this.SendLoginAsync(password);
            return result.IsSuccessful ?? false;

            void Release(McapiClient sender, EventArgs e) => semaphore.Release();
        }

        private void HandleDataPacket(byte[] bytes)
        {
            if (ResponsePacket.TryDeserialize(bytes, out var response))
            {
                ReceivedPacket.Invoke(this, new(response));
            }
            else
            {

            }
        }

        public int GetNextID()
        {
            return Interlocked.Increment(ref _packetid);
        }

        internal async ValueTask ThreadSafeWriteAsync(byte[] datapacket)
        {
            ArgumentNullException.ThrowIfNull(datapacket, nameof(datapacket));

            await _synchronized.InvokeAsync(() => _client.GetStream().WriteAsync(datapacket));
        }
    }
}
