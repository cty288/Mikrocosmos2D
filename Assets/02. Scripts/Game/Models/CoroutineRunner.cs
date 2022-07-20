using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Singletons;
using UnityEngine;

namespace Mikrocosmos
{
    public class CoroutineRunner : MonoPersistentMikroSingleton<CoroutineRunner> {

        public void RunCoroutine(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }
    }
}
