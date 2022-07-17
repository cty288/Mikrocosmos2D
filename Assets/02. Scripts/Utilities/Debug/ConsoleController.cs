using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class ConsoleController : AbstractMikroController<Mikrocosmos>
    {

        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField]private TMP_Text ouputText = null;


        #region ���ܷ���
        /// <summary>
        /// ��һ֡����֮ǰ������
        /// </summary>
        private void Start()
        {
            inputField.ActivateInputField();
            this.RegisterEvent<OnLogMessage>(OnLogError).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnLogError(OnLogMessage e) {
            ouputText.text += $"{e.message}\n";
        }

        /// <summary>
        /// ��֡ˢ��ʱ������
        /// </summary>
        private void Update()
        {
            string input = inputField.text;     // ��ȡ�����ı�
            // ���»س�����ָ��
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!input.Equals(""))
                {
                    ouputText.text += "\n>> " + $"<b>{input}</b>" + "\n";
                    string output = Console.Input(input);
                    if (output != null)
                    {
                        // �ص���ϢΪclsʱ��տ���̨�������
                        if (output.Equals("cls"))
                            ouputText.text = "";
                        else
                            ouputText.text += $"<color=red>{output}</color>" + "\n";
                    }
                    inputField.text = "";
                }
            }
            // ��������ת����һ��ָ��
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                inputField.text = Console.Last();
            // ��������ת����һ��ָ��
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                inputField.text = Console.Next();
            inputField.ActivateInputField();
        }
        #endregion
    }
}
