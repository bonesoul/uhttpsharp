# µHttpSharp

A very lightweight & simple embedded http server for c# 

Master | Provider
------ | --------
[![Build Status][AppVeyorImgMaster]][AppVeyorLinkMaster] | Windows CI Provided By [AppVeyor][]
[![Build Status][MonoImgMaster]][MonoLinkMaster] | Mono CI Provided by [travis-ci][] 

[MonoImgMaster]:https://travis-ci.org/shanielh/uHttpSharp.png?branch=master
[MonoLinkMaster]:https://travis-ci.org/shanielh/uHttpSharp
[AppVeyorLinkMaster]:https://ci.appveyor.com/project/uhttpsharp
[AppVeyorImgMaster]:https://ci.appveyor.com/api/projects/status?id=1schhjbpx7oomrx7

[travis-ci]:https://travis-ci.org/
[AppVeyor]:http://www.appveyor.com/

## Usage

Install via NuGet Package Manager :

	install-package uHttpSharp

A sample for usage : 

	using (var httpServer = new HttpServer(new HttpRequestProvider()))
	{
		// Normal port 80 :
		httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 80)));
        
		// Ssl Support :
		var serverCertificate = X509Certificate.CreateFromCertFile(@"TempCert.cer");
		httpServer.Use(new ListenerSslDecorator(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, 443)), serverCertificate));

		// Request handling : 
		httpServer.Use((context, next) => {
			Console.WriteLine("Got Request!");
			return next();
		});

		// Handler classes : 
		httpServer.Use(new TimingHandler());
		httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
										.With("about", new AboutHandler()));
		httpServer.Use(new FileHandler());
		httpServer.Use(new ErrorHandler());
		
		httpServer.Start();
		
		Console.ReadLine();
	}
	
## Features

µHttpSharp is a simple http server inspired by [koa](http://koajs.com), and has the following features :

* [RESTful](http://en.wikipedia.org/wiki/Representational_state_transfer) controllers
* Ssl Support
* Easy Chain-Of-Responsibility architecture


## Performance

µHttpSharp manages to handle **13000 requests a sec** (With Keep-Alive support) on core i5 machine, cpu goes to 27%, memory consumption and number of threads is stable.

	ab -n 10000 -c 50 -k -s 2 http://localhost:8000/
	
	This is ApacheBench, Version 2.3 <$Revision: 1528965 $>
	Copyright 1996 Adam Twiss, Zeus Technology Ltd, http://www.zeustech.net/
	Licensed to The Apache Software Foundation, http://www.apache.org/

	Benchmarking localhost (be patient)
	Completed 1000 requests
	Completed 2000 requests
	Completed 3000 requests
	Completed 4000 requests
	Completed 5000 requests
	Completed 6000 requests
	Completed 7000 requests
	Completed 8000 requests
	Completed 9000 requests


	Server Software:
	Server Hostname:        localhost
	Server Port:            8000

	Document Path:          /
	Document Length:        21 bytes

	Concurrency Level:      50
	Time taken for tests:   0.707 seconds
	Complete requests:      9357
	Failed requests:        0
	Keep-Alive requests:    9363
	Total transferred:      1507527 bytes
	HTML transferred:       196644 bytes
	Requests per second:    13245.36 [#/sec] (mean)
	Time per request:       3.775 [ms] (mean)
	Time per request:       0.075 [ms] (mean, across all concurrent requests)
	Transfer rate:          2083.53 [Kbytes/sec] received

	Connection Times (ms)
				  min  mean[+/-sd] median   max
	Connect:        0    0   0.0      0       1
	Processing:     1    4   0.7      4      13
	Waiting:        1    4   0.7      4      13
	Total:          1    4   0.7      4      13

	Percentage of the requests served within a certain time (ms)
	  50%      4
	  66%      4
	  75%      4
	  80%      4
	  90%      4
	  95%      4
	  98%      5
	  99%      9
	 100%      4 (longest request)

## How To Contribute?

* Use it
* Open Issues
* Fork and Push requests
