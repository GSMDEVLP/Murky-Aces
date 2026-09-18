using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class GameplayPhase : IGameplayTickable, IFixedGameplayTickable
    {
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerLook _playerLook;
        private readonly PlayerInteraction _playerInteraction;

        public GameplayPhase(PlayerMovement playerMovement, PlayerLook playerLook, PlayerInteraction playerInteraction)
        {
            _playerMovement = playerMovement;
            _playerLook = playerLook;
            _playerInteraction = playerInteraction;
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
    }
}
