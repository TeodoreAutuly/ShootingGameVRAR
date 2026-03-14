# AR Rig — Branche `ar-rig`

## Ce qui a été fait

### 1. Création du projet Unity et configuration Android
- Template : **3D Built-In Render Pipeline**
- Plateforme : **Android**
- Scripting Backend : **IL2CPP**
- Architecture : **ARM64**
- API Level minimum : **26 (Android 8.0)**
- Graphics API : **OpenGLES3 uniquement** (Vulkan retiré — incompatible AR Foundation)
- Color Space : **Linear**
- Active Input Handling : **Input System Package (New)**

### 2. Packages installés
| Package | Version |
|---|---|
| AR Foundation | 6.x |
| Google ARCore XR Plugin | 6.x |
| XR Interaction Toolkit | 3.x |
| Unity Netcode for GameObjects | dernière stable |
| Input System | dernière stable |
| Android Logcat | dernière stable |

> AR Foundation et ARCore XR Plugin doivent avoir la même version mineure (ex: 5.1.x / 5.1.x).

### 3. Configuration XR
`Edit → Project Settings → XR Plug-in Management → Android` : ARCore coché.

### 4. Construction de la scène AR
La scène `AR_Rig_Scene.unity` contient :
```
Hierarchy
├── AR Session
└── XR Origin (AR Rig)                       ← prefab AR Starter Assets (XRI)
    ├── AR Plane Manager (component)         ← détection sol (Horizontal)
    │     └── AR Feathered Plane (prefab de AR starter asstes) ← visualisation des surfaces détectées
    ├── AR Raycast Manager(component)        ← raycast sur les surfaces
    └── ARTargetPlacer (Script)              ← placement des cibles par tap écran
```

### 5. Script ARTargetPlacer
Utilise le **nouveau système d'input (EnhancedTouch)** pour détecter les taps écran et placer une cible sur la surface AR détectée par raycast. Premier tap : spawn de la cible. Taps suivants : déplacement de la cible.

### 6. Validation
AR Rig testé et validé sur **Samsung Galaxy A56** :
- Caméra AR avec passthrough ✅
- Détection de surfaces horizontales ✅
- Placement d'un objet 3D ancré dans le monde réel ✅

---

## Intégration dans le projet principal

Le prefab **XR Origin (AR Rig)** est prévu pour être branché dans le système de détection d'équipement existant. Quand un joueur Android se connecte, le système charge la scène AR contenant ce prefab.

Pour utiliser le rig dans une nouvelle scène :
1. Glisser **AR Session** dans la Hierarchy
2. Glisser le prefab **XR Origin (AR Rig)** dans la Hierarchy
3. Assigner les vraies cibles du jeu dans le champ `Target Prefab` du script `ARTargetPlacer`

---

## Fichiers ajoutés par cette branche

```
Assets/
├── Scenes/AR_Rig_Scene.unity       ← scène de test AR
├── Scripts/ARTargetPlacer.cs       ← script de placement des cibles
└── Prefabs/                        ← prefab capsule (cible de test)
```

---

## Notes pour le merge

Lors du merge de cette branche dans `main`, des conflits sont attendus sur les fichiers suivants car ils ont été modifiés des deux côtés :

| Fichier | Raison du conflit potentiel |
|---|---|
| `ProjectSettings/PlayerSettings.asset` | Settings Android configurés pour AR (ARM64, OpenGLES3, API 26) |
| `ProjectSettings/GraphicsSettings.asset` | Suppression de Vulkan |
| `Packages/manifest.json` | Ajout des packages AR Foundation, ARCore, Input System |

**Ces conflits doivent être résolus ensemble en équipe** pour s'assurer que les settings VR et AR coexistent correctement dans la configuration finale du projet.
