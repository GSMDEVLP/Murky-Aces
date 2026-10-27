using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Phases;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public sealed class InteractionInstaller : MonoInstaller
    {
        
        [SerializeField] private PlayerFacade _playerFacade;
        [SerializeField] private InteractionHudView _hudView;
        [SerializeField] private HeldItemSlot _heldItemSlot;
        [SerializeField] private PhysicsInteractionTargetFinder _targetFinder;
        [SerializeField]private ExteriorPlacementPreviewView _exteriorPreviewView;

        public override void InstallBindings()
        {
            Container.Bind<IInteractionTargetFinder>().FromInstance(_targetFinder);
            
            ulong actorId = EntityId.ToULong(_playerFacade.GetEntityId());
            _heldItemSlot.Initialize(actorId);
            Container.Bind<HeldItemSlot>().FromInstance(_heldItemSlot).AsSingle();
            Container.Bind<ExteriorPlacementPreviewView>().FromInstance(_exteriorPreviewView);
            Container.Bind<ExteriorPlacementPreviewPresenter>().AsSingle();
            Container.Bind<PlayerInteractionActor>().AsSingle().WithArguments(actorId,(IPickupReceiver)_heldItemSlot);
            Container.Bind<IInteractionActor>().FromResolveGetter<PlayerInteractionActor>(actor => actor).AsSingle();
            Container.Bind<InteractionContext>().FromResolveGetter<IInteractionActor>(CreateInteractionContext).AsSingle();
            Container.Bind<IInteractionInput>()
                .To<PlayerInteractionInput>()
                .AsSingle();
            Container.Bind<PlayerInteraction>().AsSingle();
            Container.Bind<InteractionHudView>().FromInstance(_hudView);
            Container.Bind<InteractionHudPresenter>().AsSingle();
            Container.Bind<PresentationPhase>().AsSingle();
        }

        private InteractionContext CreateInteractionContext(IInteractionActor actor)
        {
            return new InteractionContext(actor);
        }
    }
}
