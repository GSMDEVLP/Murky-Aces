using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application
{
    public sealed class GpsRouteUpdater : IGameplayTickable, ITankRuntimeModule
    {
        private const float ARRIVAL_RADIUS = 3f;
        private const float REFRESH_INTERVAL = 0.25f;
        private const float MINIMUM_MOVEMENT = 1f;

        private readonly GpsRouteService _routeService;

        private float _timeUntilRefresh;
        private bool _isRegistered;

        public GpsRouteUpdater(GpsRouteService routeService)
        {
            _routeService = routeService ??
                throw new ArgumentNullException(nameof(routeService));
        }

        public void Register(GameLoopRegistry registry)
        {
            if (_isRegistered)
                return;

            _timeUntilRefresh = 0f;

            registry.RegisterGameplay(this);
            _isRegistered = true;
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterGameplay(this);

            _isRegistered = false;
            _timeUntilRefresh = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!_isRegistered || deltaTime <= 0f)
                return;

            if (_routeService.TryCompleteRoute(ARRIVAL_RADIUS))
            {
                _timeUntilRefresh = 0f;
                return;
            }

            _timeUntilRefresh -= deltaTime;

            if (_timeUntilRefresh > 0f)
                return;

            _routeService.TryRefreshRoute(MINIMUM_MOVEMENT);

            _timeUntilRefresh = REFRESH_INTERVAL;
        }
    }
}