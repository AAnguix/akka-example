namespace AAnguix.AkkaExample.Domain.Messages
{
    public sealed class CollectionTimeout
    {
        public static CollectionTimeout Instance { get; } = new CollectionTimeout();
        private CollectionTimeout() { }
    }
}