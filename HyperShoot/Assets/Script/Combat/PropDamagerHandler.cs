using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Combat
{
    public class PropDamagerHandler : DamageHandler
    {
        public override void Die()
        {
            base.Die();

            fp_Utility.Destroy(gameObject);
        }
    }
}
