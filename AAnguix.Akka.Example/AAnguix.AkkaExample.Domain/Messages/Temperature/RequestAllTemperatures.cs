namespace AAnguix.AkkaExample.Domain.Messages.Temperature
{
    public sealed class RequestAllTemperatures
    {
        public RequestAllTemperatures(long requestId)
        {
            RequestId = requestId;
        }

        public long RequestId { get; }
    }
}