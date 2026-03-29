/// <summary>
/// MODEL — Données de la flèche de direction.
/// Le calcul des positions dots est maintenant dans le Bootstrapper (WorldToCanvas).
/// </summary>
public class ARNavigationMapModel
{
    public float RotationY { get; private set; }

    private readonly float _mapScale;

    public ARNavigationMapModel(float mapScale = 10f)
    {
        _mapScale = mapScale;
    }

    public void UpdateRotation(float rotationY)
    {
        RotationY = rotationY;
    }
}