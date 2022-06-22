using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MikroFramework.Utilities
{
    public class Trigger2DCheck : MonoBehaviour
    {
        public LayerMask TargetLayers;

        [SerializeField] private float maxDistance = 100;
        [SerializeField] private float detectTime = 1f;

        private float timer;

        private SimpleRC enterRC = new SimpleRC();

        [SerializeField]
        private List<Collider2D> colliders;
        /// <summary>
        /// Get all 2D colliders that are in the current trigger of this object
        /// </summary>
        public List<Collider2D> Colliders => colliders;

        /// <summary>
        /// If there are any collider in the trigger of this object
        /// </summary>
        public bool Triggered
        {
            get { return enterRC.RefCount > 0; }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= detectTime)
            {
                List<Collider2D> removedColliders = new List<Collider2D>();

                for (int i = 0; i < colliders.Count; i++) {
                    if (colliders[i] && Vector2.Distance(colliders[i].gameObject.transform.position, transform.position)
                        >= maxDistance) {
                        
                        enterRC.Release();
                        removedColliders.Add(colliders[i]);

                    }else if (colliders[i] && !colliders[i].gameObject.activeInHierarchy) {
                        
                        enterRC.Release();
                        removedColliders.Add(colliders[i]);
                    }
                }
               

                foreach (Collider2D removedCollider in removedColliders) {
                    colliders.Remove(removedCollider);
                }

                colliders.RemoveAll((collider2D1 => collider2D1 == null));

                timer = 0;
            }

        }

        private void OnTriggerStay2D(Collider2D other)
        {

            if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
            {
                if (colliders.Contains(other))
                {
                    return;
                }
                enterRC.Retain();
                colliders.Add(other);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
            {
                enterRC.Retain();
                colliders.Add(other);
            }

        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
            {
                //enterRC.Release();
                int length = colliders.FindAll(col => col == other).Count;
                for (int i = 0; i < length; i++)
                {
                    enterRC.Release();
                }
                colliders.RemoveAll(col => col == other);
            }

        }

        public void Clear()
        {
            enterRC = new SimpleRC();
            colliders.Clear();

        }
    }
}