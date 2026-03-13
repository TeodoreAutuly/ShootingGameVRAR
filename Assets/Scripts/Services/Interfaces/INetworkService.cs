using System;

public interface INetworkService
{
    // Événements
    event Action<ulong> OnPlayerJoined;
    event Action<ulong> OnPlayerLeft;
    event Action OnDisconnectedFromServer;

    // Commandes
    bool StartHost();
    bool StartClient();
    void Disconnect();
}