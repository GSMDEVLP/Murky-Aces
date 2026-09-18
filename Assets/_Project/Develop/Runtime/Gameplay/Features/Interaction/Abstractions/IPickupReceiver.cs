namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IPickupReceiver
    {
        bool IsOccupied { get; }

        bool TryReceive(IInteractable item);
        bool TryDrop();
    }
}
