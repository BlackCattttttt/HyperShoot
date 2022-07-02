using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Manager
{
    [CreateAssetMenu(fileName = "MissonData", menuName = "ScriptableObjects/Database/MissonData", order = 0)]
    [System.Serializable]
    public class MissonData : ScriptableObject
    {
        [SerializeField] private List<LevelDesign> levelDesigns;
        public List<MissonAtribute> GetMissonInLevel(int lv)
        {
            for (int i = 0; i < levelDesigns.Count; i++)
            {
                if (levelDesigns[i].level == lv)
                    return levelDesigns[i].missons;
            }
            return null;
        }
        [System.Serializable]
        public class LevelDesign
        {
            public int level;
            public List<MissonAtribute> missons;
        }
        [System.Serializable]
        public class MissonAtribute
        {
            public enum MissonType
            {
                KILL_ENEMY = 0,
                COLLECT = 1,
                FIND = 2
            }
            public int missonId;
            public string missonDes;
            public Sprite icon;
            public MissonType skillType;
            public BaseMisson misson;
        }
    } 
}
