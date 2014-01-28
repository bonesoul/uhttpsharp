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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;
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

                //httpServer.Use(new SessionHandler<DateTime>(() => DateTime.Now));
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


    internal class SessionHandler<TSession> : IHttpRequestHandler
    {
        private readonly Func<TSession> _sessionFactory;

        private static readonly Random RandomGenerator = new Random();

        private class SessionHolder
        {
            private readonly TSession _session;
            private DateTime _lastAccessTime = DateTime.Now;

            public TSession Session
            {
                get
                {
                    _lastAccessTime = DateTime.Now;
                    return _session;
                }
            }

            public DateTime LastAccessTime
            {
                get
                {
                    return _lastAccessTime;
                }
            }

            public SessionHolder(TSession session)
            {
                _session = session;
            }
        }

        private readonly ConcurrentDictionary<string, TSession> _sessions = new ConcurrentDictionary<string, TSession>();

        public SessionHandler(Func<TSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public System.Threading.Tasks.Task Handle(IHttpContext context, Func<System.Threading.Tasks.Task> next)
        {

            string sessId;
            if (!context.Cookies.TryGetByName("SESSID", out sessId))
            {
                sessId = RandomGenerator.Next().ToString(CultureInfo.InvariantCulture);
                context.Cookies.Upsert("SESSID", sessId);
            }

            var session = _sessions.GetOrAdd(sessId, CreateSession);

            context.State.Session = session;

            return next();
        }
        private TSession CreateSession(string sessionId)
        {
            return _sessionFactory();
        }
    }
}