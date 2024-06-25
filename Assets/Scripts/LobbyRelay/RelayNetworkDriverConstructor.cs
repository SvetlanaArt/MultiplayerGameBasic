using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport.Relay;

struct RelayNetworkDriverConstructor : INetworkStreamDriverConstructor
{
    private RelayServerData relayData;

    public RelayNetworkDriverConstructor (RelayServerData relayServerData)
    {
        relayData = relayServerData;
    }

    public RelayServerData GetRelayServerData()
    {
        return relayData;
    }

    public void CreateClientDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        DefaultDriverBuilder.RegisterClientDriver(world, ref driverStore, netDebug, ref relayData);
    }

    public void CreateServerDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        DefaultDriverBuilder.RegisterServerDriver(world, ref driverStore, netDebug, ref relayData);
    }
}