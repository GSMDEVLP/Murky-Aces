using UnityEngine;

public sealed class InteractionTargetLink : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _interactableBehaviour;

    public bool TryGetInteractable(out IInteractable interactable)
    {
        if (_interactableBehaviour == null)
        {
            interactable = null;
            return false;
        }

        interactable = _interactableBehaviour as IInteractable;
        return interactable != null;
    }

    private void OnValidate()
    {
        if (_interactableBehaviour == null)
            return;

        if (_interactableBehaviour is IInteractable)
            return;

        Debug.LogError(
            $"{_interactableBehaviour.name} must implement {nameof(IInteractable)}.",
            this);

        _interactableBehaviour = null;
    }
}