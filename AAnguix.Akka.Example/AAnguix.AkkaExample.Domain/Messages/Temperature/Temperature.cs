namespace AAnguix.AkkaExample.Domain.Messages.Temperature
{
    public sealed class Temperature : ITemperatureReading
    {
        public Temperature(double value)
        {
            Value = value;
        }

        public double Value { get; }
    }
}