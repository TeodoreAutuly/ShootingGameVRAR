using UnityEditor;
using UnityEngine;

/// <summary>
/// Script éditeur — câble les références inspector pour le système de cibles.
/// Exécuter via Tools/Setup/Wire Target System References
/// </summary>
public static class TargetSystemSetup2
{
    [MenuItem("Tools/Setup/Wire Target System References")]
    private static void WireTargetSystemReferences()
    {
        int changes = 0;

        WireVRPrefab(ref changes);
        WireARPrefab(ref changes);

        Debug.Log($"[TargetSystemSetup] Câblage terminé — {changes} propriétés modifiées.");
        AssetDatabase.SaveAssets();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static void WireVRPrefab(ref int changes)
    {
        const string path = "Assets/Prefabs/Player_VR.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) { Debug.LogError($"Impossible de charger {path}"); return; }

        try
        {
            // VRRadarHUD references
            VRRadarHUD radar = root.GetComponentInChildren<VRRadarHUD>(true);
            if (radar != null)
            {
                SerializedObject so = new SerializedObject(radar);

                // headTransform = Main Camera transform
                Camera cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) SetRef(so, "headTransform", cam.transform, ref changes);

                // radarBar = RadarBar RectTransform
                Transform radarBarGO = FindChildByName(root.transform, "RadarBar");
                if (radarBarGO != null)
                {
                    RectTransform rt = radarBarGO.GetComponent<RectTransform>();
                    if (rt != null) SetRef(so, "radarBar", rt, ref changes);
                }

                // indicatorTemplate = RadarBar/Indicator RectTransform
                Transform indicatorGO = FindChildByName(root.transform, "Indicator");
                if (indicatorGO != null)
                {
                    RectTransform rt = indicatorGO.GetComponent<RectTransform>();
                    if (rt != null) SetRef(so, "indicatorTemplate", rt, ref changes);
                }

                so.ApplyModifiedProperties();
            }

            // VRShootingController — gunTip
            VRShootingController shooter = root.GetComponentInChildren<VRShootingController>(true);
            if (shooter != null)
            {
                Transform gunTip = FindChildByName(root.transform, "GunTip");
                if (gunTip != null)
                {
                    SerializedObject so = new SerializedObject(shooter);
                    SetRef(so, "gunTip", gunTip, ref changes);
                    so.ApplyModifiedProperties();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void WireARPrefab(ref int changes)
    {
        const string path = "Assets/Prefabs/Player_AR.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) { Debug.LogError($"Impossible de charger {path}"); return; }

        try
        {
            ARTargetInteractor interactor = root.GetComponentInChildren<ARTargetInteractor>(true);
            if (interactor != null)
            {
                Camera arCam = root.GetComponentInChildren<Camera>(true);
                if (arCam != null)
                {
                    SerializedObject so = new SerializedObject(interactor);
                    SetRef(so, "arCamera", arCam, ref changes);
                    so.ApplyModifiedProperties();
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static void SetRef(SerializedObject so, string propName, UnityEngine.Object value, ref int changes)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            changes++;
        }
        else
        {
            Debug.LogWarning($"[TargetSystemSetup] Propriété '{propName}' introuvable sur {so.targetObject?.GetType().Name}");
        }
    }

    private static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
