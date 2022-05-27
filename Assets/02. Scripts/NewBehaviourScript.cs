using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mikrocosmos
{
    public class NewBehaviourScript : MonoBehaviour {
        [SerializeField] private List<string> list;
        // Start is called before the first frame update
        void Start()
        {
            list = list.OrderBy(s => s[0].ToString().ToUpper()[0]).Reverse().Where((s => s[0].ToString() == "A" || s[0].ToString() == "C")). ToList();
        }

       
        void Update()
        {
        
        }
    }
}
