using AAnguix.AkkaExample.Domain;
using AAnguix.AkkaExample.Domain.Messages.Registration;
using AAnguix.AkkaExample.Domain.Messages.Temperature;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using System;
using Xunit;

namespace AAnguix.AkkaExample.UnitTests
{
    /// <summary>
    /// How to write unit tests for akka actors: https://petabridge.com/blog/how-to-unit-test-akkadotnet-actors-akka-testkit/
    /// </summary>
    public class DeviceTest : TestKit
    {
        private readonly string _defaultGroup = "group";
        private readonly string _defaultDevice = "device";

        [Fact]
        public void DeviceActor_WhenNoTemperatureIsKnown_ThenRepliesWithEmptyReading()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props(_defaultGroup, _defaultDevice));

            deviceActor.Tell(new ReadTemperature(requestId: 42), probe.Ref);
            var response = probe.ExpectMsg<RespondTemperature>();
            response.RequestId.Should().Be(42);
            response.Value.Should().BeNull();
        }

        [Fact]
        public void DeviceActor_WhenTemperatureAsked_ThenRepluesWithLatestTemperature()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props(_defaultGroup, _defaultDevice));

            deviceActor.Tell(new RecordTemperature(requestId: 1, value: 24.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);

            deviceActor.Tell(new ReadTemperature(requestId: 2), probe.Ref);
            var response1 = probe.ExpectMsg<RespondTemperature>();
            response1.RequestId.Should().Be(2);
            response1.Value.Should().Be(24.0);

            deviceActor.Tell(new RecordTemperature(requestId: 3, value: 55.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 3);

            deviceActor.Tell(new ReadTemperature(requestId: 4), probe.Ref);
            var response2 = probe.ExpectMsg<RespondTemperature>();
            response2.RequestId.Should().Be(4);
            response2.Value.Should().Be(55.0);
        }

        [Fact]
        public void DeviceActor_WhenValidRegistrationRequest_ThenRepliesToIt()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props(_defaultGroup, _defaultDevice));

            deviceActor.Tell(new RequestTrackDevice(_defaultGroup, _defaultDevice), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            probe.LastSender.Should().Be(deviceActor);
        }

        [Fact]
        public void DeviceActor_WhenWrongRegistrationRequest_ThenIgnoresIt()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props(_defaultGroup, _defaultDevice));

            deviceActor.Tell(new RequestTrackDevice("wrongGroup", _defaultDevice), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));

            deviceActor.Tell(new RequestTrackDevice(_defaultGroup, "Wrongdevice"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }
    }
}