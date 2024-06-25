using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Networking.Transport.Relay;


public class LobbyManager : MonoBehaviour, IConnection, IPlayerJoin
{
    [SerializeField] int numberPlayers = 2;
    [SerializeField] float heartbeatTimer = 15f;

    const string KEY_PLAYER_NAME = "PlayerName";
    const string KEY_JOIN_CODE = "RelayJoinCode";
    const string CONNECTION_TYPE = "dtls";

    private Lobby joinedLobby;
    private string playerName;

    public event Action<GameMessageType> OnConnectionEvent;
    public event Action<string> OnPlayerNameGanged;
    public event Action<bool, RelayServerData> OnConnecting;

    private void Awake()
    {
        string playerName = KEY_PLAYER_NAME + UnityEngine.Random.Range(10, 99).ToString();
        Authenticate(playerName);
        OnPlayerNameGanged?.Invoke(playerName);
    }

    public async void Authenticate(string playerName)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await InitServices(playerName);
            await SignInAnonymously();
        }
    }

    private async Task InitServices(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);
        await UnityServices.InitializeAsync(initializationOptions);
    }

    private async Task SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void QuickJoinGame()
    {
        QuickJoinLobby();
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;
            OnConnectionEvent?.Invoke(GameMessageType.joinLobby);

            string relayJoinCode = joinedLobby.Data[KEY_JOIN_CODE].Value;
            JoinAllocation allocation = await JoinRelay(relayJoinCode);
            OnConnectionEvent?.Invoke(GameMessageType.joinLobby);

            RelayServerData serverData = new RelayServerData(allocation, CONNECTION_TYPE);
            OnConnecting?.Invoke(false, serverData);
        }
        catch (LobbyServiceException ex)
        {
            OnConnectionEvent?.Invoke(GameMessageType.noLobby);
            Debug.Log("No one lobby to connect " + ex.Message);
            CreateLobby(playerName);
        }
        catch (RelayServiceException ex)
        {
            Debug.Log("Relay join error " + ex.Message);
        }
    }

    async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate = false)
    {
        Player player = GetPlayer();
        try
        {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);
            
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject> {
                        {KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, numberPlayers, options);
            joinedLobby = lobby;
            OnConnectionEvent?.Invoke(GameMessageType.createLobby);

            await JoinRelay(relayJoinCode);
            OnConnectionEvent?.Invoke(GameMessageType.joinRelay);

            RelayServerData serverData = new RelayServerData(allocation, CONNECTION_TYPE);
            OnConnecting?.Invoke(true, serverData);
  
            if (IsLobbyHost())
            {
                StartCoroutine(LobbyHeartbeat(joinedLobby.Id, heartbeatTimer));
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log("Error lobby creation " + ex.Message);
        }

    }

    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            return default;
        }
    }

    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
            return default;
        }
    }

    IEnumerator LobbyHeartbeat(string lobbyId, float heartbeatTime)
    {
        var delayTime = new WaitForSeconds(heartbeatTime);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delayTime;
        }
    }

    private void OnDestroy()
    {
        if (IsLobbyHost())
        {
            Lobbies.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
        });
    }

 
}