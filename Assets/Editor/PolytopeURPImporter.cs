#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Importe le package URP Polytope Studio compatible avec URP 17.x.
/// Menu : Tools/Setup/Import Polytope URP Package
/// Ce script peut être supprimé après exécution.
/// </summary>
public static class PolytopeURPImporter
{
    private const string PACKAGE_PATH =
        "Assets/Polytope Studio/Lowpoly_Environments/URP/PT_Nature_Free_URP_17.unitypackage";

    [MenuItem("Tools/Setup/Import Polytope URP Package")]
    public static void ImportURPPackage()
    {
        if (!System.IO.File.Exists(System.IO.Path.Combine(
            Application.dataPath.Replace("/Assets", ""), PACKAGE_PATH)))
        {
            Debug.LogError($"[Polytope URP] Package introuvable : {PACKAGE_PATH}");
            return;
        }

        // interactive = false → importe tout sans dialogue de sélection
        AssetDatabase.ImportPackage(PACKAGE_PATH, false);
        Debug.Log("[Polytope URP] Import de PT_Nature_Free_URP_17 lancé. " +
                  "Attendez la fin de la compilation, puis supprimez Assets/Editor/PolytopeURPImporter.cs");
    }
}
#endif
