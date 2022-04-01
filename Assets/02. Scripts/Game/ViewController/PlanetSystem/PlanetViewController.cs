using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public class PlanetViewController : MonoBehaviour {
        [SerializeField] private float gravityFieldRange;
        [SerializeField] private LayerMask affectedPlanetLayers;

       
        [SerializeField] private float G = 1;

        [SerializeField] private Vector2 startDirection;
        [SerializeField] private long mass;
        [SerializeField]
        private float initialForce;

        [SerializeField] private float initialForceMultiplier;
        void Start()
        {
            Vector2 Center = this.transform.position;
            initialForce = ProperForce(this.gameObject);
            this.gameObject.GetComponent<Rigidbody2D>().AddForce(initialForce * ProperDirect(Center), ForceMode2D.Impulse);
        }

        private Vector2 ProperDirect(Vector2 pos) {
            float x = Random.value, y = Random.value / 10;
            Vector2 result;
            if (startDirection != Vector2.zero) {
                result = startDirection.normalized;
            }
            else {
                Vector2 starPos = GameObject.Find("Star").transform.position;
                result = Vector2.Perpendicular(((starPos - pos).normalized));
            }
            return result;
        }

        private float ProperForce(GameObject obj) {
            var pos = obj.transform.position;
            var rb = obj.GetComponent<Rigidbody2D>();
            var Rb = GameObject.Find("Star").GetComponent<Rigidbody2D>();
            return initialForceMultiplier* rb.mass * Mathf.Sqrt(Rb.mass / Distance(pos, Vector3.zero));
        }

        private void FixedUpdate() {
            KeepUniversalG();
        }

        void KeepUniversalG()
		{
			Vector2 Center = this.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(Center, gravityFieldRange, affectedPlanetLayers);

			foreach (Collider2D obj in colliders)
			{
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb && obj.gameObject!=gameObject) {
                    rb.AddExplosionForce(-1 * UniversalG(this.gameObject, obj.gameObject) * Time.deltaTime, Center, gravityFieldRange);
                }
					
			}

		}

        private float UniversalG(GameObject source, GameObject target) {
            var pos1 = source.transform.position;
            var pos2 = target.transform.position;
            Rigidbody2D rb1 = source.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = target.GetComponent<Rigidbody2D>();
            return (rb1.mass * rb2.mass / Distance(pos1, pos2)) * G;

        }
        float Distance(Vector2 pos1, Vector2 pos2)
        {
            Vector2 diff = (pos1 - pos2);
            float dist = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (dist < 1)
                return 1;
            else return (dist);
        }
     

    }
}
