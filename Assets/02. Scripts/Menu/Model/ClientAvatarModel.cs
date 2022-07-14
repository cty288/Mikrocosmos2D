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

        [ES3Serializable] 
        public int Layer;

        public AvatarElement(int elementIndex, Vector2 offset, int layer) {
            ElementIndex = elementIndex;
            Offset = offset;
            this.Layer = layer;
        }

        public AvatarElement() {
            ElementIndex = 0;
            Offset = Vector2.zero;
        }

        public AvatarElement Clone() {
            return new AvatarElement(ElementIndex, Offset, Layer);
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

        public void AddElement(AvatarElement element) {
            Elements.Add(element);
        }

        public void RemoveElement(int index) {
            Elements.RemoveAll((element => element.ElementIndex == index));
        }
        public Avatar() {
            Elements = new List<AvatarElement>();
        }
    }



    public interface IClientAvatarModel: IModel {
        public Avatar Avatar { get; }
    }
    public class ClientAvatarModel : AbstractModel, IClientAvatarModel {
        
        protected override void OnInit() {
            Avatar = ES3.Load<Avatar>("client_avatar", defaultValue: new Avatar());
            
        }

        public Avatar Avatar { get; protected set; }
    }
}
