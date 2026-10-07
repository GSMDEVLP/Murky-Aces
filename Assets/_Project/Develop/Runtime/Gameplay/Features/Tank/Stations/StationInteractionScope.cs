using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    [DisallowMultipleComponent]
    public sealed class StationInteractionScope : MonoBehaviour, IInteractionScope
    {
        [Header("Allowed Object Hierarchies")]
        [SerializeField]
        private Transform[] _allowedRoots = Array.Empty<Transform>();

        [Header("Explicit Interaction Targets")]
        [SerializeField]
        private MonoBehaviour[] _allowedTargets = Array.Empty<MonoBehaviour>();

        public bool Allows(IInteractable target)
        {
            Component component = target as Component;

            if (component == null)
                return false;

            if (_allowedTargets != null)
            {
                foreach (MonoBehaviour allowedTarget in _allowedTargets)
                {
                    if (allowedTarget != null && allowedTarget == component)
                        return true;
                }
            }

            if (_allowedRoots != null)
            {
                foreach (Transform root in _allowedRoots)
                {
                    if (root == null)
                        continue;

                    if (component.transform == root ||
                        component.transform.IsChildOf(root))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}