using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeliveryMechanics
{
    public class TelemetrySession : MonoBehaviour
    {
        private static TelemetrySession _instance;
        public static TelemetrySession Instance => _instance;

        private float _sessionStartTime;
        private int _runNumber;
        private readonly List<RunRecord> _runs = new List<RunRecord>();
        private RunRecord _currentRun;
        private bool _runActive;

        private string _csvPath;
        private string _sessionTimestamp;

        public struct RunRecord
        {
            public int Run;
            public float StartTime;
            public float EndTime;
            public float DurationSeconds;
            public int TotalDeliveries;
            public int TotalRounds;
            public string EndReason;
            public List<DeliveryTelemetry.LegRecord> Legs;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnApplicationQuit()
        {
            if (_instance == this && (_runActive || _runs.Count > 0))
            {
                if (_runActive)
                {
                    var telemetry = FindFirstObjectByType<DeliveryTelemetry>();
                    var brain = FindFirstObjectByType<DeliveryBrain>();
                    int deliveries = brain != null ? brain.GetDeliveryCount() : 0;
                    int rounds = telemetry != null ? telemetry.GetRoundNumber() : 0;
                    FinishCurrentRun(deliveries, rounds, "App Quit");
                }

                EndSession();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Driving")
            {
                if (_runs.Count == 0 && !_runActive)
                {
                    _sessionStartTime = Time.time;
                    _sessionTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                    var folder = Path.Combine(Application.persistentDataPath, "Telemetry");
                    Directory.CreateDirectory(folder);
                    _csvPath = Path.Combine(folder, $"session_{_sessionTimestamp}.csv");

                    WriteCsvHeader();
                    Debug.Log($"[Telemetry] === Session Started === CSV: {_csvPath}");
                }

                StartNewRun();
            }
            else if (scene.name == "MainMenu")
            {
                EndSession();
            }
        }

        private void StartNewRun()
        {
            if (_runActive)
            {
                FinishCurrentRun(0, 0, "Restart");
            }

            _runNumber++;
            _currentRun = new RunRecord
            {
                Run = _runNumber,
                StartTime = Time.time,
                Legs = new List<DeliveryTelemetry.LegRecord>()
            };
            _runActive = true;

            Debug.Log($"[Telemetry] --- Run {_runNumber} Started ---");
        }

        public void RecordLeg(DeliveryTelemetry.LegRecord leg)
        {
            if (!_runActive) return;
            _currentRun.Legs.Add(leg);
        }

        public void FinishCurrentRun(int totalDeliveries, int totalRounds, string endReason)
        {
            if (!_runActive) return;

            _currentRun.EndTime = Time.time;
            _currentRun.DurationSeconds = _currentRun.EndTime - _currentRun.StartTime;
            _currentRun.TotalDeliveries = totalDeliveries;
            _currentRun.TotalRounds = totalRounds;
            _currentRun.EndReason = endReason;
            _runs.Add(_currentRun);
            _runActive = false;

            WriteCsvRunLegs(_currentRun);

            Debug.Log($"[Telemetry] --- Run {_currentRun.Run} Ended ({endReason}) --- " +
                      $"Duration: {_currentRun.DurationSeconds:F1}s | " +
                      $"Deliveries: {totalDeliveries} | " +
                      $"Rounds: {totalRounds} | " +
                      $"Legs: {_currentRun.Legs.Count}");
        }

        private void EndSession()
        {
            if (_runs.Count == 0 && !_runActive)
            {
                return;
            }

            float sessionDuration = Time.time - _sessionStartTime;
            int totalDeliveries = 0;
            int totalLegs = 0;

            foreach (var run in _runs)
            {
                totalDeliveries += run.TotalDeliveries;
                totalLegs += run.Legs.Count;
            }

            Debug.Log($"[Telemetry] ============================");
            Debug.Log($"[Telemetry] SESSION SUMMARY");
            Debug.Log($"[Telemetry] Total session time: {sessionDuration:F1}s");
            Debug.Log($"[Telemetry] Runs: {_runs.Count}");
            Debug.Log($"[Telemetry] Total deliveries across all runs: {totalDeliveries}");
            Debug.Log($"[Telemetry] Total legs across all runs: {totalLegs}");
            Debug.Log($"[Telemetry] CSV saved to: {_csvPath}");
            Debug.Log($"[Telemetry] ============================");

            _runs.Clear();
            _runNumber = 0;
            _runActive = false;
        }

        private void WriteCsvHeader()
        {
            var header = "Run,Round,Leg,From,To,Seconds,FuelUsedPercent,FuelRemainingPercent,Collisions,CollisionFuelLostPercent,LegDistanceFt,DepotDistanceFt,AvgSpeedMph,EndReason";
            File.WriteAllText(_csvPath, header + Environment.NewLine);
        }

        private void WriteCsvRunLegs(RunRecord run)
        {
            if (string.IsNullOrEmpty(_csvPath)) return;

            var sb = new StringBuilder();
            foreach (var leg in run.Legs)
            {
                sb.AppendLine($"{run.Run},{leg.Round},{leg.Leg}," +
                              $"\"{leg.From}\",\"{leg.To}\"," +
                              $"{leg.Seconds:F2}," +
                              $"{leg.FuelUsedPercent:F2}," +
                              $"{leg.FuelRemainingPercent:F2}," +
                              $"{leg.Collisions}," +
                              $"{leg.CollisionFuelLostPercent:F2}," +
                              $"{leg.LegDistanceFeet:F1}," +
                              $"{leg.DepotDistanceFeet:F1}," +
                              $"{leg.AvgSpeedMph:F1}," +
                              $"\"{run.EndReason}\"");
            }

            File.AppendAllText(_csvPath, sb.ToString());
        }

        public int GetRunNumber()
        {
            return _runNumber;
        }

        public List<RunRecord> GetRuns()
        {
            return new List<RunRecord>(_runs);
        }
    }
}
