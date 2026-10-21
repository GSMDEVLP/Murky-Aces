using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Phases
{
    public sealed class GameplayPhase : IGameplayTickable, IFixedGameplayTickable
    {
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerLook _playerLook;
        private readonly PlayerInteraction _playerInteraction;
        private readonly PlayerStationController _stationController;
        private readonly PlayerGpsSession _gpsSession;
        private readonly HeldItemSlot _heldItemSlot;

        public GameplayPhase(
            PlayerMovement playerMovement,
            PlayerLook playerLook,
            PlayerInteraction playerInteraction,
            PlayerStationController stationController,
            PlayerGpsSession gpsSession,
            HeldItemSlot heldItemSlot)
        {
            _playerMovement = playerMovement;
            _playerLook = playerLook;
            _playerInteraction = playerInteraction;
            _stationController = stationController;
            _gpsSession = gpsSession;
            _heldItemSlot = heldItemSlot;
        }

        public void Tick(float deltaTime)
        {
            bool gpsWasActive = _gpsSession.IsActive;

            _gpsSession.Tick(deltaTime);

            if (gpsWasActive || _gpsSession.IsActive)
                return;

            _playerLook.Tick(deltaTime);
            _playerInteraction.Tick(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {

            _playerLook.FixedTick(fixedDeltaTime);
            _playerMovement.FixedTick(fixedDeltaTime);
        }

        public bool TryPrepareDespawn()
        {
            if (_gpsSession.TryForceClose() == false)
                return false;

            _playerInteraction.CancelActiveInteraction(
                InteractionCancelReason.ActorDisabled);

            if (_stationController.TryReleaseStation() == false)
                return false;

            return _heldItemSlot.TryPrepareDespawn();
        }
    }
}
