using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.EntryPoint.GameLoop
{
    public sealed class GameLoop : MonoBehaviour
    {
        private readonly List<IInputTickable> _inputSnapshot = new();
        private readonly List<IGameplayTickable> _gameplaySnapshot = new();
        private readonly List<IFixedGameplayTickable> _fixedGameplaySnapshot = new();
        private readonly List<IPresentationTickable> _presentationSnapshot = new();

        private GameLoopRegistry _gameLoopRegistry;

        [Inject]
        public void Construct(GameLoopRegistry gameLoopRegistry)
        {
            _gameLoopRegistry = gameLoopRegistry;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            Copy(_gameLoopRegistry.InputRegistry, _inputSnapshot);
            foreach (IInputTickable item in _inputSnapshot)
            {
                if (_gameLoopRegistry.IsInputRegistered(item))
                    item.Tick(deltaTime);
            }

            Copy(_gameLoopRegistry.GameplayRegistry, _gameplaySnapshot);
            foreach (IGameplayTickable item in _gameplaySnapshot)
            {
                if (_gameLoopRegistry.IsGameplayRegistered(item))
                    item.Tick(deltaTime);
            }

            Copy(_gameLoopRegistry.PresentationRegistry, _presentationSnapshot);
            foreach (IPresentationTickable item in _presentationSnapshot)
            {
                if (_gameLoopRegistry.IsPresentationRegistered(item))
                    item.Tick(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;

            Copy(_gameLoopRegistry.FixedGameplayRegistry, _fixedGameplaySnapshot);
            foreach (IFixedGameplayTickable item in _fixedGameplaySnapshot)
            {
                if (_gameLoopRegistry.IsFixedGameplayRegistered(item))
                    item.FixedTick(fixedDeltaTime);
            }
        }

        private static void Copy<T>(IReadOnlyList<T> source, List<T> destination)
        {
            destination.Clear();

            for (int i = 0; i < source.Count; i++)
                destination.Add(source[i]);
        }
    }
}