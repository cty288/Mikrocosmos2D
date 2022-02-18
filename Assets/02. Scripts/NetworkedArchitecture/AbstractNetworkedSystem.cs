using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror;
using UnityEngine;

public abstract class AbstractNetworkedSystem: NetworkBehaviour, ISystem
{
    private IArchitecture architectureModel;

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
