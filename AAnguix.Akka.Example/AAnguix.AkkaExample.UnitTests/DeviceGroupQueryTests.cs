﻿using AAnguix.AkkaExample.Domain.Actors;
using AAnguix.AkkaExample.Domain.Messages.Temperature;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akka.Util.Internal;
using System;
using System.Collections.Generic;
using Xunit;

namespace AAnguix.AkkaExample.UnitTests
{
    public class DeviceGroupQueryTests : TestKit
    {
        [Fact]
        public void DeviceGroupQuery_WhenSomeWorkingDevices_ThenReturnsTemperatureOfAllOfThem()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }

        [Fact]
        public void DeviceGroupQuery_WhenSomeDeviceHasNoReading_ThenReturnsTemperatureNotAvailable()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: null), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"] is TemperatureNotAvailable &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }

        [Fact]
        public void DeviceGroupQuery_WhenDeviceStoppedBeforeAnswering_ThenReturnsDeviceNotAvailable()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
            device2.Tell(PoisonPill.Instance);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"] is DeviceNotAvailable &&
                msg.RequestId == 1);
        }

        [Fact]
        public void DeviceGroupQuery_WhenDeviceStoppedAfterAnswering_ThenReturnsReadedTemperature()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);
            device2.Tell(PoisonPill.Instance);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }

        [Fact]
        public void DeviceGroupQuery_WhenDeviceDoesNotAnswerInTime_ThenReturnsDeviceTimedOut()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(1)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"] is DeviceTimedOut &&
                msg.RequestId == 1);
        }
    }
}