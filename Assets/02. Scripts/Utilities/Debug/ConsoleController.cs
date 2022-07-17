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


        #region 功能方法
        /// <summary>
        /// 第一帧调用之前触发。
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
        /// 在帧刷新时触发。
        /// </summary>
        private void Update()
        {
            string input = inputField.text;     // 获取输入文本
            // 按下回车输入指令
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!input.Equals(""))
                {
                    ouputText.text += "\n>> " + $"<b>{input}</b>" + "\n";
                    string output = Console.Input(input);
                    if (output != null)
                    {
                        // 回调信息为cls时清空控制台面板内容
                        if (output.Equals("cls"))
                            ouputText.text = "";
                        else
                            ouputText.text += $"<color=red>{output}</color>" + "\n";
                    }
                    inputField.text = "";
                }
            }
            // 按下上跳转到上一条指令
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                inputField.text = Console.Last();
            // 按下下跳转到下一条指令
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                inputField.text = Console.Next();
            inputField.ActivateInputField();
        }
        #endregion
    }
}
