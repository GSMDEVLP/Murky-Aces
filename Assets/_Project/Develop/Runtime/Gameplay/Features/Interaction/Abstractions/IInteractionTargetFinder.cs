using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractionTargetFinder
    {
        bool TryFindTarget(
            IInteractionScope scope,
            in InteractionContext context,
            out IInteractable target);
    }
}