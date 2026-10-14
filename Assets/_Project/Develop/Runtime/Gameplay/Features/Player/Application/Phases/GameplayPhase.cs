using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class GameplayPhase : IGameplayTickable, IFixedGameplayTickable
    {
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerLook _playerLook;
        private readonly PlayerInteraction _playerInteraction;
        private readonly PlayerStationController _stationController;
        private readonly PlayerGpsSession _gpsSession;

        public GameplayPhase(PlayerMovement playerMovement, 
            PlayerLook playerLook, 
            PlayerInteraction playerInteraction, 
            PlayerStationController stationController,
            PlayerGpsSession gpsSession)
        {
            _playerMovement = playerMovement;
            _playerLook = playerLook;
            _playerInteraction = playerInteraction;
            _stationController = stationController;
            _gpsSession = gpsSession;
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
            if (!_gpsSession.TryForceClose())
                return false;

            _playerInteraction.CancelActiveInteraction(
                InteractionCancelReason.ActorDisabled);

            return _stationController.TryReleaseStation();
        }
    }
}
