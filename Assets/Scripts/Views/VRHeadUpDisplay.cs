using TMPro;
using UnityEngine;

/// <summary>
/// HUD World Space du joueur VR — visible uniquement par le propriétaire.
///
/// Placer ce composant sur un Canvas enfant de Main Camera.
/// Le Canvas est en mode World Space et se suit grâce au parentage — aucun code
/// de suivi nécessaire puisqu'il est enfant de la caméra.
///
/// Setup Unity (dans le prefab Player_VR, sous Main Camera) :
/// Main Camera
/// └── HUD Canvas            [Canvas : World Space · VRHeadUpDisplay]
///         ├── ScorePanel    [Image optionnelle de fond semi-transparent]
///         │   └── ScoreText [TextMeshProUGUI]
///         └── ...
///
/// Paramètres Canvas recommandés :
///   Render Mode  : World Space
///   Width / Height : 800 / 600  (unités canvas)
///   Scale         : 0.001 (soit 0.8m × 0.6m en monde 3D)
///   Position      : (−0.35, 0.25, 1.0) → haut-gauche devant le joueur
///   Rotation      : (0, 0, 0) relatif à la caméra
///
/// Le score est mis à jour via UpdateScore(int) — appelé par VRPlayerController
/// dès que le NetworkVariable<int> Score change de valeur.
/// </summary>
public class VRHeadUpDisplay : MonoBehaviour
{
    [Header("UI — assigner dans le prefab")]
    [Tooltip("TextMeshPro affichant le score en haut à gauche du HUD.")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("TextMeshPro facultatif pour un message de statut (ex: 'Calibration AR en cours…').")]
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        // Visibilité : ce HUD n'est utile qu'une fois l'avatar spawné,
        // VRPlayerController.OnNetworkSpawn l'activera via ShowHUD().
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Appelée par VRPlayerController.OnNetworkSpawn() uniquement si IsOwner.
    /// </summary>
    public void ShowHUD()
    {
        gameObject.SetActive(true);
        RefreshScore(0);
    }

    /// <summary>Met à jour l'affichage du score (valeur absolue depuis NetworkVariable).</summary>
    public void UpdateScore(int score)
    {
        RefreshScore(score);
    }

    /// <summary>Affiche un message temporaire de statut (ex: calibration AR terminée).</summary>
    public void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void RefreshScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score\n{score:D4}";
    }
}
