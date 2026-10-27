using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Updaters;
using GameLoopTickable =
    _Project.Develop.Runtime.Core.GameLoop.Abstractions.ITickable;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure
{
    public sealed class PlayerInputSystem : MonoBehaviour, GameLoopTickable
    {
        [SerializeField] private float _mouseLookDegreesPerPixel = 0.15f;
        [SerializeField] private float _stickLookDegreesPerSecond = 180f;

        private CommonInputUpdater _commonInput;
        private IReadOnlyList<IInputIntentUpdater> _updaters;

        [Inject]
        public void Construct(
            CommonInputUpdater commonInput,
            List<IInputIntentUpdater> updaters)
        {
            _commonInput = commonInput ??
                throw new ArgumentNullException(nameof(commonInput));

            if (updaters == null)
                throw new ArgumentNullException(nameof(updaters));

            _updaters = updaters.AsReadOnly();
        }

        public void Tick(float deltaTime)
        {
            _commonInput.UpdateIntent(
                deltaTime,
                _mouseLookDegreesPerPixel,
                _stickLookDegreesPerSecond);

            for (int i = 0; i < _updaters.Count; i++)
                _updaters[i].UpdateIntent();
        }
    }
}
