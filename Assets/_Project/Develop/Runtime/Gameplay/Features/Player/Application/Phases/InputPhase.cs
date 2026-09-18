using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases
{
    public sealed class InputPhase : IInputTickable
    {
        private readonly PlayerInputSystem _playerInputSystem;

        public InputPhase(PlayerInputSystem playerInputSystem)
        {
            _playerInputSystem = playerInputSystem;
        }

        public void Tick(float deltaTime) => _playerInputSystem.Tick(deltaTime);
    }
}
