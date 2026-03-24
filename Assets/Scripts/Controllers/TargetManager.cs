using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager
{
    public event Action<int> OnTargetCountChanged;  // int = targets restantes
    public event Action OnAllTargetsDestroyed;

    private readonly Dictionary<SharedTargetView, SharedTargetController> _controllers = new();

    public int RemainingTargets => _controllers.Count;

    public void Register(SharedTargetView view, SharedTargetNetworkSync networkSync)
    {
        if (_controllers.ContainsKey(view))
            return;

        var controller = new SharedTargetController(view, networkSync, OnTargetDestroyed);
        _controllers[view] = controller;

        OnTargetCountChanged?.Invoke(RemainingTargets);

        Debug.Log($"[TargetManager] Target enregistrée. Total : {RemainingTargets}");
    }

    private void OnTargetDestroyed(SharedTargetController controller)
    {
        SharedTargetView keyToRemove = null;

        foreach (var kvp in _controllers)
        {
            if (kvp.Value == controller)
            {
                keyToRemove = kvp.Key;
                break;
            }
        }

        if (keyToRemove != null)
        {
            _controllers[keyToRemove].Dispose();
            _controllers.Remove(keyToRemove);
        }

        OnTargetCountChanged?.Invoke(RemainingTargets);
        Debug.Log($"[TargetManager] Target détruite. Restantes : {RemainingTargets}");

        if (RemainingTargets == 0)
            OnAllTargetsDestroyed?.Invoke();
    }
}