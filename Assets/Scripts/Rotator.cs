using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float speed = 60f;

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.World);
    }
}