using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{
    public abstract class AbstractPlanetViewController: AbstractNetworkedController<Mikrocosmos>, ICanProducePackageViewController, IHaveGravityViewController
    {
        public ICanProducePackage PackageModel { get; protected set; }

        public IHaveGravity GravityModel { get; protected set; }
        private void Awake() {
            PackageModel = GetComponent<ICanProducePackage>();
            GravityModel = GetComponent<IHaveGravity>();
            rigidbody = GetComponent<Rigidbody2D>();
            distance = Vector3.Distance(target.transform.position, transform.position);
            progress = Random.Range(0, 360000);
        }
        public GameObject target;

        public float speed = 100;
        float progress = 0;
        float distance = 0;
        public float x = 5;
        public float z = 7;

        private Rigidbody2D rigidbody;

      
       

        void OvalRotate()
        {

            progress += Time.deltaTime * speed;
            Vector3 p = new Vector3(x * Mathf.Cos(progress * Mathf.Deg2Rad), z * Mathf.Sin(progress * Mathf.Deg2Rad) * distance, 0);
            rigidbody.MovePosition(target.transform.position + p);
        }
        //start refactor
        private void FixedUpdate()
        {
            OvalRotate();
            if (isServer)
            {
                KeepUniversalG();
            }
        }

        [ServerCallback]
        void KeepUniversalG()
        {
            Vector2 Center = this.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(Center, GravityModel.GravityFieldRange, GravityModel.AffectedLayerMasks);

            foreach (Collider2D obj in colliders)
            {

                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

                if (rb && obj.gameObject != gameObject)
                {
                    if (rb.TryGetComponent<IAffectedByGravity>(out IAffectedByGravity target))
                    {
                        float explosionForce = -1 * UniversalG(GravityModel, target, transform.position, rb.transform.position) * Time.deltaTime;
                        target.ServerAddGravityForce(explosionForce, Center, GravityModel.GravityFieldRange);
                    }

                }

            }

        }

        private float UniversalG(IHaveMomentum source, IHaveMomentum target, Vector2 sourcePos, Vector2 targetPos)
        {

            float sorceMass = source.GetTotalMass();
            float destMass = target.GetTotalMass();
            return (sorceMass * destMass / Distance(sourcePos, targetPos)) * GravityModel.G;

        }

        protected float Distance(Vector2 pos1, Vector2 pos2)
        {
            Vector2 diff = (pos1 - pos2);
            float dist = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
            if (dist < 1)
                return 1;
            else return (dist);
        }
        //end 


    }
}
