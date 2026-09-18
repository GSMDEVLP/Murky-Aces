using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Core.GameLoop.Application
{
    public sealed class GameLoopRegistry 
    {
        private readonly List<IInputTickable> _inputRegistry = new();
        private readonly List<IGameplayTickable> _gameplayRegistry = new();
        private readonly List<IFixedGameplayTickable> _fixedGameplayRegistry = new();
        private readonly List<IPresentationTickable> _presentationRegistry = new();

        public IReadOnlyList<IInputTickable> InputRegistry => _inputRegistry;
        public IReadOnlyList<IGameplayTickable> GameplayRegistry => _gameplayRegistry;
        public IReadOnlyList<IFixedGameplayTickable> FixedGameplayRegistry => _fixedGameplayRegistry;
        public IReadOnlyList<IPresentationTickable> PresentationRegistry => _presentationRegistry;
    
        public void RegisterInput(IInputTickable inputTickable)
        {
            if (inputTickable == null)
                throw new ArgumentNullException(nameof(inputTickable));

            if(_inputRegistry.Contains(inputTickable))
                return;
            _inputRegistry.Add(inputTickable);
        }

        public void RegisterGameplay(IGameplayTickable gameplayTickable)
        {
            if (gameplayTickable == null)
                throw new ArgumentNullException(nameof(gameplayTickable));

            if(_gameplayRegistry.Contains(gameplayTickable))
                return;
            _gameplayRegistry.Add(gameplayTickable);
        }

        public void RegisterFixedGameplay(IFixedGameplayTickable fixedGameplayTickable)
        {
            if (fixedGameplayTickable == null)
                throw new ArgumentNullException(nameof(fixedGameplayTickable));

            if(_fixedGameplayRegistry.Contains(fixedGameplayTickable))
                return;
            _fixedGameplayRegistry.Add(fixedGameplayTickable);
        }

        public void RegisterPresentation(IPresentationTickable presentationTickable)
        {
            if (presentationTickable == null)
                throw new ArgumentNullException(nameof(presentationTickable));

            if(_presentationRegistry.Contains(presentationTickable))
                return;
            _presentationRegistry.Add(presentationTickable);
        }

        public void UnregisterInput(IInputTickable inputTickable) =>_inputRegistry.Remove(inputTickable);

        public void UnregisterGameplay(IGameplayTickable gameplayTickable) => _gameplayRegistry.Remove(gameplayTickable);

        public void UnregisterFixedGameplay(IFixedGameplayTickable fixedGameplayTickable) => _fixedGameplayRegistry.Remove(fixedGameplayTickable);

        public void UnregisterPresentation(IPresentationTickable presentationTickable) => _presentationRegistry.Remove(presentationTickable);
    }
}
