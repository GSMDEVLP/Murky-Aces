using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Sandbox
{
public sealed class TimedInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionPromts _prompt;

    [SerializeField, Min(0.1f)] private float _holdDuration = 2f;

    [SerializeField] private GameObject _visual;

    private ulong? _activeInteractorId;

    public InteractionInfo GetInteractionInfo(in InteractionContext context)
    {
        bool available =
            isActiveAndEnabled &&
            (!_activeInteractorId.HasValue ||
             _activeInteractorId.Value ==
             context.InteractorId);

        return InteractionInfo.Hold(
            _prompt,
            _holdDuration,
            available);
    }

    public bool Begin(in InteractionContext context)
    {
        Debug.Log($"Hold Begin: {context.InteractorId}", this);
        if (!isActiveAndEnabled)
            return false;

        if (_activeInteractorId.HasValue &&
            _activeInteractorId.Value !=
            context.InteractorId)
        {
            return false;
        }

        _activeInteractorId =
            context.InteractorId;

        return true;
    }

    public void Complete(in InteractionContext context)
    {
        Debug.Log($"Hold Complete: {context.InteractorId}", this);
        if (_activeInteractorId !=
            context.InteractorId)
        {
            return;
        }

        _activeInteractorId = null;

        if (_visual != null)
            _visual.SetActive(!_visual.activeSelf);
    }

    public void Cancel(in InteractionContext context, InteractionCancelReason reason)
    {
        Debug.Log($"Hold Cancel: {reason}", this);
        if (_activeInteractorId == context.InteractorId)
        {
            _activeInteractorId = null;
        }
    }

    private void OnDisable()
    {
        _activeInteractorId = null;
    }

    private void OnValidate()
    {
        if (_holdDuration < 0.1f)
            _holdDuration = 0.1f;
    }
}
}
