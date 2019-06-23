using AAnguix.AkkaExample.Domain;
using AAnguix.AkkaExample.Domain.Actors;
using AAnguix.AkkaExample.Domain.Messages;
using AAnguix.AkkaExample.Domain.Messages.Registration;
using AAnguix.AkkaExample.Domain.Messages.Temperature;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using System;
using Xunit;

namespace AAnguix.AkkaExample.UnitTests
{
    public class DeviceManagerTests : TestKit
    {
        [Fact]
        public void DeviceManager_WhenRequestTrackDevice_ThenDeviceGroupActorAndDeviceActorRegistered()
        {
            var probe = CreateTestProbe();
            var manager = Sys.ActorOf(DeviceManager.Props());

            manager.Tell(new RequestTrackDevice("group1", "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            manager.Tell(new RequestTrackDevice("group2", "device2"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor2 = probe.LastSender;
            deviceActor1.Should().NotBe(deviceActor2);
        }
    }
}