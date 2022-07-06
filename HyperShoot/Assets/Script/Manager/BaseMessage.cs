using HyperShoot.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HyperShoot.Manager.MissonData;

public class BaseMessage
{
    public struct EnemyDieMessage
    {
        public EnemyType enemyType;
    }
    public struct FindMessage
    {
        
    }
    public struct SpawnMissonMessage
    {
        public Transform position;
    }
    public struct MissonComplete
    {

    }
    public struct ActiveArtifact { }

}
