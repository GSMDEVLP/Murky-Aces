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

        public string Id => _id;
        public string DisplayName => _displayName;
        public GameObject PreviewPrefab => _previewPrefab;

        public string CompatibleMountTypeId =>
            _compatibleMountTypeId;

        public bool RequiresWelding => _requiresWelding;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(_id) &&
            !string.IsNullOrWhiteSpace(_compatibleMountTypeId);
    }
}
