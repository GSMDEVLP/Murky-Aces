using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    [RequireComponent(
        typeof(MeshFilter),
        typeof(MeshRenderer),
        typeof(MeshCollider))]
    public sealed class TestRoadView : MonoBehaviour
    {
        [SerializeField] private LocationGenerationPreview _source;

        [SerializeField, Min(0.1f)]
        private float _width = 8f;

        [SerializeField] private float _height = 0.05f;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        private void OnEnable()
        {
            if (Application.isPlaying)
                RebuildRoad();
        }

        [ContextMenu("Rebuild Road")]
        public void RebuildRoad()
        {
            if (_source == null)
            {
                Debug.LogWarning("Road source is not assigned.", this);
                return;
            }

            _source.Generate();

            LocationLayout layout = _source.Layout;

            if (layout == null ||
                layout.RoadPoints.Count < 2 ||
                _width <= 0f)
            {
                ReleaseMesh();
                return;
            }

            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();

            // Отсоединяем коллайдер перед изменением геометрии.
            _meshCollider.sharedMesh = null;

            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = "Test Road",
                    hideFlags = HideFlags.DontSave
                };
            }
            else
            {
                _mesh.Clear();
            }

            var points = layout.RoadPoints;

            var vertices = new Vector3[points.Count * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(points.Count - 1) * 6];

            float distance = 0f;
            float halfWidth = _width * 0.5f;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 previous = points[Mathf.Max(0, i - 1)];
                Vector2 next = points[Mathf.Min(points.Count - 1, i + 1)];

                Vector2 direction = (next - previous).normalized;

                var forward = new Vector3(direction.x, 0f, direction.y);
                Vector3 side = Vector3.Cross(Vector3.up, forward);

                var center = new Vector3(
                    points[i].x,
                    _height,
                    points[i].y);

                int vertexIndex = i * 2;

                vertices[vertexIndex] = transform.InverseTransformPoint(
                    center - side * halfWidth);

                vertices[vertexIndex + 1] = transform.InverseTransformPoint(
                    center + side * halfWidth);

                if (i > 0)
                    distance += Vector2.Distance(points[i - 1], points[i]);

                uv[vertexIndex] = new Vector2(0f, distance / _width);
                uv[vertexIndex + 1] = new Vector2(1f, distance / _width);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                int vertexIndex = i * 2;
                int triangleIndex = i * 6;

                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 2;
                triangles[triangleIndex + 2] = vertexIndex + 1;

                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + 2;
                triangles[triangleIndex + 5] = vertexIndex + 3;
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _meshFilter.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
        }

        private void OnDisable()
        {
            ReleaseMesh();
        }

        private void ReleaseMesh()
        {
            if (_mesh == null)
                return;

            if (_meshFilter != null)
                _meshFilter.sharedMesh = null;

            if (_meshCollider != null)
                _meshCollider.sharedMesh = null;

            if (Application.isPlaying)
                Destroy(_mesh);
            else
                DestroyImmediate(_mesh);

            _mesh = null;
        }
    }
}