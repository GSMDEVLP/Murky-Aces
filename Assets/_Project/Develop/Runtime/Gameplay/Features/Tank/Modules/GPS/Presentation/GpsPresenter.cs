using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly IGpsRouteReader _route;
        private readonly ITankMotionBody _body;
        private readonly GpsMapView _view;

        private Vector2 _lastForward = Vector2.up;

        public GpsPresenter(ITankMotionBody body, GpsMapView view, IGpsRouteReader route)
        {
            _body = body ??
                throw new ArgumentNullException(nameof(body));

            _view = view ??
                throw new ArgumentNullException(nameof(view));

            _route = route ??
                throw new ArgumentNullException(nameof(route));
        }

        public void Register(GameLoopRegistry registry)
        {
            HideMarker();
            registry.RegisterPresentation(this);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterPresentation(this);
            HideMarker();
        }

        public void Tick(float deltaTime)
        {
            if (_view == null || !_view.isActiveAndEnabled)
                return;

           GpsMapData layout = _view.DisplayedMap;

            if (layout == null)
            {
                HideMarker();
                return;
            }
            
            _view.SetRoute(_route);
            Vector3 position = _body.Position;
            Vector2 minimum = layout.Center - layout.Size * 0.5f;

            var centerUv = new Vector2(
                (position.x - minimum.x) / layout.Size.x,
                (position.z - minimum.y) / layout.Size.y);

            Vector3 forward = _body.Rotation * Vector3.forward;

            // Направление корпуса на горизонтальной плоскости.
            var horizontalForward = new Vector2(forward.x, forward.z);

            if (horizontalForward.sqrMagnitude > 0.0001f)
                _lastForward = horizontalForward.normalized;

            _view.SetNavigationPose(
                centerUv,
                _lastForward,
                true);
        }

        private void HideMarker()
        {
            if (_view != null)
            {
                _view.SetNavigationPose(
                    Vector2.zero,
                    Vector2.up,
                    false);
            }
        }
    }
}
