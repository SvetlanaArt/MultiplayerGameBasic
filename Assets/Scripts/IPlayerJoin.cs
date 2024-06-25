using System;
using Unity.Networking.Transport.Relay;

public interface IPlayerJoin
{
    public event Action<string> OnPlayerNameGanged;
    public event Action<bool, RelayServerData> OnConnecting;
    public void QuickJoinGame();
}
