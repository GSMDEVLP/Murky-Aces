using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public abstract class InteractionTargetBehaviour : MonoBehaviour, IInteractionTarget
    {
        public bool IsActive => this != null && isActiveAndEnabled;
        public long Revision { get; private set; }
        public abstract void CollectActions(in InteractionContext context, List<IInteractionAction> actions);
        protected virtual void OnEnable() => Revision++;
        protected virtual void OnDisable() => Revision++;
    }
}
