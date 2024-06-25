using Unity.Entities;
using UnityEngine;

namespace Samples.HelloNetcode
{
    public struct EnableRelayServer : IComponentData { }

     public class EnableRelayServerAuthoring : MonoBehaviour
    {
        class Baker : Baker<EnableRelayServerAuthoring>
        {
            public override void Bake(EnableRelayServerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnableRelayServer>(entity);
            }
        }
    }
}