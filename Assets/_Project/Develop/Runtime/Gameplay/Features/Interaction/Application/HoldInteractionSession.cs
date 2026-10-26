using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class HoldInteractionSession
    {
        public IInteractionTarget Target { get; }
        public IInteractionAction Action { get; }
        public InteractionInfo Info { get; }
        public long TargetRevision { get; }
        public long ActionRevision { get; }
        public float ElapsedSeconds { get; private set; }
        public float RequiredSeconds => Info.HoldDuration;
        public bool IsComplete => ElapsedSeconds >= RequiredSeconds;
        public HoldInteractionSession(IInteractionTarget target, IInteractionAction action, InteractionInfo info)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            if (info.Mode != InteractionMode.Hold || !info.IsAvailable)
                throw new ArgumentException("An available Hold action is required.", nameof(info));
            Info = info;
            TargetRevision = target.Revision;
            ActionRevision = action.Revision;
        }
        public void Advance(float deltaTime)
        {
            if (deltaTime > 0f && !float.IsNaN(deltaTime) && !float.IsInfinity(deltaTime))
                ElapsedSeconds += deltaTime;
        }
    }
}
