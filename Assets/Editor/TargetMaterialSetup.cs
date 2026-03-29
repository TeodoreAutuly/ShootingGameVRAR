using UnityEditor;
using UnityEngine;

/// <summary>
/// Script éditeur temporaire — assigne les matériaux Active/Inactive à tous les TargetController de la scène.
/// Exécuter via Tools/Setup/Assign Target Materials.
/// </summary>
public static class TargetMaterialSetup
{
    [MenuItem("Tools/Setup/Assign Target Materials")]
    private static void AssignTargetMaterials()
    {
        Material activeMat   = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Target_Active.mat");
        Material inactiveMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Target_Inactive.mat");

        if (activeMat == null || inactiveMat == null)
        {
            Debug.LogError("[TargetMaterialSetup] Impossible de charger les matériaux dans Assets/Materials/");
            return;
        }

        TargetController[] targets = Object.FindObjectsByType<TargetController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;

        foreach (TargetController tc in targets)
        {
            SerializedObject so = new SerializedObject(tc);
            SerializedProperty activeProp   = so.FindProperty("activeMaterial");
            SerializedProperty inactiveProp = so.FindProperty("inactiveMaterial");

            if (activeProp != null)   activeProp.objectReferenceValue   = activeMat;
            if (inactiveProp != null) inactiveProp.objectReferenceValue = inactiveMat;

            so.ApplyModifiedProperties();

            // Renderer material initial = inactive
            MeshRenderer mr = tc.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = inactiveMat;

            EditorUtility.SetDirty(tc.gameObject);
            count++;
        }

        Debug.Log($"[TargetMaterialSetup] Matériaux assignés à {count} cibles.");
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }
}
