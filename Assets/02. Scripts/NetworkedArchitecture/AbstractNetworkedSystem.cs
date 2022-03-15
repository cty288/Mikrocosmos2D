using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

public abstract class AbstractNetworkedSystem: NetworkBehaviour, ISystem
{
    private IArchitecture architectureModel;

    protected T GetBindedModel<T>() where T: IModel {
        return GetComponent<T>();
    }

    IArchitecture IBelongToArchitecture.GetArchitecture()
    {
        return architectureModel;
    }

    void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
    {
        this.architectureModel = architecture;
    }

    void ISystem.Init()
    {
      
    }

   
}
