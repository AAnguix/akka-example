using AAnguix.AkkaExample.Domain;
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
    public class DeviceGroupTests : TestKit
    {
        private readonly string _defaultGroup = "group";

        [Fact]
        public void DeviceGroup_WhenRequestTrackDevice_ThenDeviceActorRegistered()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props(_defaultGroup));

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device2"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor2 = probe.LastSender;
            deviceActor1.Should().NotBe(deviceActor2);

            // Check that the device actors are working
            deviceActor1.Tell(new RecordTemperature(requestId: 0, value: 1.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 0);
            deviceActor2.Tell(new RecordTemperature(requestId: 1, value: 2.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);
        }

        [Fact]
        public void DeviceGroup_WhenRequestTrackDeviceWithGroupId_ThenActorIgnoresRequest()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props(_defaultGroup));

            groupActor.Tell(new RequestTrackDevice("wrongGroup", "device1"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public void DeviceGroup_WhenMultipleRequestTrackDeviceForSameDevice_ThenReturnsSameActor()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props(_defaultGroup));

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor2 = probe.LastSender;

            deviceActor1.Should().Be(deviceActor2);
        }

        [Fact]
        public void DeviceGroup_WhenRequestDeviceListAndMultipleRegistered_ThenReturnsThem()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props(_defaultGroup));

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device2"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();

            groupActor.Tell(new RequestDeviceList(requestId: 0), probe.Ref);
            probe.ExpectMsg<ReplyDeviceList>(s => s.RequestId == 0
                && s.Ids.Contains("device1")
                && s.Ids.Contains("device2"));
        }

        [Fact]
        public void DeviceGroup_WhenOneDeviceShutdown_AndRequestDeviceList_ThenReturnsActiveDevices()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props(_defaultGroup));

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var toShutDown = probe.LastSender;

            groupActor.Tell(new RequestTrackDevice(_defaultGroup, "device2"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();

            groupActor.Tell(new RequestDeviceList(requestId: 0), probe.Ref);
            probe.ExpectMsg<ReplyDeviceList>(s =>
                s.RequestId == 0 && s.Ids.Contains("device1") && s.Ids.Contains("device2"));

            probe.Watch(toShutDown);
            toShutDown.Tell(PoisonPill.Instance);
            probe.ExpectTerminated(toShutDown);

            // using awaitAssert to retry because it might take longer for the groupActor
            // to see the Terminated, that order is undefined
            probe.AwaitAssert(() =>
            {
                groupActor.Tell(new RequestDeviceList(requestId: 1), probe.Ref);
                probe.ExpectMsg<ReplyDeviceList>(s => 
                    s.RequestId == 1 && s.Ids.Count == 1 && s.Ids.Contains("device2"));
            });
        }
    }
}