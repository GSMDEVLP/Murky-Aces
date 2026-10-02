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

        public GameplayPhase(PlayerMovement playerMovement, 
            PlayerLook playerLook, 
            PlayerInteraction playerInteraction, 
            PlayerStationController stationController)
        {
            _playerMovement = playerMovement;
            _playerLook = playerLook;
            _playerInteraction = playerInteraction;
            _stationController = stationController;
        }

        public void Tick(float deltaTime)
        {
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
            _playerInteraction.CancelActiveInteraction(
                InteractionCancelReason.ActorDisabled);

            return _stationController.TryReleaseStation();
        }
    }
}
