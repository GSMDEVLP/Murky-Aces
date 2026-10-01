using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed partial class TurretAimRuntime
    {
        private sealed class TurretAimPresenter : IPresentationTickable
        {
            private readonly TurretAimRuntime _runtime;

            public TurretAimPresenter(TurretAimRuntime runtime)
            {
                _runtime = runtime;
            }

            public void Tick(float deltaTime)
            {
                _runtime._rig.ApplyAngles(
                    _runtime._mechanism.State.CurrentYaw,
                    _runtime._mechanism.State.CurrentPitch);
            }
        }
    }
}