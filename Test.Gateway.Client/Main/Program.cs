﻿// Copyright (c) Cragon. All rights reserved.

namespace Test
{
    using DotNetty.Buffers;
    using DotNetty.Codecs;
    using DotNetty.Common.Internal.Logging;
    using DotNetty.Handlers.Tls;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {

        static async Task RunClientAsync()
        {
            IPAddress host = IPAddress.Parse("192.168.0.10");
            int port = 5882;

            var factory = new ClientSessionFactory();
            var group = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new LengthFieldPrepender(
                            ByteOrder.LittleEndian, 2, 0, false));
                        pipeline.AddLast(new LengthFieldBasedFrameDecoder(
                            ByteOrder.LittleEndian, ushort.MaxValue, 0, 2, 0, 2, true));
                        pipeline.AddLast(new ClientHandler(factory));
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(host, port));

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait(1000);
            }
        }

        static void Main() => RunClientAsync().Wait();
    }
}
