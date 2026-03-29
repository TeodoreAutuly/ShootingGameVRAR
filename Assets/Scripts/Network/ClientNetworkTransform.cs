using Unity.Netcode.Components;

/// <summary>
/// NetworkTransform en mode Owner-Authoritaire.
/// L'objet possédé (Owner) envoie ses mises à jour de transform aux autres clients via le serveur.
///
/// Ajouter ce composant à la place de NetworkTransform sur :
/// - Player_VR : Camera Offset/Main Camera, Left Controller, Right Controller
/// - Player_AR : racine (XR Origin AR Rig), enfant Drone
/// </summary>
[UnityEngine.DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() => false;
}
