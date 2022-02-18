using System.Collections;

using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

public abstract class AbstractNetworkedController<T> : NetworkBehaviour, IController where T : NetworkedArchitecture<T>, new()
{

    IArchitecture IBelongToArchitecture.GetArchitecture()
    {

        return NetworkedArchitecture<T>.Interface;
    }
}