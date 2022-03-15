using System.Collections;

using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

public abstract class AbstractNetworkedController<T> : NetworkBehaviour, IController where T : NetworkedArchitecture<T>, new()
{
    protected T GetModel<T>() where T:IModel {
        return GetComponent<T>();
    }

    IArchitecture IBelongToArchitecture.GetArchitecture() {
        return NetworkedArchitecture<T>.Interface;
    }
}