using System.Collections.Generic;
using CarStuff;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DeliveryMechanics
{
    public class DeliveryBrain : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private int maxPackagesPerTrip = 3;
        public readonly HashSet<DropoffMechanics> HeldPackages = new HashSet<DropoffMechanics>();
        private PickupMechanics _pickupMechanics;
        private PlayerAudioController _playerAudioController;
        private PlayerController _playerController;
        private DropoffMechanics[] _allDropOffs;
        private int _deliveryCount;
        private string _currentMessage;

        public HashSet<DropoffMechanics> GetHeldPackages()
        {
            return HeldPackages;
        }

        public int GetDeliveryCount()
        {
            return _deliveryCount;
        }

        public bool HasPackages()
        {
            return HeldPackages.Count > 0;
        }

        public string GetCurrentMessage()
        {
            return _currentMessage;
        }

        void Start()
        {
            _allDropOffs = FindObjectsByType<DropoffMechanics>(FindObjectsSortMode.None);
            _pickupMechanics = FindObjectOfType<PickupMechanics>();
            _playerAudioController = FindFirstObjectByType<PlayerAudioController>();
            _playerController = GetComponent<PlayerController>();
            if (_playerController == null)
            {
                _playerController = FindFirstObjectByType<PlayerController>();
            }
            SetDeliveryCount(0);
        }

        public void TryPickup()
        {
            if (HasPackages())
            {
                SetCurrentMessage("FINISH YOUR CURRENT DELIVERIES FIRST");
                _playerAudioController?.PlayDeliveryError();
                return;
            }

            if (_allDropOffs.Length == 0)
            {
                return;
            }

            var pool = new List<DropoffMechanics>(_allDropOffs);
            var count = Mathf.Min(maxPackagesPerTrip, pool.Count);

            for (var i = 0; i < count; i++)
            {
                var j = Random.Range(i, pool.Count);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            HeldPackages.Clear();
            for (var i = 0; i < count; i++)
            {
                pool[i].SetWaypointActive(true);
                HeldPackages.Add(pool[i]);
            }

            _pickupMechanics.SetWaypointActive(true);
            _playerController?.RefillFuelToMax();
            _playerAudioController?.PlayPickupSuccess();
            ClearMessage();
        }

        public void TryDropoff(DropoffMechanics dropoff)
        {
            if (HeldPackages.Remove(dropoff))
            {
                SetDeliveryCount(_deliveryCount + 1);
                dropoff.SetWaypointActive(false);
                _playerController?.RefuelFromDropoff();
                _playerAudioController?.PlayDropoffSuccess();
                ClearMessage();
            }
            else
            {
                SetCurrentMessage($"YOU DO NOT HAVE A PACKAGE FOR '{dropoff.GetDropOffId()}'");
                _playerAudioController?.PlayDeliveryError();
            }
        }

        public void ClearMessage()
        {
            SetCurrentMessage(null);
        }

        private void SetDeliveryCount(int deliveryCount)
        {
            _deliveryCount = deliveryCount;
        }

        private void SetCurrentMessage(string msg)
        {
            _currentMessage = msg;
        }

        private void Update()
        {
            if (HeldPackages.Count != 0)
            {
                _pickupMechanics.SetWaypointActive(false);
            }
            else
            {
                _pickupMechanics.SetWaypointActive(true);
            }
        }
    }
}
