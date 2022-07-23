using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mikrocosmos
{

    public enum CommandType {
        INT,
        BOOL,
        STRING
    }

    public struct Command {
        public CommandArg[] Args;
        public bool Hidden;

        public Command(CommandArg[] args, bool hidden = false) {
            Args = args;
            Hidden = hidden;
        }
    }

    public struct CommandArg {
        public CommandType CommandArgType;
        public string ArgumentName;
        public object DefaultValue;
        public string Description;
        public Vector2 ValueRange;
        public CommandArg(CommandType commandArgType, string argumentName, string description = "", object defaultValue = null, Vector2 valueRange = new Vector2()) {
            CommandArgType = commandArgType;
            ArgumentName = argumentName;
            DefaultValue = defaultValue;
            Description = description;
            ValueRange = valueRange;
        }
    }
    /// <summary>
    /// ����̨��̬�࣬���ڳ����ڲ����ԡ�
    /// </summary>
    public static class Console
    {
        #region ָ���б�

        private static readonly Dictionary<string, Command> commands = new Dictionary<string, Command>() {
            {"help", new Command(Array.Empty<CommandArg>())},
            {"cls", new Command(Array.Empty<CommandArg>())},
            {"addMoney", new Command(new [] {
                new CommandArg(CommandType.INT, "value"),
                new CommandArg(CommandType.STRING, "playerName", "",NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().matchInfo.Name)
            })},
            {"gameManager",  new Command(new [] {
                new CommandArg(CommandType.STRING,"playerName"),
                new CommandArg(CommandType.BOOL, "isManager","", true),
            })},
            {"addBuff", new Command(new [] {
              
                new CommandArg(CommandType.INT, "buffID", "", null, new Vector2(0,4)),
                new CommandArg(CommandType.INT, "buffLevel", "",1, new Vector2(1,5)),
                new CommandArg(CommandType.STRING, "playerName","", NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().matchInfo.Name),
            })},
            {"dm", new Command(new [] {
                new CommandArg(CommandType.STRING, "message",""),
                new CommandArg(CommandType.STRING, "playerName", ""),
            })},
            {"imtherealdeveloper", new Command(Array.Empty<CommandArg>(), true)}
        };
        
      
        #endregion

        #region ��̬��Ա����
        private static int position = -1;   // ��ǰ��ȡ��ʷ��¼��λ��
        private static List<string> consoleHistory = new List<string>();    // ����̨��ʷ��¼
        #endregion

        #region ��̬���з���

        public static void AddToConsoleHistory(string text) {
            consoleHistory.Add(text);
            position = consoleHistory.Count;
        }

        /// <summary>
        /// �����̨����ָ�
        /// </summary>
        /// <param name="input">ָ���ַ�����</param>
        /// <returns>�ص���Ϣ��</returns>
        public static string Input(string input)
        {
            // �ָ��ַ�����ȡ�����б�
            List<string> args = new List<string>(input.Split(' '));
            consoleHistory.Add("/"+input);
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
                            output = AddMoney(int.Parse(args[1]), args[2]);
                            break;
                        case "gameManager":
                            output = GiveGameManager(args[1], bool.Parse(args[2]));
                            break;
                        case "addBuff":
                            output = AddBuff(args[3], int.Parse(args[1]), int.Parse(args[2]), args[0]);
                            break;
                        case "dm":
                            output = DM(args[1], args[2]);
                            break;
                        case "imtherealdeveloper":
                            output = Spectator();
                            break;
                        // ����ָ��
                        default:
                            output = "Unable to find the command. Type /help to view the command list.";
                            break;
                    }
                }
               
            }
            else {
                output = "Unable to find the command. Type /help to view the command list.";
            }
           
            return output;
        }

        private static string Spectator() {
            Mikrocosmos.Interface.GetSystem<ICommandSystem>().CmdRequestSpectator(NetworkClient.localPlayer);
            return "Notifying Server...";
        }

        private static string DM(string s, string s1) {
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdRequestDM(NetworkClient.localPlayer, s1, s);
            return "Notifying Server...";
        }

        private static string AddBuff(string s, int parse, int i, string commandName) {
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdRequestAddBuff(NetworkClient.localPlayer,s, parse, i, commandName);
            return "Notifying Server...";
        }

        private static string GiveGameManager(string s, bool parse) {
            if (NetworkServer.active && !parse && s== NetworkClient.localPlayer.GetComponent<NetworkMainGamePlayer>().matchInfo.Name) {
                return "You can't remove yourself as a game manager because you are the host!";
            }
            
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdGiveGameManager(NetworkClient.localPlayer, s, parse);
            return "Notifying Server...";
        }

        private static string AddMoney(int parse, string name) {
            Mikrocosmos.Interface.GetSystem<ICommandSystem>()
                .CmdRequestAddMoneyCommand(NetworkClient.localPlayer, parse, name);
            return "Notifying Server...";
        }





        //FF00C4
        private static string GetCommandComplete(string commandName) {
            if (!commands[commandName].Hidden) {
                CommandArg[] args = commands[commandName].Args;
                string output = "";
                output += $"<color=yellow><b>- /{commandName}</b>";
                foreach (CommandArg arg in args)
                {
                    string valueRangeString = "";
                    if (arg.ValueRange != Vector2.zero)
                    {
                        valueRangeString = $" <color=#FF00C4>{{{arg.ValueRange.x}:{arg.ValueRange.y}}}</color>";
                    }
                    if (arg.DefaultValue != null)
                    {
                        output += $"  [<b>{arg.CommandArgType}</b>: <color=orange>{arg.ArgumentName}{valueRangeString} = {arg.DefaultValue}</color>]";
                    }
                    else
                    {
                        output += $"  <<b>{arg.CommandArgType}</b>: <color=orange>{arg.ArgumentName}{valueRangeString}</color>>";
                    }

                }
                output += "</color>";
                return output;
            }

            return "";
        }
        
        
        private static bool CheckArguments(List<string> inputArgs, Command command, out string output) {
            output = "";
            string commandName = inputArgs[0];
            CommandArg[] commandArgs = command.Args;
            int defaultArgCount = commandArgs.Select(arg => arg.DefaultValue != null).Count();
            if (inputArgs.Count - 1 < commandArgs.Length - defaultArgCount || inputArgs.Count - 1 > commandArgs.Length) {
                output = "Invalid arguments.\n";
                output += GetCommandComplete(commandName);
                return false;
            }

            bool success = true;
            for (int i = 1; i < inputArgs.Count; i++) {
                CommandArg commandArg = commandArgs[i - 1];
                //use CommandFunction to try parse if the argument is a function
                inputArgs[i] = CommandFunctions.ProcessArgument(inputArgs[i], out bool parseSuccess,
                    out string parseFailedMessage);

                if (parseSuccess) {
                    switch (commandArg.CommandArgType)
                    {
                        case CommandType.INT:
                            if (!int.TryParse(inputArgs[i], out int _)) {
                                success = false;
                            }
                            
                            if (commandArg.ValueRange != Vector2.zero) {
                                if (int.Parse(inputArgs[i]) < commandArg.ValueRange.x || int.Parse(inputArgs[i]) > commandArg.ValueRange.y) {
                                    success = false;
                                }
                            }
                            break;
                        case CommandType.BOOL:
                            if (!bool.TryParse(inputArgs[i], out bool _))
                            {
                                success = false;
                            }
                            break;
                        case CommandType.STRING:
                            break;
                    }

                    if (!success)
                    {
                        output = $"Invalid arguments for {commandArg.CommandArgType}: {commandArg.ArgumentName}.\n";
                        break;
                    }
                }
                else {
                    output = parseFailedMessage +"\n";
                    output += GetCommandComplete(commandName);
                    return false;
                }
                
            }

           

            if (!success) {
                output += GetCommandComplete(commandName);
                return false;
            }


            for (int i = inputArgs.Count - 1; i < commandArgs.Length; i++)
            {
                CommandArg commandArg = commandArgs[i];
                if (commandArg.DefaultValue == null) {
                    success = false;
                    output = $"Missing arguments for {commandArg.CommandArgType}: {commandArg.ArgumentName}.\n";
                    break;
                }else {
                    inputArgs.Add(commandArg.DefaultValue.ToString());
                }
            }

            if (!success)
            {
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
                string temp = GetCommandComplete(commandsKey);
                if (!String.IsNullOrEmpty(temp)) {
                    output += temp;
                    output += "\n";
                }
               
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
