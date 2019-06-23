using Akka.Actor;
using System;

namespace AAnguix.AkkaExample
{
    public class SupervisedActor : UntypedActor
    {
        protected override void PreStart() => Console.WriteLine("supervised actor started");
        protected override void PostStop() => Console.WriteLine("supervised actor stopped");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "fail":
                    Console.WriteLine("supervised actor fails now");
                    throw new Exception("I failed!");
            }
        }
    }
}