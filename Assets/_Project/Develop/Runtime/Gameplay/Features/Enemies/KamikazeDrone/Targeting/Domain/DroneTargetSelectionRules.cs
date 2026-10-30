using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Domain
{
    public static class DroneTargetSelectionRules
    {

        public static ulong Select(IReadOnlyList<DroneTargetCandidate> candidates, ulong currentTargetId)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            int index = FindBest(
                candidates,
                CombatTargetKind.Player,
                currentTargetId);

            if (index < 0)
            {
                index = FindBest(
                    candidates,
                    CombatTargetKind.Tank,
                    currentTargetId);
            }

            if (index < 0)
            {
                index = FindBest(
                    candidates,
                    CombatTargetKind.Monster,
                    currentTargetId);
            }

            return index >= 0 ? candidates[index].TargetId : 0;
        }

        private static int FindBest(
            IReadOnlyList<DroneTargetCandidate> candidates,
            CombatTargetKind kind,
            ulong preferredId)
        {
            int bestIndex = -1;

            for (int i = 0; i < candidates.Count; i++)
            {
                DroneTargetCandidate candidate = candidates[i];

                if (candidate.TargetId == 0 || candidate.Kind != kind)
                    continue;

                if (candidate.TargetId == preferredId)
                    return i;

                if (bestIndex < 0 ||
                    IsCloser(candidate, candidates[bestIndex]))
                {
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static bool IsCloser(
            DroneTargetCandidate candidate,
            DroneTargetCandidate other)
        {
            if (candidate.SqrDistanceToDrone != other.SqrDistanceToDrone)
                return candidate.SqrDistanceToDrone < other.SqrDistanceToDrone;

            return candidate.TargetId < other.TargetId;
        }

        private static bool ContainsTank(
            IReadOnlyList<DroneTargetCandidate> candidates,
            ulong targetId)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].TargetId == targetId &&
                    candidates[i].Kind == CombatTargetKind.Tank)
                {
                    return true;
                }
            }

            return false;
        }
    }
}