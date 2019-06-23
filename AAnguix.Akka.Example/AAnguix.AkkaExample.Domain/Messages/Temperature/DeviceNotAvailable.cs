namespace AAnguix.AkkaExample.Domain.Messages.Temperature
{
    public sealed class DeviceNotAvailable : ITemperatureReading
    {
        public static DeviceNotAvailable Instance { get; } = new DeviceNotAvailable();
        private DeviceNotAvailable() { }
    }
}