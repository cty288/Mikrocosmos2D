using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{

    public enum CommandType {
        INT,
        BOOL,
        STRING
    }

    public struct CommandArg {
        public CommandType CommandArgType;
        public string Destription;
        public CommandArg(CommandType commandArgType, string destription) {
            CommandArgType = commandArgType;
            Destription = destription;
        }
    }
    /// <summary>
    /// ����̨��̬�࣬���ڳ����ڲ����ԡ�
    /// </summary>
    public static class Console
    {
        #region ָ���б�

        private static readonly Dictionary<string, CommandArg[]> commands = new Dictionary<string, CommandArg[]>() {
            {"help", Array.Empty<CommandArg>()},
            {"cls", Array.Empty<CommandArg>()},
            {"addMoney", new [] {new CommandArg(CommandType.INT, "value")}},
            {"gameManager", new [] {
                new CommandArg(CommandType.STRING,"playerName"),
                new CommandArg(CommandType.BOOL, "isManager")
            } }
        };
        
      
        #endregion

        #region ��̬��Ա����
        private static int position = -1;   // ��ǰ��ȡ��ʷ��¼��λ��
        private static List<string> consoleHistory = new List<string>();    // ����̨��ʷ��¼
        #endregion

        #region ��̬���з���
        /// <summary>
        /// �����̨����ָ�
        /// </summary>
        /// <param name="input">ָ���ַ�����</param>
        /// <returns>�ص���Ϣ��</returns>
        public static string Input(string input)
        {
            // �ָ��ַ�����ȡ�����б�
            List<string> args = new List<string>(input.Split(' '));
            consoleHistory.Add(input);
            position = consoleHistory.Count;
            // ������ص�
            string output = null;
            if (commands.ContainsKey(args[0])) {
                if (CheckArguments(args, commands[args[0]], out output)) {
                    switch (args[0]) {
                        // ����
                        case "help":
                            output = Show();
                            break;
                        // ��տ���̨
                        case "cls":
                            output = Clear();
                            break;
                        case "addMoney":
                            output = AddMoney(int.Parse(args[1]));
                            break;
                        case "gameManager":
                            output = GiveGameManager(args[1], bool.Parse(args[2]));
                            break;
                        // ����ָ��
                        default:
                            output = "Unable to find the command.";
                            break;
                    }
                }
               
            }
            else {
                output = "Unable to find the command.";
            }
           
            return output;
        }

        private static string GiveGameManager(string s, bool parse) {
            if (NetworkServer.active && !parse && s== NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().matchInfo.Name) {
                return "You can't remove yourself as a game manager because you are the host!";
            }
            
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdGiveGameManager(NetworkClient.localPlayer, s, parse);
            return "Notifying Server...";
        }

        private static string AddMoney(int parse) {
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdRequestAddMoneyCommand(NetworkClient.localPlayer, parse);
            return "Notifying Server...";
        }






        private static string GetCommandComplete(string commandName) {
            CommandArg[] args = commands[commandName];
            string output = "";
            output += $"<b><color=yellow>- {commandName}";
            foreach (CommandArg arg in args)
            {
                output += $"  <{arg.CommandArgType} {arg.Destription}>";
            }
            output += "</color></b>";
            return output;
        }
        
        
        private static bool CheckArguments(List<string> inputArgs, CommandArg[] commandArgs, out string output) {
            output = "";
            string commandName = inputArgs[0];
            if (inputArgs.Count -1 != commandArgs.Length) {
                output = "Invalid arguments.\n";
                output += GetCommandComplete(commandName);
                return false;
            }

            bool success = true;
            for (int i = 1; i < inputArgs.Count; i++) {
                CommandArg commandArg = commandArgs[i - 1];

                switch (commandArg.CommandArgType) {
                    case CommandType.INT:
                        if (!int.TryParse(inputArgs[i], out int _)) {
                            success = false;
                        } 
                        break;
                    case CommandType.BOOL:
                        if (!bool.TryParse(inputArgs[i], out bool _)) {
                            success = false;
                        }
                        break;
                    case CommandType.STRING:
                        break;
                }

                if (!success) {
                    output = $"Invalid arguments for {commandArg.CommandArgType}.\n";
                    break;
                }
            }

            if (!success) {
                output += GetCommandComplete(commandName);
                return false;
            }
            output = "";
            return true;
        }

        /// <summary>
        /// ��ȡ����̨��һ����ʷ��¼��
        /// </summary>
        /// <returns>��һ��ָ���ֶΡ�</returns>
        public static string Last()
        {
            if (position == -1)
                return null;
            position -= 1;
            if (position < 0)
                position = 0;
            return consoleHistory[position];
        }

        /// <summary>
        /// ��ȡ����̨��һ����ʷ��¼��
        /// </summary>
        /// <returns>��һ��ָ���ֶΡ�</returns>
        public static string Next()
        {
            if (position == -1)
                return null;
            position += 1;
            if (position >= consoleHistory.Count)
                position = consoleHistory.Count - 1;
            return consoleHistory[position];
        }
        #endregion

        #region ��̬˽�з���
        /// <summary>
        /// ��ʾȫ������̨���
        /// </summary>
        /// <returns>�ص���Ϣ��</returns>
        private static string Show()
        {
            string output = null;

            foreach (var commandsKey in commands.Keys) {
                output +=  GetCommandComplete(commandsKey); ;
                output += "\n";
            }
           
            return output;
        }

        /// <summary>
        /// ��տ���̨��¼��
        /// </summary>
        /// <returns>�ص���Ϣ��</returns>
        private static string Clear()
        {
            position = -1;
            consoleHistory.Clear();
            return "cls";
        }
        #endregion

        #region ����̨����
        /// <summary>
        /// ���Է�����
        /// </summary>
        /// <returns>�ص���Ϣ��</returns>
        private static string Test() {
            GameObject gameObject = new GameObject("Test");
            if (gameObject)
            {
                GameObject.Instantiate(gameObject);
                return "Object has been generated.";
            }
            return "There have no such object.";
        }
        #endregion
    }
}
