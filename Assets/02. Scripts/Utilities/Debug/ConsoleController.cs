using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Mirror;
using NHibernate.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class ConsoleController : AbstractMikroController<Mikrocosmos>
    {
        //0������1������

      
        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private GameObject outputTextPrefab = null;
        [SerializeField] private RectTransform textLayout;
        [SerializeField] private GameObject messageTextPrefab = null;

        private ICommandSystem commandSystem;
        private List<GameObject> allTexts = new List<GameObject>();
        
        private PlayerMatchInfo matchInfo;
        private void Awake() {
           
        }

      

        #region ���ܷ���
        /// <summary>
        /// ��һ֡����֮ǰ������
        /// </summary>
        private void Start()
        {
            inputField.ActivateInputField();
            this.RegisterEvent<OnLogMessage>(OnLogError).UnRegisterWhenGameObjectDestroyed(gameObject);
            commandSystem = this.GetSystem<ICommandSystem>();
            matchInfo = this.GetSystem<IRoomMatchSystem>().ClientGetMatchInfoCopy();
            this.RegisterEvent<OnClientReceiveMessage>(OnClientReceiveChatMessage)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnClientReceiveChatMessage(OnClientReceiveMessage e) {
            GameObject message = Instantiate(messageTextPrefab, textLayout);
            TMP_Text nameText = message.GetComponent<TMP_Text>();
            nameText.text = e.Name;
            nameText.color = e.Team == matchInfo.Team ? Color.green : Color.red;
            message.GetComponentInChildren<AvatarElementViewController>(true).SetAvatar(e.avatar);
            message.transform.Find("Message").GetComponent<TMP_Text>().text = e.Message;
            //message.GetComponent<ChatMessage>().Refresh();
            //StartCoroutine(RefreshMessage(message));
            LayoutRebuilder.ForceRebuildLayoutImmediate(textLayout);
            allTexts.Add(message);
        }

        private void OnLogError(OnLogMessage e) {
            AddOutputText(e.message+"\n");
        }

        private void AddOutputText(string message) {
            GameObject outputText = Instantiate(outputTextPrefab, textLayout);
            TMP_Text text = outputText.GetComponent<TMP_Text>();
            text.text = message;
            text.DOFade(0, 0);
            allTexts.Add(outputText);
            StartCoroutine(RefreshOutputPanel(text));
        }

        private IEnumerator RefreshMessage(GameObject message) {
            textLayout.sizeDelta += new Vector2(0.1f, 0);
            yield return new WaitForEndOfFrame();
            textLayout.sizeDelta -= new Vector2(0.1f, 0);
        }
        private IEnumerator RefreshOutputPanel(TMP_Text outputText) {
            textLayout.sizeDelta += new Vector2(0.1f, 0);
            yield return new WaitForEndOfFrame();
            textLayout.sizeDelta -= new Vector2(0.1f, 0);
            outputText.DOFade(1, 0);
        }
        
        /// <summary>
        /// ��֡ˢ��ʱ������
        /// </summary>
        private void Update()
        {
            string input = inputField.text;     // ��ȡ�����ı�
            // ���»س�����ָ��
            if (Input.GetKeyDown(KeyCode.Return)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) { //new line
                    inputField.text += "\n";
                    inputField.ForceLabelUpdate();
                }
                else //output text
                {
                    if (!input.Equals("")) {
                        input = input.Trim();
                        if (input.StartsWith("/")) { //is command
                            string output = "\n>> " + $"<b>{input}</b>" + "\n";
                            AddOutputText(output);
                            input = input.TrimStart('/');
                            string o = Console.Input(input);
                            if (o != null)
                            {
                                if (o.Equals("cls"))
                                {
                                    foreach (GameObject allText in allTexts)
                                    {
                                        Destroy(allText);
                                    }
                                }
                                else
                                {
                                    output = $"<color=red>{o}</color>" + "\n";
                                    AddOutputText(output);
                                }
                            }
                        }else {
                            this.GetSystem<ICommandSystem>().CmdSendChatMessage(NetworkClient.localPlayer, input);
                            Console.AddToConsoleHistory(input);
                        }
                        inputField.text = "";
                        inputField.ForceLabelUpdate();
                    }







                }
            }
            // ��������ת����һ��ָ��
            else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                inputField.text = Console.Last();
                inputField.caretPosition = inputField.text.Length;
            }
            
            // ��������ת����һ��ָ��
            else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                inputField.text = Console.Next();
                inputField.caretPosition = inputField.text.Length;
                inputField.ForceLabelUpdate();
            }
               
            inputField.ActivateInputField();
          
        }
        #endregion
    }
}
