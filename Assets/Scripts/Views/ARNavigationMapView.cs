using UnityEngine;

public class ARNavigationMapView : MonoBehaviour
{
    [SerializeField] private Transform board;
    [SerializeField] private Transform arrow;

    // Demi-taille du board en unités locales pour clamper l'arrow
    [SerializeField] private float boardHalfSize = 0.5f;

    public void UpdateArrow(Vector2 normalizedPosition, float rotationY)
    {
        if (arrow == null || board == null)
            return;

        // Clamp pour garder l'arrow dans les limites du board
        float clampedX = Mathf.Clamp(normalizedPosition.x, -boardHalfSize, boardHalfSize);
        float clampedZ = Mathf.Clamp(normalizedPosition.y, -boardHalfSize, boardHalfSize);

        // Déplace l'arrow en coordonnées locales du board
        arrow.localPosition = new Vector3(clampedX, arrow.localPosition.y, clampedZ);

        // Oriente l'arrow selon le regard du player (axe Y uniquement)
        arrow.localRotation = Quaternion.Euler(arrow.localEulerAngles.x, rotationY, arrow.localEulerAngles.z);
        Debug.Log("AR Arrow Rotation" + arrow.localRotation);
    }
}