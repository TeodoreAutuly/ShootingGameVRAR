using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Finalise la configuration du radar VR : ajoute RectTransform + Image à RadarBar/Indicator.
/// Exécuter via Tools/Setup/Setup Radar UI
/// </summary>
public static class RadarUISetup
{
    [MenuItem("Tools/Setup/Setup Radar UI")]
    private static void SetupRadarUI()
    {
        const string path = "Assets/Prefabs/Player_VR.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) { Debug.LogError($"Impossible de charger {path}"); return; }

        try
        {
            int changes = 0;

            // ── RadarBar ──────────────────────────────────────────────────────
            Transform radarBarT = FindChildByName(root.transform, "RadarBar");
            RectTransform radarBarRT = null;
            if (radarBarT != null)
            {
                GameObject radarBarGO = radarBarT.gameObject;
                radarBarRT = radarBarGO.GetComponent<RectTransform>();
                if (radarBarRT == null)
                {
                    radarBarGO.AddComponent<RectTransform>();
                    radarBarRT = radarBarGO.GetComponent<RectTransform>();
                    if (radarBarRT != null)
                    {
                        radarBarRT.sizeDelta = new Vector2(600, 30);
                        radarBarRT.anchorMin = new Vector2(0.5f, 1f);
                        radarBarRT.anchorMax = new Vector2(0.5f, 1f);
                        radarBarRT.pivot     = new Vector2(0.5f, 1f);
                        radarBarRT.anchoredPosition = new Vector2(0, -10);
                    }
                    changes++;
                }
                else
                {
                    Debug.Log("[RadarUISetup] RadarBar a déjà un RectTransform.");
                }
            }

            // ── Indicator ─────────────────────────────────────────────────────
            // Find the Indicator that is a child of RadarBar
            Transform indicatorT = radarBarRT != null ? FindChildByName(radarBarRT.transform, "Indicator") : null;
            if (indicatorT == null)
                indicatorT = FindChildByName(root.transform, "Indicator");

            RectTransform indicatorRT = null;
            if (indicatorT != null)
            {
                GameObject indicatorGO = indicatorT.gameObject;
                indicatorRT = indicatorGO.GetComponent<RectTransform>();
                if (indicatorRT == null)
                {
                    indicatorGO.AddComponent<RectTransform>();
                    indicatorRT = indicatorGO.GetComponent<RectTransform>();
                    if (indicatorRT != null)
                        indicatorRT.sizeDelta = new Vector2(20, 20);
                }
                else
                {
                    indicatorRT.sizeDelta = new Vector2(20, 20);
                }

                if (indicatorGO.GetComponent<Image>() == null)
                {
                    Image img = indicatorGO.AddComponent<Image>();
                    img.color = new Color(1f, 0.2f, 0.1f, 0.9f);
                }

                indicatorGO.SetActive(false);
                changes++;
            }

            // ── Wire VRRadarHUD references ────────────────────────────────────
            VRRadarHUD radar = root.GetComponentInChildren<VRRadarHUD>(true);
            if (radar != null)
            {
                SerializedObject so = new SerializedObject(radar);
                if (radarBarRT != null)
                {
                    SerializedProperty p = so.FindProperty("radarBar");
                    if (p != null) { p.objectReferenceValue = radarBarRT; changes++; }
                }
                if (indicatorRT != null)
                {
                    SerializedProperty p = so.FindProperty("indicatorTemplate");
                    if (p != null) { p.objectReferenceValue = indicatorRT; changes++; }
                }
                so.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"[RadarUISetup] Terminé — {changes} changements appliqués.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
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
