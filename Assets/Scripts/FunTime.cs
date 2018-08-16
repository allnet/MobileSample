using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public class FunTime : StateController
    {
        public GameObject funtimePrefab;
        private GameObject clone;

        //public void Awake()
        //{

        //}

        private void Start()
        {
            clone = Instantiate(funtimePrefab, transform.position, transform.rotation);
        }

        public void OnEnable()
        {
           //clone = Instantiate(funtimePrefab, transform.position, transform.rotation);
        }

        public void OnDisable()
        {
            Destroy(clone);

        }
    }
}
