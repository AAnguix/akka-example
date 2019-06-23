using Akka.Actor;
using System;

namespace AAnguix.AkkaExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ActorSupervisionExample();
        }

        private static void ActorSupervisionExample()
        {
            using (var system = ActorSystem.Create("my-actor-system"))
            {
                var supervisingActor = system.ActorOf(Props.Create<SupervisingActor>(), "supervising-actor");
                supervisingActor.Tell("failChild");
                Console.ReadLine();
            }
        }
    }
}