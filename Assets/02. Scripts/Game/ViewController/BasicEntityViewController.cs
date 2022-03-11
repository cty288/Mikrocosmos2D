using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public abstract class BasicEntityViewController<T> : AbstractNetworkedController<Mikrocosmos> where T:IEntity {
        protected T model;

        protected virtual void Awake() {
            model = GetComponent<T>();
        }
    }
}
