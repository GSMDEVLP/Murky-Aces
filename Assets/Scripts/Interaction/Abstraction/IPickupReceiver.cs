public interface IPickupReceiver
{
    bool IsOccupied { get; }

    bool TryReceive(IInteractable item);
    bool TryDrop();
}