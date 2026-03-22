public class ARControlController
{
    private readonly IARLocomotionService _locomotion;

    private bool _moveUp;
    private bool _moveDown;
    private bool _moveForward;
    private bool _moveBackward;

    public ARControlController(IARLocomotionService locomotion)
    {
        _locomotion = locomotion;
    }

    public void Subscribe(ARControlView view)
    {
        view.OnUpPressed       += OnUpPressed;
        view.OnUpReleased      += OnUpReleased;
        view.OnDownPressed     += OnDownPressed;
        view.OnDownReleased    += OnDownReleased;
        view.OnForwardPressed  += OnForwardPressed;
        view.OnForwardReleased += OnForwardReleased;
        view.OnBackwardPressed += OnBackwardPressed;
        view.OnBackwardReleased+= OnBackwardReleased;
    }

    public void Unsubscribe(ARControlView view)
    {
        view.OnUpPressed       -= OnUpPressed;
        view.OnUpReleased      -= OnUpReleased;
        view.OnDownPressed     -= OnDownPressed;
        view.OnDownReleased    -= OnDownReleased;
        view.OnForwardPressed  -= OnForwardPressed;
        view.OnForwardReleased -= OnForwardReleased;
        view.OnBackwardPressed -= OnBackwardPressed;
        view.OnBackwardReleased-= OnBackwardReleased;
    }

    public void Tick(float deltaTime)
    {
        if (_moveUp)       _locomotion.MoveUp(deltaTime);
        if (_moveDown)     _locomotion.MoveDown(deltaTime);
        if (_moveForward)  _locomotion.MoveForward(deltaTime);
        if (_moveBackward) _locomotion.MoveBackward(deltaTime);
    }

    private void OnUpPressed()       => _moveUp = true;
    private void OnUpReleased()      => _moveUp = false;
    private void OnDownPressed()     => _moveDown = true;
    private void OnDownReleased()    => _moveDown = false;
    private void OnForwardPressed()  => _moveForward = true;
    private void OnForwardReleased() => _moveForward = false;
    private void OnBackwardPressed() => _moveBackward = true;
    private void OnBackwardReleased()=> _moveBackward = false;
}