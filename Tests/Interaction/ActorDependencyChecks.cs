using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Interior.Abstraction;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;
using UnityEngine;

static class ActorDependencyChecks
{
    public static void Run(Action<string, Action> run)
    {
        run("Actor accepts separate station occupant and exit input implementations", () => {
            var services = new Services();
            var occupant = new Occupant();
            var input = new ExitInput();
            var actor = new PlayerInteractionActor(1, services, occupant, input,
                services, services, services, services);
            Check(actor.IsInStation && actor.ConsumeExitRequest());
            Check(!actor.ConsumeExitRequest() && input.ReadCount == 2);
        });
        run("Actor still delegates station exit to the occupant", () => {
            var services = new Services();
            var occupant = new Occupant();
            var actor = new PlayerInteractionActor(1, services, occupant, new ExitInput(),
                services, services, services, services);
            Check(actor.TryExitStation(null) && occupant.ExitCount == 1);
        });
        run("Actor reports a missing exit input dependency explicitly", () => {
            var services = new Services();
            try
            {
                new PlayerInteractionActor(1, services, new Occupant(), null,
                    services, services, services, services);
            }
            catch (ArgumentNullException exception)
            {
                Check(exception.ParamName == "stationIntentSource");
                return;
            }
            throw new Exception("Expected ArgumentNullException.");
        });
    }

    static void Check(bool value)
    {
        if (!value) throw new Exception("Assertion failed.");
    }

    sealed class ExitInput : IStationIntentSource
    {
        public int ReadCount { get; private set; }
        public bool ConsumeExitRequest() => ++ReadCount == 1;
    }

    // This occupant intentionally has no input responsibilities.
    sealed class Occupant : IStationOccupant
    {
        public int ExitCount { get; private set; }
        public bool IsInStation => true;
        public bool TryEnterStation(long revision, CrewRoleId role,
            Transform seat, Transform camera, IInteractionScope scope,
            IStationCapabilityProfile profile) => true;
        public bool TryExitStation(Transform anchor) { ExitCount++; return true; }
        public bool TryForceExitStation(Transform anchor) => true;
        public bool TrySetReleaseHandler(Func<bool> handler) => true;
        public void ClearReleaseHandler(Func<bool> handler) { }
        public bool TryRollbackStationEntry() => true;
    }

    sealed class Services : IPickupReceiver, IDrivingIntentSource,
        IGunnerIntentSource, ITankInteriorOccupant, IPlayerGpsSession
    {
        public bool IsOccupied => false;
        public IHoldableItem HeldItem => null;
        public bool TryReceive(IHoldableItem item) => false;
        public bool TryReleaseHeldItem(IHoldableItem item) => false;
        public bool TryDrop() => false;
        public DrivingIntentSnapshot ReadDrivingIntent() => DrivingIntentSnapshot.Neutral;
        public GunnerIntentSnapshot ConsumeGunnerIntent() => GunnerIntentSnapshot.Neutral;
        public CrewLocation Current => CrewLocation.Outside;
        public long Revision => 0;
        public bool IsInInterior => false;
        public bool CanUseHatch => false;
        public bool TryEnterTank(long revision, Transform anchor) => false;
        public bool TryExitTank(long revision, Transform anchor) => false;
        public bool IsActive => false;
        public bool IsReady => false;
        public bool CanBegin(IGpsScreen screen, IInteractionActor actor) => false;
        public bool TryBegin(IGpsScreen screen, IInteractionActor actor) => false;
        public bool TryForceClose() => true;
    }
}
