using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Sandbox
{
public sealed class ToggleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractionPromts _prompt;
    [SerializeField] private GameObject _visual;

    public InteractionInfo GetInteractionInfo(in InteractionContext context)
    {
        return InteractionInfo.Press(_prompt, isActiveAndEnabled);
    }

    public bool Begin(in InteractionContext context)
    {
        return isActiveAndEnabled;
    }

    public void Complete(in InteractionContext context)
    {
        if (_visual == null)
            return;

        _visual.SetActive(!_visual.activeSelf);
    }

    public void Cancel(in InteractionContext context, InteractionCancelReason reason)
    {
    }
}
}
