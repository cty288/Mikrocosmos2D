
#if UNITY_5 || UNITY_2017_1_OR_NEWER
using JetBrains.Annotations;
#endif
using TMPro;
using UnityEngine;

namespace Polyglot
{
    [AddComponentMenu("Mesh/Localized TextMesh")]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMesh : MonoBehaviour, ILocalize
    {

        [Tooltip("The TextMesh component to localize")]
        [SerializeField]
        private TMP_Text text;

        [Tooltip("The key to localize with")]
        [SerializeField]
        private string key = null;

        public string Key { get { return key; } }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        [UsedImplicitly]
#endif
        public void Reset()
        {
            text = GetComponent<TMP_Text>();
        }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        [UsedImplicitly]
#endif
        public void Start()
        {
            Localization.Instance.AddOnLocalizeEvent(this);
        }

        public void OnLocalize()
        {
            var flags = text.hideFlags;
            text.hideFlags = HideFlags.DontSave;
            text.text = Localization.Get(key);

            var direction = Localization.Instance.SelectedLanguageDirection;

            if (IsOppositeDirection(text.alignment, direction))
            {
                switch (text.alignment)
                {
                    case TextAlignmentOptions.Left:
                        text.alignment = TextAlignmentOptions.Right;
                        break;
                    case TextAlignmentOptions.Right:
                        text.alignment = TextAlignmentOptions.Left;
                        break;
                }
            }
            text.hideFlags = flags;
        }

        private bool IsOppositeDirection(TextAlignmentOptions alignment, LanguageDirection direction)
        {
            return (direction == LanguageDirection.LeftToRight && IsAlignmentRight(alignment)) || (direction == LanguageDirection.RightToLeft && IsAlignmentLeft(alignment));
        }

        private bool IsAlignmentRight(TextAlignmentOptions alignment)
        {
            return alignment == TextAlignmentOptions.Right;
        }
        private bool IsAlignmentLeft(TextAlignmentOptions alignment)
        {
            return alignment == TextAlignmentOptions.Left;
        }
    }
}