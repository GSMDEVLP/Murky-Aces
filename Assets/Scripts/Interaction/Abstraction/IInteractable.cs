public interface IInteractable
{
    InteractionInfo GetInteractionInfo(in InteractionContext context);

    bool Begin(in InteractionContext context);

    void Complete(in InteractionContext context);

    void Cancel(
        in InteractionContext context,
        InteractionCancelReason reason);
}