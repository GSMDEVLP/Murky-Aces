namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractionInput
    {
        long ContextRevision { get; }
        bool InteractPressedThisFrame { get; }
        bool InteractHeld { get; }
        bool InteractReleasedThisFrame { get; }
        bool DropPressedThisFrame { get; }
        int InteractionMenuNavigation { get; }
        bool InteractionMenuCancelPressed { get; }
        void SetInteractionMenuOpen(bool isOpen);
        void SuppressInteractionUntilReleased();
    }
}
