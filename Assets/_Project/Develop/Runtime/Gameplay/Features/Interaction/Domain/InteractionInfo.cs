using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public readonly struct InteractionInfo
    {
        public InteractionPromts Prompt { get; }
        public InteractionMode Mode { get; }
        public float HoldDuration { get; }
        public bool IsAvailable { get; }
        public long Revision { get; }
        private InteractionInfo(InteractionPromts prompt, InteractionMode mode,
            float holdDuration, bool isAvailable, long revision)
        {
            Prompt = prompt;
            Mode = mode;
            HoldDuration = holdDuration;
            IsAvailable = isAvailable;
            Revision = revision;
        }
        public static InteractionInfo Press(InteractionPromts prompt,
            bool isAvailable = true, long revision = 0)
        {
            return new InteractionInfo(prompt, InteractionMode.Press, 0f, isAvailable, revision);
        }
        public static InteractionInfo Hold(InteractionPromts prompt, float duration,
            bool isAvailable = true, long revision = 0)
        {
            if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
                throw new ArgumentOutOfRangeException(nameof(duration));
            return new InteractionInfo(prompt, InteractionMode.Hold, duration, isAvailable, revision);
        }
        public bool Matches(in InteractionInfo other)
        {
            return IsAvailable && other.IsAvailable &&
                   Prompt == other.Prompt && Mode == other.Mode &&
                   HoldDuration == other.HoldDuration && Revision == other.Revision;
        }
    }
}
