using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Crew.Abstractions
{
    public interface ICrewLocationReader
    {
        CrewLocation Current { get; }
        long Revision { get; }
    }
}