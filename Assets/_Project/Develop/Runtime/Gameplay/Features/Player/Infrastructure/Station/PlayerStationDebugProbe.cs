using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station
{
    public sealed class PlayerStationDebugProbe : MonoBehaviour
    {
        [SerializeField] private Transform _seatAnchor;
        [SerializeField] private Transform _exitAnchor;

        private IStationOccupant _occupant;

        [Inject]
        public void Construct(IStationOccupant occupant)
        {
            _occupant = occupant;
        }

        [ContextMenu("Station Debug/Enter")]
        private void EnterStation()
        {
            if (!ValidatePlayMode())
                return;

            bool result =
                _occupant.TryEnterStation(_seatAnchor);

            Debug.Log(
                $"Station enter: {result}. " +
                $"IsInStation: {_occupant.IsInStation}",
                this);
        }

        [ContextMenu("Station Debug/Exit")]
        private void ExitStation()
        {
            if (!ValidatePlayMode())
                return;

            bool result =
                _occupant.TryExitStation(_exitAnchor);

            Debug.Log(
                $"Station exit: {result}. " +
                $"IsInStation: {_occupant.IsInStation}",
                this);
        }

        private bool ValidatePlayMode()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                Debug.LogWarning(
                    "Station debug is available only in Play Mode.",
                    this);

                return false;
            }

            if (_occupant == null)
            {
                Debug.LogError(
                    "IStationOccupant was not injected.",
                    this);

                return false;
            }

            return true;
        }
    }
}