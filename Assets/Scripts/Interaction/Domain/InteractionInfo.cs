using System;

public readonly struct InteractionInfo
{
    public InteractionPromts Prompt { get; }
    public InteractionMode Mode { get; }
    public float HoldDuration { get; }
    public bool IsAvailable { get; }

    private InteractionInfo(InteractionPromts prompt, InteractionMode mode, float holdDuration, bool isAvailable)
    {
        Prompt = prompt;
        Mode = mode;
        HoldDuration = holdDuration;
        IsAvailable = isAvailable;
    }

    public static InteractionInfo Press(InteractionPromts prompt, bool isAvailable = true)
    {
        return new InteractionInfo(prompt, InteractionMode.Press, 0f, isAvailable);
    }

    public static InteractionInfo Hold(InteractionPromts prompt, float duration, bool isAvailable = true)
    {
        if (duration <= 0f)
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Hold duration must be greater than zero.");

        return new InteractionInfo(prompt, InteractionMode.Hold, duration, isAvailable);
    }
}