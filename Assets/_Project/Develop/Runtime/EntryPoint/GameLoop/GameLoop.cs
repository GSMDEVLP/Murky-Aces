using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.EntryPoint.GameLoop
{
    public sealed class GameLoop : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;

        [Inject]
        public void Construct(GameLoopRegistry gameLoopRegistry)
        {
            _gameLoopRegistry = gameLoopRegistry;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            foreach (IInputTickable inputTickable in _gameLoopRegistry.InputRegistry)
                inputTickable.Tick(deltaTime);

            foreach (IGameplayTickable gameplayTickable in _gameLoopRegistry.GameplayRegistry)
                gameplayTickable.Tick(deltaTime);

            foreach (IPresentationTickable presentationTickable in _gameLoopRegistry.PresentationRegistry)
                presentationTickable.Tick(deltaTime);
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;

            foreach (IFixedGameplayTickable fixedGameplayTickable in _gameLoopRegistry.FixedGameplayRegistry)
                fixedGameplayTickable.FixedTick(fixedDeltaTime);
        }
    }
}
