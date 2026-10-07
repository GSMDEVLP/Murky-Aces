using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Abstractions
{
    public interface IGunLauncher
    {
        bool TryLaunch(in GunShotRequest request);
    }
}