using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DeviceDetectionService : MonoBehaviour
{
    public static DeviceDetectionService Instance { get; private set; }

    // On définit clairement les rôles de ton jeu
    public enum PlayerRole
    {
        Unknown,
        VR_Shooter,
        AR_Drone
    }

    [Header("Debug")]
    [Tooltip("Force un rôle dans l'Éditeur Unity pour faciliter les tests")]
    [SerializeField] private PlayerRole forceRoleInEditor = PlayerRole.VR_Shooter;

    public PlayerRole CurrentRole { get; private set; } = PlayerRole.Unknown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// À appeler depuis ton AppBootstrap.cs
    /// </summary>
    public void Initialize()
    {
        DetectDeviceRole();
    }

    private void DetectDeviceRole()
    {
        // 1. Cas de l'Éditeur Unity (pour tester sur PC sans build)
        if (Application.isEditor)
        {
            CurrentRole = forceRoleInEditor;
            Debug.Log($"[DeviceDetection] Lancement dans l'Éditeur. Rôle forcé : {CurrentRole}");
            return;
        }

        // 2. Vérification de la présence d'un affichage VR (XRDisplaySubsystem)
        bool isVRHeadset = false;
        var displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displaySubsystems);

        foreach (var subsystem in displaySubsystems)
        {
            if (subsystem.running)
            {
                isVRHeadset = true;
                break;
            }
        }

        // Sécurité supplémentaire : On vérifie aussi le nom du modèle de l'appareil
        string deviceModel = SystemInfo.deviceModel.ToLower();
        if (deviceModel.Contains("oculus") || deviceModel.Contains("quest"))
        {
            isVRHeadset = true;
        }

        // 3. Assignation du rôle en fonction de la détection
        if (isVRHeadset)
        {
            CurrentRole = PlayerRole.VR_Shooter;
            Debug.Log("[DeviceDetection] Casque VR détecté. Assignation du rôle : VR_Shooter.");
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            CurrentRole = PlayerRole.AR_Drone;
            Debug.Log("[DeviceDetection] Smartphone détecté. Assignation du rôle : AR_Drone.");
        }
        else
        {
            // Fallback par défaut si on build sur PC/Mac en standalone
            CurrentRole = PlayerRole.VR_Shooter;
            Debug.LogWarning("[DeviceDetection] Appareil non reconnu (probablement PC Standalone). Rôle par défaut : VR_Shooter.");
        }
    }
}