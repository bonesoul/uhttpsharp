# uhttpsharp [![Build status](https://ci.appveyor.com/api/projects/status?id=1schhjbpx7oomrx7)](https://ci.appveyor.com/project/uhttpsharp)

A very lightweight & simple embedded http server for c# 

Usage : 

	using (var httpServer = new HttpServer(800, new HttpRequestProvider()))
	{
		httpServer.Use(new TimingHandler());
		httpServer.Use(new HttpRouter().With(string.Empty, new IndexHandler())
										.With("about", new AboutHandler()));
		httpServer.Use(new FileHandler());
		httpServer.Use(new ErrorHandler());


		httpServer.Start();
		Console.ReadLine();
	}