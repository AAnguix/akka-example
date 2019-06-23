using AAnguix.AkkaExample.Domain.Messages;
using AAnguix.AkkaExample.Domain.Messages.Temperature;
using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;

namespace AAnguix.AkkaExample.Domain.Actors
{
    public class DeviceGroupQuery : UntypedActor
    {
        private ICancelable queryTimeoutTimer;

        public DeviceGroupQuery(Dictionary<IActorRef, string> actorToDeviceId, long requestId, IActorRef requester, TimeSpan timeout)
        {
            ActorToDeviceId = actorToDeviceId;
            RequestId = requestId;
            Requester = requester;
            Timeout = timeout;

            //If we didn't get all the responses from the devices after some time, we send a CollectionTimeout message to the requester.
            queryTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, CollectionTimeout.Instance, Self);

            //We change the function that will handle messages.
            //WaitingForReplies will do the job, instead of OnReceive.
            Become(WaitingForReplies(new Dictionary<string, ITemperatureReading>(), new HashSet<IActorRef>(ActorToDeviceId.Keys)));
        }

        protected override void PreStart()
        {
            foreach (var deviceActor in ActorToDeviceId.Keys)
            {
                Context.Watch(deviceActor);
                deviceActor.Tell(new ReadTemperature(0));
            }
        }

        protected override void PostStop()
        {
            queryTimeoutTimer.Cancel();
        }

        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        public Dictionary<IActorRef, string> ActorToDeviceId { get; }
        public long RequestId { get; }
        public IActorRef Requester { get; }
        public TimeSpan Timeout { get; }

        public UntypedReceive WaitingForReplies(
            Dictionary<string, ITemperatureReading> repliesSoFar,
            HashSet<IActorRef> stillWaiting)
        {
            return message =>
            {
                switch (message)
                {
                    case RespondTemperature response when response.RequestId == 0:
                        var deviceActor = Sender;
                        ITemperatureReading reading = null;
                        if (response.Value.HasValue)
                        {
                            reading = new Temperature(response.Value.Value);
                        }
                        else
                        {
                            reading = TemperatureNotAvailable.Instance;
                        }
                        ReceivedResponse(deviceActor, reading, stillWaiting, repliesSoFar);
                        break;
                    case Terminated t:
                        ReceivedResponse(t.ActorRef, DeviceNotAvailable.Instance, stillWaiting, repliesSoFar);
                        break;
                    case CollectionTimeout _:
                        var replies = new Dictionary<string, ITemperatureReading>(repliesSoFar);
                        //We have reach te timeout, so we are not going to wait for these actors.
                        foreach (var actor in stillWaiting)
                        {
                            var deviceId = ActorToDeviceId[actor];
                            replies.Add(deviceId, DeviceTimedOut.Instance);
                        }
                        //We reply with what we have, and the actor is stopped.
                        Requester.Tell(new RespondAllTemperatures(RequestId, replies));
                        Context.Stop(Self);
                        break;
                }
            };
        }

        public void ReceivedResponse(
            IActorRef deviceActor,
            ITemperatureReading reading,
            HashSet<IActorRef> stillWaiting,
            Dictionary<string, ITemperatureReading> repliesSoFar)
        {
            Context.Unwatch(deviceActor);
            stillWaiting.Remove(deviceActor);

            var deviceId = ActorToDeviceId[deviceActor];
            repliesSoFar.Add(deviceId, reading);

            if (stillWaiting.Count == 0)
            {
                //We have received the responses from all the devices. We repply and the actor is stopped.
                Requester.Tell(new RespondAllTemperatures(RequestId, repliesSoFar));
                Context.Stop(Self);
            }
            else
            {
                Context.Become(WaitingForReplies(repliesSoFar, stillWaiting));
            }
        }

        protected override void OnReceive(object message)
        {
        }

        public static Props Props(Dictionary<IActorRef, string> actorToDeviceId, long requestId, IActorRef requester, TimeSpan timeout) =>
            Akka.Actor.Props.Create(() => new DeviceGroupQuery(actorToDeviceId, requestId, requester, timeout));
    }
}