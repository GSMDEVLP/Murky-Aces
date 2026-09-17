using UnityEngine;
using Zenject;

public sealed class InteractionInstaller : MonoInstaller
{
    [SerializeField] private InteractionHudView _hudView;
    [SerializeField] private HeldItemSlot _heldItemSlot;
    [SerializeField] private PhysicsInteractionTargetFinder _targetFinder;

    public override void InstallBindings()
    {
        Container.Bind<IInteractionTargetFinder>().FromInstance(_targetFinder);
        Container.Bind<InteractionContext>().FromResolveGetter<PlayerFacade>(CreateInteractionContext).AsSingle();
        Container.Bind<PlayerInteraction>().AsSingle();
        Container.Bind<InteractionHudView>().FromInstance(_hudView);
        Container.Bind<InteractionHudPresenter>().AsSingle();
        Container.Bind<PresentationPhase>().AsSingle();
    }

    private InteractionContext CreateInteractionContext(PlayerFacade player)
    {
        ulong interactorId = EntityId.ToULong(player.GetEntityId());
        return new InteractionContext(interactorId, _heldItemSlot);
    }
}