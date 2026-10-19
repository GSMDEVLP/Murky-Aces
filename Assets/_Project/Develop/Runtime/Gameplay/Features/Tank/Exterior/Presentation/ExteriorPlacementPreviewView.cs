using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Presentation
{
    public sealed class ExteriorPlacementPreviewView : MonoBehaviour
    {
        private GameObject _instance;
        private GameObject _prefab;
        private Transform _anchor;

        public bool TryShow(GameObject prefab, Transform anchor)
        {
            if (!isActiveAndEnabled ||
                prefab == null ||
                anchor == null ||
                !anchor.gameObject.activeInHierarchy)
            {
                Hide();
                return false;
            }

            if (_instance != null &&
                _prefab == prefab &&
                _anchor == anchor)
            {
                return true;
            }

            Hide();

            _instance = Instantiate(prefab, anchor, false);
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.identity;
            _instance.transform.localScale = Vector3.one;

            _prefab = prefab;
            _anchor = anchor;

            return true;
        }

        public void Hide()
        {
            if (_instance != null)
            {
                _instance.SetActive(false);
                Destroy(_instance);
            }

            _instance = null;
            _prefab = null;
            _anchor = null;
        }

        private void OnDisable()
        {
            Hide();
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}