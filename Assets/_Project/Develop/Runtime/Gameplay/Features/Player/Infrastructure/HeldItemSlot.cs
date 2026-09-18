using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Sandbox;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
public sealed class HeldItemSlot : MonoBehaviour, IPickupReceiver
{
    [SerializeField] private Transform _anchor;

    [Header("Drop")]
    [SerializeField, Min(0f)] private float _dropDistance = 0.5f;
    [SerializeField, Min(0f)] private float _dropForce = 2f;

    private PickupInteractable _heldItem;

    public bool IsOccupied => _heldItem != null;

    public bool TryReceive(IInteractable item)
    {
        if (IsOccupied || _anchor == null)
            return false;

        if (item is not PickupInteractable pickupItem)
            return false;

        _heldItem = pickupItem;

        Transform itemTransform = pickupItem.transform;

        itemTransform.SetParent(_anchor, false);
        itemTransform.localPosition = Vector3.zero;
        itemTransform.localRotation = Quaternion.identity;

        return true;
    }

    public bool TryDrop()
    {
        if (!IsOccupied || _anchor == null)
            return false;

        PickupInteractable item = _heldItem;
        _heldItem = null;

        Vector3 dropDirection = _anchor.forward;
        Vector3 dropPosition =
            _anchor.position +
            dropDirection * _dropDistance;

        item.Drop(dropPosition, dropDirection * _dropForce);

        return true;
    }
}
}
