/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Net.Sockets;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharpdemo.Handlers;

namespace uhttpsharpdemo
{
    internal static class Program
    {
        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            for (var port = 8000; port <= 65535; port++)
            {
                using (var httpServer = new HttpServer(port, new HttpRequestProvider()))
                {
                    httpServer.Use(new TimingHandler());
                    httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
                                                   .With("about", new AboutHandler()));
                    httpServer.Use(new FileHandler());
                    httpServer.Use(new ErrorHandler());

                    try
                    {
                        httpServer.Start();
                        Console.ReadLine();
                    }
                    catch (SocketException)
                    {
                        continue;
                    }
                }
                return;
            }
        }
    }
}