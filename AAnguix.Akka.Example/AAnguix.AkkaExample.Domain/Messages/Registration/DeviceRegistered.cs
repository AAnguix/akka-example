namespace AAnguix.AkkaExample.Domain.Messages.Registration
{
    /// <summary>
    /// Indicates that a new device has been successfully registered.
    /// </summary>
    public sealed class DeviceRegistered
    {
        public static DeviceRegistered Instance { get; } = new DeviceRegistered();
        private DeviceRegistered() { }
    }
}