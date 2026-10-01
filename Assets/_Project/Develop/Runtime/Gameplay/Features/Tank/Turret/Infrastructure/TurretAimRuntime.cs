using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed partial class TurretAimRuntime : IGameplayTickable
    {
        private readonly TurretMechanism _mechanism;
        private readonly ITurretRig _rig;

        public TurretAimState State => _mechanism.State;
        public IPresentationTickable Presentation { get; }

        public TurretAimRuntime(TurretMechanism mechanism, ITurretRig rig)
        {
            _mechanism = mechanism ??
                throw new ArgumentNullException(nameof(mechanism));
            _rig = rig ??
                throw new ArgumentNullException(nameof(rig));

            Presentation = new TurretAimPresenter(this);
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
        }
    }
}