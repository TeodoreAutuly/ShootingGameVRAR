using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gère l'activation et la destruction (Hit) unitaire d'une cible synchronisée.
/// </summary>
public class TargetController : NetworkBehaviour
{
    [Header("État de la Cible")]
    // Synchronise l'état entre le serveur (VR) et le client (AR) automatiquement.
    [SerializeField] private NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false);

    [Header("Visuel & Événements (exécutés localement pour FX)")]
    public UnityEvent OnActivatedLocally;
    public UnityEvent OnDestroyedLocally;

    private Collider targetCollider;
    private MeshRenderer visualRenderer;

    private void Awake()
    {
        targetCollider = GetComponent<Collider>();
        visualRenderer = GetComponent<MeshRenderer>();
        
        // S'abonne au changement d'état synchro
        isActivated.OnValueChanged += HandleActivationStateChanged;
    }

    public override void OnNetworkSpawn()
    {
        // Applique l'état initial
        UpdateTargetVisuals(isActivated.Value);
    }

    private void UpdateTargetVisuals(bool activeState)
    {
        // Exemple : activer la cible au joueur VR pour qu'elle soit tirable
        if (targetCollider != null) targetCollider.enabled = activeState;
        
        // Exemple : changer sa couleur / la rendre visible
        if (visualRenderer != null)
        {
            Color colorState = activeState ? Color.red : Color.gray;
            visualRenderer.material.color = colorState;
        }

        if (activeState) OnActivatedLocally?.Invoke();
    }

    private void HandleActivationStateChanged(bool previousState, bool newState)
    {
        UpdateTargetVisuals(newState);
    }

    // --- LOGIQUE CLIENT AR --- //

    /// <summary>
    /// Fonction appelée depuis le bouton UI en AR lorsque le drone survole la cible.
    /// </summary>
    public void DemandActivationByAR()
    {
        if (IsClient && !IsServer)
        {
            ActivateTargetServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateTargetServerRpc(ServerRpcParams rpcParams = default)
    {
        isActivated.Value = true; // Déclenche le OnValueChanged sur tous les clients
        Debug.Log($"[Target] Activée par le drone AR (Client: {rpcParams.Receive.SenderClientId})");
    }


    // --- LOGIQUE JOUEUR VR --- //

    /// <summary>
    /// Fonction appelée par le Raycast ou l'Event XRI (XR Interaction Toolkit) du pistolet VR
    /// </summary>
    public void HandleHitFromVRWeapon(int damage = 1)
    {
        // Vérifie si on est bien la VR/Serveur qui tire, et que la cible est active
        if (IsServer && isActivated.Value)
        {
            RegisterDamageServerRpc(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        // Exemple simplifié sans points de vie
        ExecuteDestructionClientRpc();
    }

    [ClientRpc]
    private void ExecuteDestructionClientRpc()
    {
        // Jouer un effet particulaire ou son
        OnDestroyedLocally?.Invoke();
        
        if (IsServer)
        {
            NetworkObject.Despawn(true); // Détruit l'objet à travers le réseau
        }
    }
}