
namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractionTargetFinder
    {
        bool TryFindTarget(IInteractionScope scope, out IInteractable target);
    }
}