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
        public Dictionary<int, AvatarElement> Elements;

        public List<AvatarElement> GetAllElements() {
            List<AvatarElement> elements = new List<AvatarElement>();
            foreach (var element in Elements.Values) {
                elements.Add(element);
            }   

            return elements;
        }

        public Avatar(IEnumerable<AvatarElement> elements) {
            Elements = new Dictionary<int, AvatarElement>();
            foreach (var element in elements)
            {
                Elements.Add(element.ElementIndex, element);
            }
        }

        public void AddElement(AvatarElement element) {
            if (!Elements.ContainsKey(element.ElementIndex)) {
                Elements.Add(element.ElementIndex, element);
            }
        }

        public void UpdateOffset(int index, Vector2 offset) {
            if (Elements.ContainsKey(index)) {
                Elements[index].Offset = offset;
            }
        }

        public void RemoveElement(int index) {
            Elements.Remove(index);
        }
        public Avatar() {
            Elements = new Dictionary<int, AvatarElement>();
        }
    }



    public interface IClientAvatarModel: IModel {
        public Avatar Avatar { get; }

        void SaveAvatar(Avatar avatar);
    }
    public class ClientAvatarModel : AbstractModel, IClientAvatarModel {
        
        protected override void OnInit() {
            Avatar = ES3.Load<Avatar>("client_avatar", defaultValue: new Avatar());
            
        }

        

        public Avatar Avatar { get; protected set; }
        public void SaveAvatar(Avatar avatar) {
            Avatar = avatar;
            ES3.Save("client_avatar", avatar);
        }
    }
}
