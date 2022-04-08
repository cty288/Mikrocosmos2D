using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mikrocosmos
{
    public class ControlablePlanetViewController : MonoBehaviour
    {

            public GameObject target;
           
            public float speed = 100;
            float progress = 0;
            float distance = 0;
            public float x = 5;
            public float z = 7;

            private Rigidbody2D rigidbody;

            private void Awake() {
                rigidbody = GetComponent<Rigidbody2D>();
                distance = Vector3.Distance(target.transform.position, transform.position);
                progress = Random.Range(0, 360000);
            }
            private void FixedUpdate() {
                List<int> list = new List<int>();
                for (int i = 0; i < list.Count; i++) {
                    
                }
                OvalRotate();
            }


            void OvalRotate()
            {

                progress += Time.deltaTime * speed;
                Vector3 p = new Vector3(x * Mathf.Cos(progress * Mathf.Deg2Rad), z * Mathf.Sin(progress * Mathf.Deg2Rad) * distance, 0);
                rigidbody.MovePosition( target.transform.position + p);
            }
    }

    
}
