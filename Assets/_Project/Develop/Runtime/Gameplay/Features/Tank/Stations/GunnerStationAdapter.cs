using System;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class GunnerStationAdapter : ICrewStationRoleAdapter
    {
        private readonly TurretAimRuntime _turret;
        private IGunnerIntentSource _intentSource;

        public bool IsActive => _intentSource != null;

        public GunnerStationAdapter(TurretAimRuntime turret)
        {
            _turret = turret ??
                throw new ArgumentNullException(nameof(turret));
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

            _turret.SetInput(
                intent.Traverse,
                intent.Elevation);

            if (intent.ToggleReferenceModeRequested)
                _turret.ToggleReferenceMode();

            // FireRequested и ZoomHeld подключим вместе
            // с орудием и GunnerCamera.
        }

        public void ClearOutput()
        {
            _turret.ClearInput();
        }

        public void Deactivate()
        {
            ClearOutput();
            _intentSource = null;
        }
    }
}