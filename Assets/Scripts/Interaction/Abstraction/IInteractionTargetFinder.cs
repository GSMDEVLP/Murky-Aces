public interface IInteractionTargetFinder
{
    bool TryFindTarget(out IInteractable target);
}