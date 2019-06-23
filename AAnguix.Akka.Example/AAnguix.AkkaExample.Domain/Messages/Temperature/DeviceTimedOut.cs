namespace AAnguix.AkkaExample.Domain.Messages.Temperature
{
    public sealed class DeviceTimedOut : ITemperatureReading
    {
        public static DeviceTimedOut Instance { get; } = new DeviceTimedOut();
        private DeviceTimedOut() { }
    }
}