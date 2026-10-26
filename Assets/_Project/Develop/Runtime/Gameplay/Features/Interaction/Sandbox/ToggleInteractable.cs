using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Sandbox
{
public sealed class ToggleInteractable : SingleActionInteractionTarget
{
    [SerializeField] private InteractionPromts _prompt;
    [SerializeField] private GameObject _visual;

    public override InteractionInfo GetInteractionInfo(in InteractionContext context)
    {
        return InteractionInfo.Press(_prompt, isActiveAndEnabled);
    }

    public override bool Begin(in InteractionContext context)
    {
        return isActiveAndEnabled;
    }

    public override void Complete(in InteractionContext context)
    {
        if (_visual == null)
            return;

        _visual.SetActive(!_visual.activeSelf);
    }

    public override void Cancel(in InteractionContext context, InteractionCancelReason reason)
    {
    }
}
}
