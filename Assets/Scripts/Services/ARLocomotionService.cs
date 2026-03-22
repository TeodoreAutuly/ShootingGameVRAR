using UnityEngine;

public class ARLocomotionService : IARLocomotionService
{
    private readonly Transform _xrOrigin;
    private readonly Transform _camera;
    private readonly float _speed;

    public ARLocomotionService(Transform xrOrigin, Transform camera, float speed = 1.5f)
    {
        _xrOrigin = xrOrigin;
        _camera = camera;
        _speed = speed;
    }

    public void MoveUp(float deltaTime)      => Move(Vector3.up, deltaTime);
    public void MoveDown(float deltaTime)    => Move(-Vector3.up, deltaTime);
    public void MoveForward(float deltaTime) => Move(FlatForward, deltaTime);
    public void MoveBackward(float deltaTime)=> Move(-FlatForward, deltaTime);

    private Vector3 FlatForward =>
        Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;

    private void Move(Vector3 direction, float deltaTime)
    {
        _xrOrigin.position += direction * (_speed * deltaTime);
    }
}