using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractable
    {
        InteractionInfo GetInteractionInfo(in InteractionContext context);

        bool Begin(in InteractionContext context);

        void Complete(in InteractionContext context);

        void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason);
    }
}
