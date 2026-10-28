using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application
{
    public sealed class TankRadarScanner : IGameplayTickable, ITankRuntimeModule
    {
        private readonly TankRadar _radar;
        private readonly RadarTargetRegistry _targets;
        private readonly ITankMotionBody _body;

        private readonly HashSet<string> _allowedTypes;
        private readonly List<RadarContact> _buffer = new();

        private readonly float _rangeSquared;
        private readonly float _scanInterval;
        private readonly ulong? _ownTargetId;
        private float _timeUntilNextScan;


        public bool IsRegistered { get; private set; }

        public TankRadarScanner(
            TankRadar radar,
            RadarTargetRegistry targets,
            ITankMotionBody body,
            RadarScanSettings settings,
            ulong? ownTargetId)
        {
            _radar = radar ??
                throw new ArgumentNullException(nameof(radar));

            _targets = targets ??
                throw new ArgumentNullException(nameof(targets));

            _body = body ??
                throw new ArgumentNullException(nameof(body));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _rangeSquared = settings.Range * settings.Range;
            _scanInterval = settings.ScanInterval;
            _ownTargetId = ownTargetId;

            _allowedTypes = new HashSet<string>(settings.TargetTypeIds, StringComparer.Ordinal);
        }

        public void Register(GameLoopRegistry registry)
        {
            _timeUntilNextScan = 0f;
            registry.RegisterGameplay(this);

            IsRegistered = true;
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterGameplay(this);

            IsRegistered = false;
            _timeUntilNextScan = 0f;
            _buffer.Clear();
            _radar.ClearContacts();
        }

        public void Tick(float deltaTime)
        {
            if (!IsRegistered || !_radar.IsEnabled)
            {
                _timeUntilNextScan = 0f;
                return;
            }

            if (deltaTime <= 0f)
                return;

            _timeUntilNextScan -= deltaTime;

            if (_timeUntilNextScan > 0f)
                return;

            Scan();

            _timeUntilNextScan += _scanInterval;

            if (_timeUntilNextScan <= 0f)
                _timeUntilNextScan = _scanInterval;
        }

        private void Scan()
        {
            _buffer.Clear();

            Vector3 origin = _body.Position;

            foreach (var target in _targets.Targets)
            {
                if (!target.IsAvailable)
                    continue;

                ulong targetId = target.TargetId;

                if (_ownTargetId.HasValue &&
                    targetId == _ownTargetId.Value)
                {
                    continue;
                }

                string typeId = target.TypeId;

                if (!_allowedTypes.Contains(typeId))
                    continue;

                Vector3 position = target.WorldPosition;
                Vector3 offset = position - origin;

                if (!(offset.sqrMagnitude <= _rangeSquared))
                    continue;

                _buffer.Add(new RadarContact(
                    targetId,
                    typeId,
                    position));
            }

            _radar.ReplaceContacts(_buffer);
        }
    }
}
