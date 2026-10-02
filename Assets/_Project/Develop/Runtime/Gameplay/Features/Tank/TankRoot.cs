using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank
{
    public sealed class TankRoot : MonoBehaviour
    {
        private GameLoopRegistry _gameLoopRegistry;
        private IReadOnlyList<ITankRuntimeModule> _modules;
        private bool _isRegistered;

        [Inject]
        public void Construct(
            GameLoopRegistry gameLoopRegistry,
            List<ITankRuntimeModule> modules)
        {
            _gameLoopRegistry = gameLoopRegistry ??
                throw new ArgumentNullException(nameof(gameLoopRegistry));

            if (modules == null || modules.Count == 0)
                throw new ArgumentException(
                    "At least one tank runtime module is required.",
                    nameof(modules));

            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] == null)
                    throw new ArgumentException(
                        "Tank runtime module cannot be null.",
                        nameof(modules));
            }

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
            if (_isRegistered ||
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