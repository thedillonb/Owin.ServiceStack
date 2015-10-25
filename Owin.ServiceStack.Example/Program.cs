using Microsoft.Owin.Hosting;
using ServiceStack.ServiceHost;
using System;

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
            app.UseServiceStack("Test", GetType().Assembly);
        }
    }

    [Route("/hello")]
    public class Hello
    {
        public string Name { get; set; }
    }

    public class HelloService : IService
    {
        public object Any(Hello hello)
        {
            return $"Hello, {hello.Name}!";
        }

    }
}
