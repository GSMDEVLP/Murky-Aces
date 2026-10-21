using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [CreateAssetMenu(
        fileName = "ExteriorAttachmentConfig",
        menuName = "Murky Aces/Tank/Exterior Attachment Config")]
    public sealed class ExteriorAttachmentConfig : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _compatibleMountTypeId;
        [SerializeField] private bool _requiresWelding = true;
        [SerializeField] private GameObject _previewPrefab;
        [SerializeField, Min(0.1f)]
        private float _weldingDuration = 2f;

        public string Id => _id;
        public string DisplayName => _displayName;
        public GameObject PreviewPrefab => _previewPrefab;
        public float WeldingDuration => _weldingDuration;

        public string CompatibleMountTypeId =>
            _compatibleMountTypeId;

        public bool RequiresWelding => _requiresWelding;

        public bool IsValid =>
            string.IsNullOrWhiteSpace(_id) == false &&
            string.IsNullOrWhiteSpace(_compatibleMountTypeId) == false &&
            (_requiresWelding == false ||
            (_weldingDuration > 0f &&
            float.IsInfinity(_weldingDuration) == false));
    }
}
