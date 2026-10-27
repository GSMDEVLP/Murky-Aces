using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Application
{
    public sealed class TurretAimRuntime : IGameplayTickable
    {
        private readonly TurretMechanism _mechanism;
        private readonly ITurretRig _rig;

        public TurretAimState State => _mechanism.State;

        public TurretAimRuntime(TurretMechanism mechanism, ITurretRig rig)
        {
            _mechanism = mechanism ??
                throw new ArgumentNullException(nameof(mechanism));
            _rig = rig ??
                throw new ArgumentNullException(nameof(rig));
        }

        public void SetInput(float traverse, float elevation)
        {
            _mechanism.SetInput(traverse, elevation);
        }

        public void ToggleReferenceMode()
        {
            _mechanism.ToggleReferenceMode(_rig.HullYaw);
        }

        public void ClearInput()
        {
            _mechanism.ClearInput();
        }

        public void Tick(float deltaTime)
        {
            _mechanism.Tick(deltaTime, _rig.HullYaw);

            _rig.ApplyAngles(
                _mechanism.State.CurrentYaw,
                _mechanism.State.CurrentPitch);
        }
    }
}
