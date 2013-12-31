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
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using uhttpsharp;

namespace uhttpsharpdemo
{
    internal static class Program
    {
        private static void Main()
        {
            for (var port = 8000; port <= 65535; ++port)
            {
                var httpServer = new HttpServer(port);

                httpServer.Use(new TimingHandler());
                httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
                                               .With("about", new AboutHandler()));
                httpServer.Use(new FileHandler());
                httpServer.Use(new ErrorHandler());

                try
                {
                    httpServer.Start();
                }
                catch (SocketException)
                {
                    continue;
                }
                break;
            }
            Console.ReadLine();
        }
    }

    public class TimingHandler : IHttpRequestHandler
    {

        public async System.Threading.Tasks.Task<HttpResponse> Handle(HttpRequest httpRequest, Func<System.Threading.Tasks.Task<HttpResponse>> next)
        {
            var stopWatch = Stopwatch.StartNew();
            var retVal = await next();
            Console.WriteLine(stopWatch.Elapsed);
            return retVal;
        }
    }
}