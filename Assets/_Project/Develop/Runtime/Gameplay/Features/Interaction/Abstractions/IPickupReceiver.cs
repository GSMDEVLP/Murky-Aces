namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IPickupReceiver
    {
        bool IsOccupied { get; }

        IHoldableItem HeldItem { get; }

        bool TryReceive(IHoldableItem item);
        bool TryDrop();

        bool TryReleaseHeldItem(IHoldableItem expectedItem);
    }
}
