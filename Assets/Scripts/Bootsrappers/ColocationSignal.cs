using System;
using Unity.Netcode;
using UnityEngine;

namespace Colocation.Network
{
    /// <summary>
    /// Lightweight NetworkBehaviour that carries the "calibrate now" signal
    /// from the AR client to the VR client.
    /// 
    /// Lives on a NetworkObject that both clients can see (e.g. a shared
    /// session manager GO, or the AR player's NetworkObject).
    /// 
    /// Uses Distributed Authority RPCs — no server involved.
    /// Any client with authority on this object can broadcast.
    /// 
    /// Flow:
    ///   1. AR bootstrapper calls ColocationSignal.Instance.SendCalibrationSignal()
    ///   2. This sends an Rpc to all other clients (SendTo.NotMe)
    ///   3. The VR bootstrapper listens to OnCalibrationRequested and auto-calibrates
    ///   
    /// No pose data is transmitted — each client calibrates from its own local sensors.
    /// The signal just means: "I scanned you, lock your origin NOW."
    /// </summary>
    public class ColocationSignal : NetworkBehaviour
    {
        /// <summary>
        /// Fired on every client when the AR player has completed its scan.
        /// The VR client should immediately capture its HMD pose as origin.
        /// </summary>
        public static event Action OnCalibrationRequested;

        private static ColocationSignal _instance;
        public static ColocationSignal Instance => _instance;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (_instance == null)
                _instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (_instance == this)
                _instance = null;

            base.OnNetworkDespawn();
        }

        /// <summary>
        /// Called by the AR bootstrapper after it commits its anchor pose.
        /// Broadcasts to all other clients and fires locally.
        /// </summary>
        public void SendCalibrationSignal()
        {
            Debug.Log("[ColocationSignal] AR scan complete — broadcasting calibration signal.");

            // Notify remote clients
            BroadcastCalibrationRpc();

            // Also fire locally (the AR client may want to react too)
            OnCalibrationRequested?.Invoke();
        }

        [Rpc(SendTo.NotMe)]
        private void BroadcastCalibrationRpc()
        {
            Debug.Log("[ColocationSignal] Calibration signal received from AR client.");
            OnCalibrationRequested?.Invoke();
        }
    }
}