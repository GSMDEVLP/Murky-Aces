using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Crew.Abstraction
{
    public interface ICrewLocationReader
    {
        CrewLocation Current { get; }
        long Revision { get; }
    }
}