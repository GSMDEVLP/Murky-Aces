using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Infrastructure
{
    public sealed class CombatTargetRegistryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CombatTargetRegistry>().AsSingle();
        }
    }
}