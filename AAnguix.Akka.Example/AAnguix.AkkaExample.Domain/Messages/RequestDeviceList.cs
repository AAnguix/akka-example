namespace AAnguix.AkkaExample.Domain.Messages
{
    public sealed class RequestDeviceList
    {
        public RequestDeviceList(long requestId)
        {
            RequestId = requestId;
        }

        public long RequestId { get; }
    }
}