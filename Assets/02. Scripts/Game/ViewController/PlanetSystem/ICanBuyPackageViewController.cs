using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

namespace Mikrocosmos
{
    public interface ICanBuyPackageViewController : IController
    {
        ICanBuyPackage BuyPackageModel { get; }
    }
}
