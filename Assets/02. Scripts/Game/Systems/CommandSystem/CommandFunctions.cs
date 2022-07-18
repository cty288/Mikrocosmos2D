using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Mikrocosmos
{
    public static class CommandFunctions {
        public static string ProcessArgument(string inputArg, out bool parseSuccess, out string parseFailedMessage) {
            //find the first ( and last )
            int firstBracket = inputArg.IndexOf('(');
            int lastBracket = inputArg.LastIndexOf(')');
            parseFailedMessage = "";
            //if there are no brackets, return the inputArg
            if (firstBracket == -1 || lastBracket == -1) {
                parseSuccess = true;
                return inputArg;
            }
            //if there are brackets, get the function name first
            string functionName = inputArg.Substring(0, firstBracket);
            //use reflections to get the function
            System.Reflection.MethodInfo function = typeof(CommandFunctions).GetMethod(functionName);
            //if the function is null, return the inputArg
            if (function == null) {
                parseSuccess = true;
                return inputArg;
            }

            
           
            string insideBrackets = inputArg.Substring(firstBracket + 1, lastBracket - firstBracket - 1);
            
            List<string> arguments = new List<string>();
            //iteratively get all the arguments, but don't count commas inside brackets as arguments, also consider nested brackets
            int bracketDepth = 0;
            int lastComma = 0;
            for (int i = 0; i < insideBrackets.Length; i++)
            {
                if (insideBrackets[i] == '(')
                {
                    bracketDepth++;
                }
                else if (insideBrackets[i] == ')')
                {
                    bracketDepth--;
                }
                else if (insideBrackets[i] == ',' && bracketDepth == 0)
                {
                    arguments.Add(insideBrackets.Substring(lastComma, i - lastComma));
                    lastComma = i + 1;
                }
            }
            arguments.Add(insideBrackets.Substring(lastComma, insideBrackets.Length - lastComma));



            //check if the function has the correct number of arguments
            if (function.GetParameters().Length != arguments.Count) {
                parseSuccess = false;
                parseFailedMessage = "Wrong number of arguments for function " + functionName;
                return inputArg;
            }

            //for each argument, use recursive call to process the argument
            string[] processedArguments = new string[arguments.Count];
            for (int i = 0; i < arguments.Count; i++) {
                bool success = false;
                processedArguments[i] = ProcessArgument(arguments[i], out success, out parseFailedMessage);
                if (!success) {
                    parseSuccess = false;
                    return inputArg;
                }
            }

            //try parse the arguments to the function's parameters
            object[] parsedArguments = new object[processedArguments.Length];
            ParameterInfo[] functionParameters = function.GetParameters();
            
            for (int i = 0; i < functionParameters.Length; i++) {
                //try parse the argument to the parameter's type
                try {
                    parsedArguments[i] = System.Convert.ChangeType(processedArguments[i], functionParameters[i].ParameterType);
                }
                catch (System.Exception e) {
                    parseSuccess = false;
                    parseFailedMessage = "Could not parse argument " + i + " to type " + functionParameters[i].ParameterType.Name;
                    return inputArg;
                }
            }

            //try invoke the function with the parsed arguments
            try {
                object result = function.Invoke(null, parsedArguments);
                parseSuccess = true;
                return result.ToString();
            }
            catch (System.Exception e)
            {
                parseSuccess = false;
                parseFailedMessage = "Could not invoke function " + functionName;
                return inputArg;
            }
        }

        public static int RandomInt(int min, int max) {
            return UnityEngine.Random.Range(min, max+1);
        }

        public static float RandomFloat(float min, float max) {
            return UnityEngine.Random.Range(min, max);
        }

    }
}
