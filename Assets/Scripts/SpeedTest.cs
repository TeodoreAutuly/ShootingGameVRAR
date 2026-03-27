using UnityEngine;

public class BulletDebug : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        float distanceTraveled = Vector3.Distance(_previousPosition, transform.position);
        Debug.Log($"[BulletDebug] pos={transform.position} | vitesse={_rb.linearVelocity.magnitude:F2} m/s | distance/frame={distanceTraveled:F3}m");
        _previousPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[BulletDebug] OnTriggerEnter avec {other.gameObject.name}");
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"[BulletDebug] OnCollisionEnter avec {other.gameObject.name}");
    }
}