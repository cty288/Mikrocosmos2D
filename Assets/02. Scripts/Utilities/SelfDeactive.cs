using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class SelfDeactive : MonoBehaviour{
        public void DeactiveSelf() {
            gameObject.SetActive(false);
        }
    }
}
