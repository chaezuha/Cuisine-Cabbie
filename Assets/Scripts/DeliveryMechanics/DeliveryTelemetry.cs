using System.Collections.Generic;
using CarStuff;
using UnityEngine;

namespace DeliveryMechanics
{
    public class DeliveryTelemetry : MonoBehaviour
    {
        private PlayerController _playerController;
        private DeliveryBrain _deliveryBrain;
        private PickupMechanics _pickupMechanics;

        private float _legStartTime;
        private float _legStartFuel;
        private string _legStartLabel;
        private Vector3 _legStartPos;
        private bool _tracking;
        private int _legCollisions;
        private float _legCollisionFuelLost;

        private int _roundNumber;
        private int _legIndex;
        private readonly List<LegRecord> _currentRoundLegs = new List<LegRecord>();

        private const float MetersToFeet = 3.28084f;
        private const float FeetPerSecondToMph = 0.681818f;

        public struct LegRecord
        {
            public int Round;
            public int Leg;
            public string From;
            public string To;
            public float Seconds;
            public float FuelUsedPercent;
            public float FuelRemainingPercent;
            public int Collisions;
            public float CollisionFuelLostPercent;
            public float LegDistanceFeet;
            public float DepotDistanceFeet;
            public float AvgSpeedMph;
        }

        private void Start()
        {
            _playerController = FindFirstObjectByType<PlayerController>();
            _deliveryBrain = FindFirstObjectByType<DeliveryBrain>();
            _pickupMechanics = FindFirstObjectByType<PickupMechanics>();
        }

        private void OnDestroy()
        {
            if (TelemetrySession.Instance != null && _deliveryBrain != null)
            {
                bool outOfFuel = _playerController != null && _playerController.GetFuel() <= 0f;
                string reason = outOfFuel ? "Ran Out of Fuel" : "Restart";

                TelemetrySession.Instance.FinishCurrentRun(
                    _deliveryBrain.GetDeliveryCount(),
                    _roundNumber,
                    reason
                );
            }
        }

        public void OnPickup()
        {
            Vector3 depotPos = _pickupMechanics != null ? _pickupMechanics.GetPositon() : Vector3.zero;

            if (_tracking)
            {
                FinishLeg("Depot", depotPos);
                PrintRoundSummary();
            }

            _roundNumber++;
            _legIndex = 0;
            _currentRoundLegs.Clear();
            BeginLeg("Depot", depotPos);
        }

        public void OnCollision(float fuelLost)
        {
            if (!_tracking) return;
            _legCollisions++;
            _legCollisionFuelLost += fuelLost;
        }

        public void OnDropoff(string dropoffId, Vector3 dropoffPos)
        {
            if (!_tracking) return;

            FinishLeg(dropoffId, dropoffPos);
            BeginLeg(dropoffId, dropoffPos);
        }

        private void BeginLeg(string label, Vector3 position)
        {
            _legStartTime = Time.time;
            _legStartFuel = _playerController != null ? _playerController.GetFuel() : 0f;
            _legStartLabel = label;
            _legStartPos = position;
            _legCollisions = 0;
            _legCollisionFuelLost = 0f;
            _tracking = true;
        }

        private void FinishLeg(string toLabel, Vector3 toPos)
        {
            _legIndex++;
            float maxFuel = _playerController != null ? _playerController.GetMaxFuel() : 1f;
            float currentFuel = _playerController != null ? _playerController.GetFuel() : 0f;
            float elapsed = Time.time - _legStartTime;
            float fuelUsed = _legStartFuel - currentFuel;

            float legDistanceFeet = Vector3.Distance(_legStartPos, toPos) * MetersToFeet;

            Vector3 depotPos = _pickupMechanics != null ? _pickupMechanics.GetPositon() : Vector3.zero;
            float depotDistanceFeet = Vector3.Distance(depotPos, toPos) * MetersToFeet;

            float avgSpeedMph = elapsed > 0f ? (legDistanceFeet / elapsed) * FeetPerSecondToMph : 0f;

            var record = new LegRecord
            {
                Round = _roundNumber,
                Leg = _legIndex,
                From = _legStartLabel,
                To = toLabel,
                Seconds = elapsed,
                FuelUsedPercent = (fuelUsed / maxFuel) * 100f,
                FuelRemainingPercent = (currentFuel / maxFuel) * 100f,
                Collisions = _legCollisions,
                CollisionFuelLostPercent = (_legCollisionFuelLost / maxFuel) * 100f,
                LegDistanceFeet = legDistanceFeet,
                DepotDistanceFeet = depotDistanceFeet,
                AvgSpeedMph = avgSpeedMph
            };

            _currentRoundLegs.Add(record);
            TelemetrySession.Instance?.RecordLeg(record);

            Debug.Log($"[Telemetry] Run {TelemetrySession.Instance?.GetRunNumber()} | " +
                      $"Round {record.Round} | Leg {record.Leg}: " +
                      $"{record.From} -> {record.To} | " +
                      $"{record.Seconds:F1}s | " +
                      $"Fuel used: {record.FuelUsedPercent:F1}% | " +
                      $"Fuel remaining: {record.FuelRemainingPercent:F1}% | " +
                      $"Collisions: {record.Collisions} ({record.CollisionFuelLostPercent:F1}% fuel) | " +
                      $"Leg dist: {record.LegDistanceFeet:F0}ft | " +
                      $"Depot dist: {record.DepotDistanceFeet:F0}ft | " +
                      $"Avg: {record.AvgSpeedMph:F1}mph");
        }

        private void PrintRoundSummary()
        {
            if (_currentRoundLegs.Count == 0) return;

            float totalSeconds = 0f;
            float totalFuelUsed = 0f;

            foreach (var leg in _currentRoundLegs)
            {
                totalSeconds += leg.Seconds;
                totalFuelUsed += leg.FuelUsedPercent;
            }

            Debug.Log($"[Telemetry] === Round {_roundNumber} Summary === " +
                      $"Legs: {_currentRoundLegs.Count} | " +
                      $"Total time: {totalSeconds:F1}s | " +
                      $"Total fuel used: {totalFuelUsed:F1}%");
        }

        public List<LegRecord> GetCurrentRoundLegs()
        {
            return new List<LegRecord>(_currentRoundLegs);
        }

        public int GetRoundNumber()
        {
            return _roundNumber;
        }
    }
}
