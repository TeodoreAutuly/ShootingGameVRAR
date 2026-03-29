using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Recrée les nœuds RadarBar / Indicator en tant que vrais UI GameObjects (RectTransform)
/// dans la HUD Canvas du prefab Player_VR.
/// Exécuter via Tools/Setup/Fix Radar UI Nodes
/// </summary>
public static class RadarUIFix
{
    [MenuItem("Tools/Setup/Fix Radar UI Nodes")]
    private static void FixRadarUINodes()
    {
        const string path = "Assets/Prefabs/Player_VR.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) { Debug.LogError($"Impossible de charger {path}"); return; }

        try
        {
            // ── Trouver HUD Canvas ─────────────────────────────────────────
            Transform hudCanvas = FindChildByName(root.transform, "HUD Canvas");
            if (hudCanvas == null) { Debug.LogError("[RadarUIFix] HUD Canvas introuvable."); return; }

            // ── Supprimer les anciens nœuds (Transform seul) ───────────────
            DestroyNamed(hudCanvas, "RadarBar");
            DestroyNamed(hudCanvas, "Indicator");

            // ── Créer RadarBar (UI GO avec RectTransform) ────────────────
            GameObject radarBarGO = new GameObject("RadarBar");
            radarBarGO.transform.SetParent(hudCanvas, false);
            RectTransform radarBarRT = radarBarGO.AddComponent<RectTransform>();
            radarBarRT.sizeDelta        = new Vector2(600, 30);
            radarBarRT.anchorMin        = new Vector2(0.5f, 1f);
            radarBarRT.anchorMax        = new Vector2(0.5f, 1f);
            radarBarRT.pivot            = new Vector2(0.5f, 0.5f);
            radarBarRT.anchoredPosition = new Vector2(0, -30);

            // ── Créer Indicator (enfant de RadarBar) ──────────────────────
            GameObject indicatorGO = new GameObject("Indicator");
            indicatorGO.transform.SetParent(radarBarGO.transform, false);
            RectTransform indicatorRT = indicatorGO.AddComponent<RectTransform>();
            indicatorRT.sizeDelta        = new Vector2(20, 20);
            indicatorRT.anchoredPosition = Vector2.zero;
            Image img    = indicatorGO.AddComponent<Image>();
            img.color    = new Color(1f, 0.2f, 0.1f, 0.9f);
            indicatorGO.SetActive(false);

            // ── Câbler VRRadarHUD ──────────────────────────────────────────
            VRRadarHUD radar = root.GetComponentInChildren<VRRadarHUD>(true);
            if (radar != null)
            {
                SerializedObject so   = new SerializedObject(radar);
                SerializedProperty p1 = so.FindProperty("radarBar");
                SerializedProperty p2 = so.FindProperty("indicatorTemplate");
                if (p1 != null) p1.objectReferenceValue = radarBarRT;
                if (p2 != null) p2.objectReferenceValue = indicatorRT;
                so.ApplyModifiedProperties();
                Debug.Log("[RadarUIFix] VRRadarHUD câblé.");
            }
            else
            {
                Debug.LogWarning("[RadarUIFix] VRRadarHUD non trouvé dans le prefab.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log("[RadarUIFix] Terminé — RadarBar + Indicator créés.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void DestroyNamed(Transform parent, string name)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                Object.DestroyImmediate(child.gameObject);
                return;
            }
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
