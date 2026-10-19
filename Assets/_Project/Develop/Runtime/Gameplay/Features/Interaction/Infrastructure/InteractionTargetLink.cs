using UnityEngine;
using UnityEngine.Serialization;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public sealed class InteractionTargetLink : MonoBehaviour
    {
        [FormerlySerializedAs("_interactableBehaviour")]
        [SerializeField] private MonoBehaviour _targetBehaviour;

        public bool TryGetTarget(out IInteractionTarget target)
        {
            target = null;

            if (!isActiveAndEnabled ||
                _targetBehaviour == null ||
                !_targetBehaviour.isActiveAndEnabled)
            {
                return false;
            }

            target = _targetBehaviour as IInteractionTarget;
            return target != null;
        }

        private void OnValidate()
        {
            if (_targetBehaviour == null ||
                _targetBehaviour is IInteractionTarget)
            {
                return;
            }

            Debug.LogError(
                $"{_targetBehaviour.name} must implement {nameof(IInteractionTarget)}.",
                this);

            _targetBehaviour = null;
        }
    }
}