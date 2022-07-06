using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Combat
{
    public class PropDamagerHandler : DamageHandler
    {
        [System.Serializable]
        public class ItemRate
        {
            public GameObject item;
            public float rate;

            public ItemRate(GameObject item, float rate)
            {
                this.item = item;
                this.rate = rate;
            }
        }

        [SerializeField] private List<ItemRate> itemRates;
        public override void Die()
        {
            base.Die();
            List<ItemRate> temp = new List<ItemRate>();
            float rate = 0;
            for (int i = 0; i < itemRates.Count; i++)
            {
                rate += itemRates[i].rate * 10f;
                temp.Add(new ItemRate(itemRates[i].item, rate));
            }
            int random = Random.Range(0, 1001);
            for (int i = 0; i < temp.Count; i++)
            {
                if (random <= temp[i].rate)
                {
                    SimplePool.Spawn(temp[i].item, transform.position + Vector3.up * 1.2f, Quaternion.identity);
                    break;
                }
            }
            Destroy(gameObject);
        }
    }
}
