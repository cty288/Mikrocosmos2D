using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ICollisionMaskModel : IModel {
        LayerMask Allocate();
        void Release();
    }
    public class CollisionMaskSystem : NetworkedModel, ICollisionMaskModel {
       [SerializeField]
        private int hookingObj = 0;

        private void Awake() {
            Mikrocosmos.Interface.RegisterModel<ICollisionMaskModel>(this);
        }

        public LayerMask Allocate() {
            int result = LayerMask.NameToLayer($"CollisionMask{hookingObj}");
            hookingObj++;
            return result;
        }

        public void Release() {
            hookingObj--;
        }
    }
}
