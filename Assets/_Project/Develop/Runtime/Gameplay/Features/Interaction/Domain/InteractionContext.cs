using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain
{
    public readonly struct InteractionContext
    {
        public IInteractionActor Actor { get; }

        // Сохраняем для совместимости с существующими interactable.
        public ulong InteractorId => Actor.Id;

        public InteractionContext(IInteractionActor actor)
        {
            Actor = actor
                ?? throw new ArgumentNullException(nameof(actor));
        }
    }
}