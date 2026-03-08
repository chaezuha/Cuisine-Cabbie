using UnityEngine;

namespace CarStuff
{
    public class Gearbox : MonoBehaviour
    {
        private int _gearType;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _gearType = 0;
        }

        public void ShiftGearUp()
        {
            switch (_gearType)
            {
                case -1:
                    Debug.Log("Shifting to Neutral");
                    _gearType = 0;
                    break;
                case 0:
                    Debug.Log("Shifting to Drive");
                    _gearType = 1;
                    break;
            }
        }

        public void ShiftGearDown()
        {
            switch (_gearType)
            {
                case 1:
                    Debug.Log("Shifting to Neutral");
                    _gearType = 0;
                    break;
                case 0:
                    Debug.Log("Shifting to Reverse");
                    _gearType = -1;
                    break;
            }
        }

        public void ShiftGear()
        {
            switch (_gearType)
            {
                //Drive to Neutral
                case 1:
                    Debug.Log("Shifting to Neutral");
                    _gearType = 0;
                    break;
                //Neutral to Reverse
                case 0:
                    Debug.Log("Shifting to Reverse");

                    _gearType = -1;
                    break;
                //Reverse to Drive
                case -1:
                    Debug.Log("Shifting to Drive");
                    _gearType = 1;
                    break;
            }
        }

        public int GetGearType()
        {
            return _gearType;
        }
    }
}
