using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractionTarget
    {
        bool IsActive { get; }
        long Revision { get; }
        void CollectActions(in InteractionContext context, List<IInteractionAction> actions);
    }
}
