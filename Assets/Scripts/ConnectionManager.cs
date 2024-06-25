using System;
using System.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using UnityEngine;

public class ConnectionManager : MonoBehaviour, IConnection
{
    [Header ("Local connection")]
    [SerializeField] private bool isLocalConnection = false;
    [SerializeField] private string listenIP = "127.0.0.1";
    [SerializeField] private string connectIP = "127.0.0.1";
    [SerializeField] private ushort port = 7979;
    [SerializeField] LobbyManager joinManager;

    private bool isConnecting = false;
    private bool isServerStarted = false;

    public event Action<GameMessageType> OnConnectionEvent;

    private void Awake()
    {
        Application.runInBackground = true;
        joinManager.OnConnecting += Connect;
    }

    private void Start()
    {
        if (isLocalConnection)
            ConnectLocal();
    }

    public void Connect(bool isServer, RelayServerData relayServerData)
    {
        if (isConnecting)
        { return; }
        isConnecting = true;
        
        NetworkStreamReceiveSystem.DriverConstructor = new RelayNetworkDriverConstructor(relayServerData);
        if (isServer)
        {
            StartCoroutine(InitServerListening(relayServerData));
        }
        else
        {
            isServerStarted = true;
        }
        StartCoroutine(InitClientConnection(relayServerData));
    }

    private IEnumerator InitServerListening(RelayServerData relayServerData)
    {
        OnConnectionEvent?.Invoke(GameMessageType.createServer);
        while (!ClientServerBootstrap.HasServerWorld)
            yield return null;

        World serverWorld = ClientServerBootstrap.ServerWorld;
        EntityQuery query = GetQuery(serverWorld);
   
        query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.AnyIpv4);
        isServerStarted = true;
        OnConnectionEvent?.Invoke(GameMessageType.listening);
    }

    private IEnumerator InitClientConnection(RelayServerData relayServerData)
    {
        OnConnectionEvent?.Invoke(GameMessageType.connectingClient);
        while (!ClientServerBootstrap.HasClientWorlds && !isServerStarted)
            yield return null;

        World clientWorld =  ClientServerBootstrap.ClientWorld;
        EntityQuery query = GetQuery(clientWorld);
        query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(
                                                        clientWorld.EntityManager, 
                                                        relayServerData.Endpoint);
        isConnecting = false;
        OnConnectionEvent?.Invoke(GameMessageType.connected);
    }

    private EntityQuery GetQuery(World world)
    {
        return world.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
    }


    public void ConnectLocal()
    {
        if (isConnecting)
        { return; }
        isConnecting = true;
        StartCoroutine(LocalClientServerConnecting());
    }

    private IEnumerator LocalClientServerConnecting()
    {
        while (!ClientServerBootstrap.HasServerWorld)
            yield return null;

        while (!ClientServerBootstrap.HasClientWorlds )
            yield return null;

        World serverWorld = ClientServerBootstrap.ServerWorld;
        EntityQuery query = GetQuery(serverWorld);
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(listenIP, (ushort)port);
        query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(endpoint);
        
        World clientWorld =  ClientServerBootstrap.ClientWorld;
        query = GetQuery(clientWorld);
        endpoint = NetworkEndpoint.Parse(connectIP, (ushort)port);
        query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(
                                                        clientWorld.EntityManager, 
                                                        endpoint);
        isConnecting = false;
    }

    
}