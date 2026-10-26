using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public static class InteractionActions
    {
        public static void CollectAvailable(IInteractionTarget target,
            in InteractionContext context, List<IInteractionAction> actions)
        {
            actions.Clear();
            if (target == null || !target.IsActive) return;
            target.CollectActions(context, actions);
            for (int i = actions.Count - 1; i >= 0; i--)
                if (actions[i] == null || !actions[i].GetInteractionInfo(context).IsAvailable)
                    actions.RemoveAt(i);
            for (int i = 0; i < actions.Count; i++)
            for (int j = i + 1; j < actions.Count; j++)
                if (actions[i].Id == actions[j].Id)
                    throw new InvalidOperationException("Action IDs must be unique within an interaction target.");
        }
        public static bool Contains(List<IInteractionAction> actions, IInteractionAction expected)
        {
            for (int i = 0; i < actions.Count; i++)
                if (ReferenceEquals(actions[i], expected)) return true;
            return false;
        }
    }
}
