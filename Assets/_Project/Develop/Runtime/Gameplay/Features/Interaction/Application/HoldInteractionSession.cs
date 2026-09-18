using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
public sealed class HoldInteractionSession
{
    public IInteractable Target { get; }
    public InteractionInfo Info { get; }
    public float ElapsedSeconds { get; private set; }

    public float RequiredSeconds => Info.HoldDuration;

    public bool IsComplete => ElapsedSeconds >= RequiredSeconds;

    public HoldInteractionSession(IInteractable target, InteractionInfo info)
    {
        Target = target ??
            throw new ArgumentNullException(nameof(target));

        if (info.Mode != InteractionMode.Hold)
        {
            throw new ArgumentException(
                "A hold session requires Hold interaction mode.",
                nameof(info));
        }

        Info = info;
    }

    public void Advance(float deltaTime)
    {
        if (deltaTime > 0f)
            ElapsedSeconds += deltaTime;
    }
}
}
