using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public abstract class SingleActionInteractionTarget : InteractionTargetBehaviour, IInteractionAction
    {
        public InteractionActionId Id => InteractionActionId.Primary;
        public override void CollectActions(in InteractionContext context, List<IInteractionAction> actions)
        {
            if (IsActive) actions.Add(this);
        }
        public abstract InteractionInfo GetInteractionInfo(in InteractionContext context);
        public abstract bool Begin(in InteractionContext context);
        public abstract void Complete(in InteractionContext context);
        public abstract void Cancel(in InteractionContext context, InteractionCancelReason reason);
    }
}
