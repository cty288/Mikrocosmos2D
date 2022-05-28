using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class PoisonSpearModel : BasicGoodsModel{
        [SerializeField, SyncVar]
        private float addedForce;

        [SerializeField]
        private float addedMomentum;

        public float AddedMomentum => addedMomentum;

        [SerializeField]
        private int addedDamage;

        public int AddedDamage => addedDamage;

        public float AddedForce => addedForce;

        [SerializeField] private float poisonTime;

        public float PoisonTime => poisonTime;

        [SerializeField] private int poisonDamage;

        public int PoisonDamage => poisonDamage;
        
        public override void OnUsed()
        {

        }
    }
}
