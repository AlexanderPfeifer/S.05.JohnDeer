using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class ZombiePlayerHordeRegistry : MonoBehaviour
{
    public List<GameObject> Zombies { get; private set; } = new List<GameObject>();

    [SerializeField] private int playerIndex;
    [HideInInspector] public NecromanceHorde necromanceHorde;

    public CinemachineTargetGroup TargetGroup;

    private void Start()
    {
        necromanceHorde = GetComponent<NecromanceHorde>();
    }

    public void RegisterZombie(GameObject zombie)
    {
        if(!Zombies.Contains(zombie))
        {
            Zombies.Add(zombie);
            TargetGroup.AddMember(zombie.transform, 1, .5f);
        }
    }

    public void UnregisterZombie(GameObject zombie)
    {
        if (Zombies.Contains(zombie))
        {
            Zombies.Remove(zombie);
            TargetGroup.RemoveMember(zombie.transform);

            if (Zombies.Count == 0 )
            {
                necromanceHorde.SpawnPlayerZombies();
            }

            Destroy(zombie);
        }
    }

    public int GetPlayerIndex()
    { 
        return playerIndex; 
    }
}