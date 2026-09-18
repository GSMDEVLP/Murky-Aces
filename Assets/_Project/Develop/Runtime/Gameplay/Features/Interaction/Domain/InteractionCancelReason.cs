namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public enum InteractionCancelReason
    {
        InputReleased,
        TargetLost,
        TargetUnavailable,
        ContextChanged,
        ActorDisabled,
        Interrupted
    }
}
