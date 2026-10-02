using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Presentation
{
    public sealed class GameplayCameraRig : MonoBehaviour
    {
        public Transform PlayerCameraPivot
        {
            get;
            private set;
        }

        public bool TryBindPlayerCameraPivot(
            Transform cameraPivot)
        {
            if (cameraPivot == null)
                return false;

            transform.SetParent(cameraPivot, false);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            PlayerCameraPivot = cameraPivot;

            return true;
        }

        public bool TryUnbindPlayerCameraPivot(Transform cameraPivot)
        {
            if (PlayerCameraPivot == null ||
                PlayerCameraPivot != cameraPivot)
            {
                return false;
            }

            transform.SetParent(null, true);
            PlayerCameraPivot = null;
            return true;
        }
    }
}