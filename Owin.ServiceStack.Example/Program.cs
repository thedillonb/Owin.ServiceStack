using Microsoft.Owin.Hosting;
using ServiceStack.ServiceHost;
using System;
using Funq;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceInterface;

namespace Owin.ServiceStack.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Program>("http://localhost:9000"))
            {
                Console.WriteLine("Application running on port 9000");
                Console.WriteLine("Press [enter] to quit...");
                Console.ReadLine();
            }
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseServiceStack(new Host());
        }
    }

    class Host : ServiceStackHost
    {
        public Host()
            :base("Test", typeof(Host).Assembly)
        {
        }

        protected override void Configure(Container container)
        {
            Plugins.Add(new global::ServiceStack.Razor.RazorFormat());

            var endpointHostConfig = new EndpointHostConfig
            {
                CustomHttpHandlers = {
                    { System.Net.HttpStatusCode.NotFound, new global::ServiceStack.Razor.RazorHandler("/notfound") }
                }
            };

            SetConfig(endpointHostConfig);
        }
    }

    [Route("/hello")]
    public class Hello
    {
        public string Name { get; set; }
    }

    [Route("/page")]
    public class Page
    {
    }

    public class HelloService : IService
    {
        public object Any(Hello hello)
        {
            return $"Hello, {hello.Name}!";
        }

        [DefaultView("Page")]
        public object Get(Page hello)
        {
            return new { Name = "Dillon" };
        }
    }
}
