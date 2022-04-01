using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class MaskableVisionControlExample : MaskableVisionControl
    {
        public override void OnStartServer() {
            base.OnStartServer();
            //StartCoroutine(TurnMask());
        }

        private void Update() {
            if (isServer) {
               
                if (Input.GetKeyDown(KeyCode.P)) {
                    //ServerSetClientAlwaysUnMaskable(connectionToClient);
                }
            }

           
        }

        private IEnumerator TurnMask() {
            while (true) {
                yield return new WaitForSeconds(3f);
                IsMaskable = !IsMaskable;
            }
        }
    }
}
