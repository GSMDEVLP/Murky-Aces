using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Infrastructure
{
    public sealed class KamikazeDroneRoot : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;
        private IReadOnlyList<IDroneRuntimeModule> _modules;
        private bool _isRegistered;

        [Inject]
        public void Construct(GameLoopRegistry gameLoopRegistry, List<IDroneRuntimeModule> modules)
        {
            if (gameLoopRegistry == null)
                throw new ArgumentNullException(nameof(gameLoopRegistry));

            if (modules == null || modules.Count == 0)
                throw new ArgumentException(
                    "At least one drone runtime module is required.",
                    nameof(modules));

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] == null)
                    throw new ArgumentException(
                        "Drone runtime module cannot be null.",
                        nameof(modules));
            }

            Unregister();
            _gameLoopRegistry = gameLoopRegistry;
            _modules = modules.AsReadOnly();
            TryRegister();
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void TryRegister()
        {
            if (isActiveAndEnabled == false ||
                _isRegistered ||
                _gameLoopRegistry == null ||
                _modules == null)
            {
                return;
            }

            for (int i = 0; i < _modules.Count; i++)
                _modules[i].Register(_gameLoopRegistry);

            _isRegistered = true;
        }

        private void Unregister()
        {
            if (_isRegistered == false)
                return;

            for (int i = 0; i < _modules.Count; i++)
                _modules[i].Unregister(_gameLoopRegistry);

            _isRegistered = false;
        }
    }
}
