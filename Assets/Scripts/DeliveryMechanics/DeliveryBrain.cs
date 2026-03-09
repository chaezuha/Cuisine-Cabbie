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
        
        [Header("Layer Settings")]
        [SerializeField] private float maxRadius = 1200f;
        [SerializeField] private int totalLayers = 6;
        private int _currentLayer = 0;

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
            
            float layerSize = maxRadius / totalLayers;
            float minDist = 0.0f;
            float maxDist = (_currentLayer + 1) * layerSize;


            if (_allDropOffs.Length == 0)
            {
                return;
            }

            Vector3 depotPos = _pickupMechanics.GetPositon();
            var pool = new List<DropoffMechanics>();
            foreach (var dropoff in _allDropOffs)
            {
                float dist = Vector3.Distance(depotPos, dropoff.GetPositon());
                if (dist >= minDist && dist < maxDist)
                {
                    pool.Add(dropoff);
                }
            }
            
            while (pool.Count == 0 && _currentLayer < totalLayers - 1)
            {
                _currentLayer++;
                minDist = _currentLayer * layerSize;
                maxDist = (_currentLayer + 1) * layerSize;
                foreach (var dropoff in _allDropOffs)
                {
                    float dist = Vector3.Distance(depotPos, dropoff.GetPositon());
                    if (dist >= minDist && dist < maxDist)
                    {
                        pool.Add(dropoff);
                    }
                }
            }

            if (pool.Count == 0) return;
            
            var count = Mathf.Min(maxPackagesPerTrip, pool.Count);

            for (var i = 0; i < count; i++)
            {
                var j = Random.Range(i, pool.Count);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            
            foreach (var dropoff in _allDropOffs)
            {
                dropoff.SetWaypointActive(false);
            }

            HeldPackages.Clear();
            for (var i = 0; i < count; i++)
            {
                pool[i].SetWaypointActive(true);
                HeldPackages.Add(pool[i]);
            }

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
                if (_pickupMechanics != null && _playerController != null)
                {
                    var deliveryDistanceMeters = Vector3.Distance(_pickupMechanics.GetPositon(), dropoff.GetPositon());
                    _playerController.RefuelFromDropoff(deliveryDistanceMeters);
                }
                _playerAudioController?.PlayDropoffSuccess();
                ClearMessage();
                if (HeldPackages.Count == 0 && _currentLayer < totalLayers - 1)
                {
                    _currentLayer++;
                }
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
