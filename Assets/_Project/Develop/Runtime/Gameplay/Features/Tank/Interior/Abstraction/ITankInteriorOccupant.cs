using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstraction;


namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Interior.Abstraction
{
    public interface ITankInteriorOccupant : ICrewLocationReader
    {
        bool IsInInterior { get; }
        bool CanUseHatch { get; }

        bool TryEnterTank(
            long expectedRevision,
            Transform insideAnchor);

        bool TryExitTank(
            long expectedRevision,
            Transform outsideAnchor);
    }
}