# Owin.ServiceStack [![Build status](https://ci.appveyor.com/api/projects/status/12p77gegunmqlw5k?svg=true)](https://ci.appveyor.com/project/thedillonb/owin-servicestack)

Host ServiceStack (v3) as OWIN middleware. This library provides the necessary adapters to support any V3 ServiceStack application in an OWIN pipeline. The adapters convert the OWIN request/response objects into ServiceStack request/response objects and pass them to a special IAppHost implementation which does the rest!

## Use

Using Owin.ServiceStack is real simple. First, download from Nuget:

```
Install-Package Owin.ServiceStack
```

Now, simply add a `UseServiceStack` statement to your `IAppBuilder` with arguments similar to what you'd typicaly pass to your current `IAppHost` implementation.

```c#
public void Configuration(IAppBuilder app)
{
  // This creates a IAppHost implementation and passes "Test" as the service name, 
  // and the current assembly as locations to look for IService implementations
  app.UseServiceStack("Test", GetType().Assembly);
}
```

However, if you are currently inheriting a `IAppHost` implementation (such as `AppHostHttpListenerLongRunningBase` or `AppHostHttpListenerBase `, you can do the following:

```c#
// Make sure what ever implementation your AppHost was inheriting before is 
// modified to inherit from ServiceStackHost. This prevents ServiceStack
// from trying to use a HttpListener, or similar method, to listen for connections
class MyCustomHost : ServiceStackHost
{
  public override Configure(Container c)
  {
    c.Register<Thing>(...)
  }
}

// Your IAppBuilder configuration
public void Configuration(IAppBuilder app)
{
  // Create a IAppHost implementation
  var host = new MyCustomHost();
  host.Init();
  
  // Use an already constructed host and plug it into Owin
  app.UseServiceStack(host);
}
```

## License

The MIT License (MIT)

Copyright (c) 2015 Dillon Buchanan

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
