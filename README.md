# µHttpSharp [![Build status](https://ci.appveyor.com/api/projects/status?id=1schhjbpx7oomrx7)](https://ci.appveyor.com/project/uhttpsharp)

A very lightweight & simple embedded http server for c# 

Usage : 

	using (var httpServer = new HttpServer(800, new HttpRequestProvider()))
	{
		httpServer.Use((context, next) => {
			Console.WriteLine("Got Request!");
			return next();
		});

		httpServer.Use(new TimingHandler());
		httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
										.With("about", new AboutHandler()));
		httpServer.Use(new FileHandler());
		httpServer.Use(new ErrorHandler());
		
		httpServer.Start();
		Console.ReadLine();
	}
	
## Under Construction

ÂµHttpSharp is going through heavy modifications to be more like [koa](http://koajs.com) :  

* ~~new `IHttpContext` interface (Gathers `IHttpResponse`, `IHttpRequest` and maybe even session)~~ (Done)
* ~~new `httpServer.Use((context, next) => { next(); });` syntax~~ (Done)

More modifications will be made to make it more "user friendly" out of the box :

* [RESTful](http://en.wikipedia.org/wiki/Representational_state_transfer) controllers
* NuGet package

## Performance

µHttpSharp manages to handle *3300 requests a sec* on core i5 machine, cpu goes to 27%, memory consumption and number of threads is stable.

That's nice, Not good as koa, But i'm going to try push it further.

## How To Contribute?

* Use it
* Open Issues
* Fork and Push requests



