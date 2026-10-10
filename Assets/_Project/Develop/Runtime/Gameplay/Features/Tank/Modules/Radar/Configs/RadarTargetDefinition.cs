using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Configs
{
    [CreateAssetMenu(fileName = "RadarTargetDefinition", menuName = "Murky Aces/Tank/Radar/Target Definition")]
    public sealed class RadarTargetDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private Color _markerColor = Color.green;

        public string Id => _id;
        public Color MarkerColor => _markerColor;

        public bool IsValid => !string.IsNullOrWhiteSpace(_id);
    }
}