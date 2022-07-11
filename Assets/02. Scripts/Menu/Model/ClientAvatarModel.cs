using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Mirror.FizzySteam;
using UnityEngine;

namespace Mikrocosmos
{
    [Serializable]
    [ES3Serializable]
    public class AvatarElement {
        [ES3Serializable]
        public int ElementIndex;
        [ES3Serializable]
        public Vector2 Offset;

        public AvatarElement(int elementIndex, Vector2 offset) {
            ElementIndex = elementIndex;
            Offset = offset;
        }

        public AvatarElement() {
            ElementIndex = 0;
            Offset = Vector2.zero;
        }
    }
    

    [Serializable] 
    [ES3Serializable] 
    public class Avatar {
        [ES3Serializable] 
        public List<AvatarElement> Elements;

        public Avatar(IEnumerable<AvatarElement> elements) {
            Elements = new List<AvatarElement>();
            Elements.AddRange(elements);
        }
        public Avatar() {
            Elements = new List<AvatarElement>();
        }
    }



    public interface IClientAvatarModel {
        public Avatar Avatar { get; }
    }
    public class ClientAvatarModel : AbstractModel, IClientAvatarModel {
        
        protected override void OnInit() {
            Avatar = ES3.Load<Avatar>("client_avatar", defaultValue: null);
            
        }

        public Avatar Avatar { get; protected set; }
    }
}
