using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class LocationGenerationPreview : MonoBehaviour
    {
        [SerializeField] private int _seed = 12345;
        [SerializeField] private Vector2 _center = Vector2.zero;
        [SerializeField] private Vector2 _size = new Vector2(200f, 200f);

        [SerializeField, Range(2, 16)]
        private int _pointCount = 6;

        [SerializeField] private float _previewHeight = 0.5f;

        private LocationLayout _layout;

        public LocationLayout Layout => _layout;

        private void OnEnable()
        {
            Generate();
        }

        private void OnValidate()
        {
            Generate();
        }

        [ContextMenu("Generate Preview")]
        public void Generate()
        {
            if (_size.x <= 0f || _size.y <= 0f || _pointCount < 2)
            {
                _layout = null;
                return;
            }

            var generator = new TestLocationGenerator();

            _layout = generator.Generate(
                _seed,
                _center,
                _size,
                _pointCount);
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled || _layout == null)
                return;

            Color previousColor = Gizmos.color;

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(
                ToWorld(_layout.Center),
                new Vector3(_layout.Size.x, 0f, _layout.Size.y));

            var points = _layout.RoadPoints;

            Gizmos.color = Color.yellow;

            for (int i = 1; i < points.Count; i++)
            {
                Gizmos.DrawLine(
                    ToWorld(points[i - 1]),
                    ToWorld(points[i]));
            }

            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.color = i == 0
                    ? Color.green
                    : i == points.Count - 1
                        ? Color.red
                        : Color.cyan;

                Gizmos.DrawSphere(ToWorld(points[i]), 2f);
            }

            Gizmos.color = previousColor;
        }

        private Vector3 ToWorld(Vector2 point)
        {
            return new Vector3(point.x, _previewHeight, point.y);
        }
    }
}