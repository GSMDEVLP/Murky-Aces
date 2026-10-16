using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure
{
    public sealed class GpsScreenInteractable : MonoBehaviour, IInteractable, IGpsScreen
    {
        [SerializeField] private CrewRoleId _role;
        [SerializeField] private Transform _viewAnchor;
        [SerializeField] private GpsMapView _mapView;
        [SerializeField, Min(0.00001f)] private float _cursorSensitivity = 0.002f;

        public float CursorSensitivity => Mathf.Max(0.00001f, _cursorSensitivity);

        public float ScreenAspect => _mapView != null ? _mapView.ScreenAspect : 1f;

        private CrewStationRegistry _stations;
        private IPlayerGpsSession  _session;
        private GpsRouteService _routeService;

        public CrewRoleId Role => _role;
        public Transform ViewAnchor => _viewAnchor;

        [Inject]
        public void Construct(CrewStationRegistry stations, GpsRouteService routeService)
        {
            _stations = stations;
            _routeService = routeService;
        }

        public bool CanUse(IInteractionActor actor)
        {
            return this != null &&
                    isActiveAndEnabled &&
                    _mapView != null &&
                    _mapView.HasMap &&
                    _viewAnchor != null &&
                    _viewAnchor.gameObject.activeInHierarchy &&
                    actor != null &&
                    _stations != null &&
                    (_role == CrewRoleId.Driver ||
                    _role == CrewRoleId.CommanderLoader) &&
                    _stations.TryGet(
                       _role,
                       out CrewStationController station) &&
                    station.IsOccupiedBy(actor.Id);
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            return InteractionInfo.Press(
                InteractionPromts.Enter,
                CanInteract(context.Actor));
        }

        public bool Begin(in InteractionContext context)
        {
            return CanInteract(context.Actor);
        }

        public void Complete(in InteractionContext context)
        {
            if (context.Actor is IGpsSessionOwner owner)
                owner.GpsSession.TryBegin(this, context.Actor);
        }

        public void Cancel(in InteractionContext context, InteractionCancelReason reason)
        {
        }

        public void AttachSession(IPlayerGpsSession session)
        {
            _session = session;
        }

        public void DetachSession(IPlayerGpsSession session)
        {
            if (ReferenceEquals(_session, session))
                _session = null;
        }

        private bool CanInteract(IInteractionActor actor)
        {
            return actor is IGpsSessionOwner owner &&
                   owner.GpsSession != null &&
                   owner.GpsSession.CanBegin(this, actor);
        }

        private void OnDisable()
        {
            _session?.TryForceClose();
        }
        public bool TryZoom(IInteractionActor actor, float zoomInput)
        {
            if (!CanUse(actor) ||
                _session == null ||
                !_session.IsReady ||
                _mapView == null ||
                !_mapView.HasMap)
            {
                return false;
            }

            _mapView.ApplyZoom(zoomInput);
            return true;
        }
        public bool TrySetCursor(IInteractionActor actor, Vector2 screenUv)
        {
            if (_role != CrewRoleId.CommanderLoader ||
                !CanUse(actor) ||
                _session == null ||
                !_session.IsReady ||
                _mapView == null ||
                !_mapView.HasMap)
            {
                return false;
            }

            _mapView.SetCursor(screenUv, true);
            return true;
        }

        public void HideCursor()
        {
            if (_mapView != null)
                _mapView.SetCursor(Vector2.zero, false);
        }

        public bool TrySelectDestination(IInteractionActor actor, Vector2 screenUv)
        {
            if (_role != CrewRoleId.CommanderLoader ||
                !CanUse(actor) ||
                _session == null ||
                !_session.IsReady ||
                _mapView == null ||
                _routeService == null)
            {
                return false;
            }

            if (!_mapView.TryScreenToWorld(
                    screenUv,
                    out Vector2 worldPosition))
            {
                return false;
            }

            return _routeService.TrySetDestination(
                actor,
                new GpsRoutePoint(worldPosition));
        }
        public bool TryClearRoute(IInteractionActor actor)
        {
            if (_role != CrewRoleId.CommanderLoader ||
                !CanUse(actor) ||
                _session == null ||
                !_session.IsReady ||
                _routeService == null)
            {
                return false;
            }

            return _routeService.TryClear(actor);
        }
    }
}