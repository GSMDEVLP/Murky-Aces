using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain
{
    public sealed class TankRadar
    {
        private IReadOnlyList<RadarContact> _contacts = Array.Empty<RadarContact>();

        public bool IsEnabled { get; private set; }

        public IReadOnlyList<RadarContact> Contacts => _contacts;

        public void SetEnabled(bool enabled)
        {
            if (IsEnabled == enabled)
                return;

            IsEnabled = enabled;

            if (!enabled)
                ClearContacts();
        }

        public void ReplaceContacts(
            IEnumerable<RadarContact> contacts)
        {
            if (contacts == null)
                throw new ArgumentNullException(nameof(contacts));

            if (!IsEnabled)
                return;

            _contacts = new List<RadarContact>(contacts).AsReadOnly();
        }

        public void ClearContacts()
        {
            _contacts = Array.Empty<RadarContact>();
        }
    }
}