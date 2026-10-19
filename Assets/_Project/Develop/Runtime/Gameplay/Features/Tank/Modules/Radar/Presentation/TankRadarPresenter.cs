using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Presentation
{
    public sealed class TankRadarPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly TankRadar _radar;
        private readonly TankRadarScanner _scanner;
        private readonly ITankMotionBody _body;
        private readonly TankRadarDisplayView _view;

        private readonly Dictionary<string, Vector4> _colors =
            new(StringComparer.Ordinal);

        private readonly List<RadarContact> _displayContacts =
            new(TankRadarDisplayView.MaxContacts);

        private readonly Vector4[] _positions =
            new Vector4[TankRadarDisplayView.MaxContacts];

        private readonly Vector4[] _markerColors =
            new Vector4[TankRadarDisplayView.MaxContacts];

        private readonly Comparison<RadarContact> _compareContacts;
        private readonly float _inverseRange;
        private readonly float _rangeSquared;

        private Vector3 _origin;
        private Vector3 _lastForward = Vector3.forward;
        private bool _isRegistered;

        public TankRadarPresenter(
            TankRadar radar,
            TankRadarScanner scanner,
            ITankMotionBody body,
            TankRadarDisplayView view,
            TankRadarConfig config)
        {
            _radar = radar ??
                throw new ArgumentNullException(nameof(radar));

            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));

            _body = body ??
                throw new ArgumentNullException(nameof(body));

            _view = view ??
                throw new ArgumentNullException(nameof(view));

            if (config == null || !config.IsValid)
            {
                throw new ArgumentException(
                    "Radar config is missing or invalid.",
                    nameof(config));
            }

            _inverseRange = 1f / config.Range;
            _rangeSquared = config.Range * config.Range;
            _compareContacts = CompareByDistance;

            foreach (RadarTargetDefinition definition in config.TargetTypes)
            {
                Color color = definition.MarkerColor;

                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                    color = color.linear;

                _colors.Add(
                    definition.Id,
                    new Vector4(color.r, color.g, color.b, color.a));
            }
        }

        public void Register(GameLoopRegistry registry)
        {
            _view.TurnOff();

            registry.RegisterPresentation(this);
            _isRegistered = true;
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterPresentation(this);
            _isRegistered = false;
            _displayContacts.Clear();

            if (_view != null)
                _view.TurnOff();
        }

        public void Tick(float deltaTime)
        {
            if (_view == null || !_view.isActiveAndEnabled)
                return;

            if (!_isRegistered ||
                !_scanner.IsRegistered ||
                !_radar.IsEnabled)
            {
                _view.TurnOff();
                return;
            }

            _origin = _body.Position;

            // Проекция направления корпуса на горизонтальную плоскость.
            Vector3 forward = _body.Rotation * Vector3.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude > 0.0001f)
                _lastForward = forward.normalized;

            Vector3 right = new Vector3(
                _lastForward.z,
                0f,
                -_lastForward.x);

            _displayContacts.Clear();

            foreach (RadarContact contact in _radar.Contacts)
            {
                if (!_colors.ContainsKey(contact.TypeId))
                    continue;

                Vector3 offset = contact.WorldPosition - _origin;

                if (!(offset.sqrMagnitude <= _rangeSquared))
                    continue;

                _displayContacts.Add(contact);
            }

            // При переполнении экрана выбираем ближайшие контакты.
            if (_displayContacts.Count > TankRadarDisplayView.MaxContacts)
                _displayContacts.Sort(_compareContacts);

            int count = Math.Min(
                _displayContacts.Count,
                TankRadarDisplayView.MaxContacts);

            for (int i = 0; i < count; i++)
            {
                RadarContact contact = _displayContacts[i];
                Vector3 offset = contact.WorldPosition - _origin;

                _positions[i] = new Vector4(
                    Vector3.Dot(offset, right) * _inverseRange,
                    Vector3.Dot(offset, _lastForward) * _inverseRange,
                    0f,
                    0f);

                _markerColors[i] = _colors[contact.TypeId];
            }

            _view.Render(
                true,
                _positions,
                _markerColors,
                count);
        }

        private int CompareByDistance(RadarContact left, RadarContact right)
        {
            float leftDistance =
                (left.WorldPosition - _origin).sqrMagnitude;

            float rightDistance =
                (right.WorldPosition - _origin).sqrMagnitude;

            int result = leftDistance.CompareTo(rightDistance);

            return result != 0
                ? result
                : left.TargetId.CompareTo(right.TargetId);
        }
    }
}