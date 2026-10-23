using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [CreateAssetMenu(fileName = "ExteriorAttachmentConfig", menuName = "Murky Aces/Tank/Exterior Attachment Config")]
    public sealed class ExteriorAttachmentConfig : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private ExteriorMountType  _compatibleMountType;
        [SerializeField] private bool _requiresWelding = true;
        [SerializeField] private GameObject _previewPrefab;
        [SerializeField, Min(0.1f)]
        private float _weldingDuration = 2f;

        public string DisplayName => _displayName;
        public GameObject PreviewPrefab => _previewPrefab;
        public float WeldingDuration => _weldingDuration;

        public ExteriorMountType  CompatibleMountType => _compatibleMountType;

        public bool RequiresWelding => _requiresWelding;

        public bool IsValid =>
            _compatibleMountType != ExteriorMountType.None &&
            System.Enum.IsDefined(
                typeof(ExteriorMountType),
                _compatibleMountType) &&
            (!_requiresWelding ||
                (_weldingDuration > 0f &&
                !float.IsInfinity(_weldingDuration)));
    }
}
