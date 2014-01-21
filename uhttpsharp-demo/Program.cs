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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Listeners;
using uhttpsharpdemo.Handlers;

namespace uhttpsharpdemo
{
    internal static class Program
    {
        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();

            //var serverCertificate = X509Certificate.CreateFromCertFile(@"TempCert.cer");

            using (var httpServer = new HttpServer(new HttpRequestProvider()))
            {
                httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 82)));
                //httpServer.Use(new ListenerSslDecorator(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 443)), serverCertificate));

                httpServer.Use(new ExceptionHandler());
                httpServer.Use(new TimingHandler());
                
                httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
                                               .With("about", new AboutHandler())
                                               .With("strings", new RestHandler<string>(new StringsRestController(), new JsonResponseProvider())));

                httpServer.Use(new FileHandler());
                httpServer.Use(new ErrorHandler());
                httpServer.Use((context, next) =>
                {
                    Console.WriteLine("Got Request!");
                    return next();
                });

                httpServer.Start();
                Console.ReadLine();
            }
                
        }
    }
}