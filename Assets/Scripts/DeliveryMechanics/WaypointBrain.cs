using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeliveryMechanics
{
    public class WaypointBrain : MonoBehaviour
    {
        private Vector3 _playerPos;

        public float CalculateDistance(Vector3 dropOffPos)
        {
            return Vector3.Distance(_playerPos, dropOffPos);
        }

        public void SetPlayerPos(Vector3 playerPos)
        {
            _playerPos = playerPos;
        }
    }
}