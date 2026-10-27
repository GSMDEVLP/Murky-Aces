using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application
{
    public sealed class GunnerStationAdapter : ICrewStationRoleAdapter
    {
        private readonly TurretAimRuntime _turret;
        private readonly GunnerZoomState _zoom;
        private readonly TankFireRequestHandler _fire;

        private IGunnerIntentSource _intentSource;
        private ulong _actorId;

        public bool IsActive => _intentSource != null;

        public GunnerStationAdapter(
            TurretAimRuntime turret,
            GunnerZoomState zoom,
            TankFireRequestHandler fire)
        {
            _turret = turret ??
                throw new ArgumentNullException(nameof(turret));

            _zoom = zoom ??
                throw new ArgumentNullException(nameof(zoom));

            _fire = fire ??
                throw new ArgumentNullException(nameof(fire));
        }

        public bool CanUse(IInteractionActor actor)
        {
            return actor is IGunnerIntentSource;
        }

        public bool TryActivate(IInteractionActor actor)
        {
            if (IsActive)
                return false;

            _intentSource = actor as IGunnerIntentSource;

            if (_intentSource == null)
                return false;

            _actorId = actor.Id;
            ClearOutput();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_intentSource == null)
            {
                ClearOutput();
                return;
            }

            GunnerIntentSnapshot intent =
                _intentSource.ConsumeGunnerIntent();

            _zoom.SetHeld(intent.ZoomHeld);

            _turret.SetInput(
                intent.Traverse,
                intent.Elevation);

            if (intent.ToggleReferenceModeRequested)
                _turret.ToggleReferenceMode();

            if (intent.FireRequested)
                _fire.TryRequestFire(_actorId);
        }

        public void ClearOutput()
        {
            _turret.ClearInput();
            _zoom.Clear();
            _fire.CancelRequest(_actorId);
        }

        public void Deactivate()
        {
            ClearOutput();
            _intentSource = null;
            _actorId = 0;
        }
    }
}
