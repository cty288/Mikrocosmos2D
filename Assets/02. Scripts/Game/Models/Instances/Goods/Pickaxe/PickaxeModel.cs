using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class PickaxeModel : BasicGoodsModel {
        [SerializeField] private int damageToMeteors = 20;

        public int DamageToMeteors => damageToMeteors;
    }
}
