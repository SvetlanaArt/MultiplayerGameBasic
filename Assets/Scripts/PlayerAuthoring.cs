using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
}

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {

        public override void Bake(PlayerAuthoring authoring)
        {
            PlayerData component = default(PlayerData);
            AddComponent(component);
        }
    }
}