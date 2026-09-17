public readonly struct InteractionContext
{
    public ulong  InteractorId { get; }
    public IPickupReceiver PickupReceiver { get; }

    public InteractionContext(ulong  interactorId, IPickupReceiver pickupReceiver)
    {
        InteractorId = interactorId;
        PickupReceiver = pickupReceiver;
    }
}