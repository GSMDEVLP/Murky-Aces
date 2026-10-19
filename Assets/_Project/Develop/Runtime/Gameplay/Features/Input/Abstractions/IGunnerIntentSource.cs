using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions
{
    public interface IGunnerIntentSource
    {
        GunnerIntentSnapshot ConsumeGunnerIntent();
    }
}