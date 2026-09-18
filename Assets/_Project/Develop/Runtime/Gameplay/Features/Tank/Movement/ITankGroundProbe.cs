namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public interface ITankGroundProbe
    {
        TankGroundInfo Probe(float maxDistance);
    }
}