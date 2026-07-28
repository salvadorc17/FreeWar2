using System;
using System.Collections.Generic;
using System.Text;

namespace FactionsGame
{
    public class ConsoleController
    {
        FactionsGame _engine;


        public enum ConsoleCommands
        {
            map,
            exit
        };



        public ConsoleController(FactionsGame engine)
        {
            _engine = engine;
        }



        public string ParseCommand(string command)
        {
            string response = string.Empty;
            if (!String.IsNullOrEmpty(command))
            {
                bool parseOK = false;
                string[] commandArray;

                // Break the string into an array separated by spaces
                commandArray = command.Split(new char[] { ' ' });
                if (commandArray.Length > 0)
                {
                    if (commandArray[0].Length > 0)
                    {
                        parseOK = true;
                    }
                }


                if (parseOK)
                {
                    switch (commandArray[0].ToLower())
                    {
                        case "debug":
                            if (commandArray.Length == 2)
                            {
                                int argValue = ParseArgumentBoolean(commandArray[1]);
                                if (argValue != -1)
                                {
                                    if (argValue == 0)
                                        _engine.ShowDebugInfo = false;
                                    else
                                        _engine.ShowDebugInfo = true;

                                    response = commandArray[0] + " " + argValue.ToString();
                                }
                                else
                                    response = "Invalid argument: " + commandArray[1];
                            }
                            else
                                response = commandArray[0] + " is " + _engine.ShowDebugInfo.ToString();
                            break;
                        case "show_path":
                            if (commandArray.Length == 2)
                            {
                                int argValue = ParseArgumentBoolean(commandArray[1]);
                                if (argValue != -1)
                                {
                                    if (argValue == 0)
                                        _engine.PathDebugEnabled = false;
                                    else
                                        _engine.PathDebugEnabled = true;

                                    response = commandArray[0] + " " + argValue.ToString();
                                }
                                else
                                    response = "Invalid argument: " + commandArray[1];
                            }
                            else
                                response = commandArray[0] + " is " + _engine.ShowDebugInfo.ToString();
                            break;
                        default:
                            break;
                    }

                    return "> " + command + Environment.NewLine + response;
                }
            }
            return null;
        }



        /// <summary>
        /// Parse a command string to see if it matches "true/false,0/1,on/off".
        /// Will return 1 if true, 0 if false, and -1 if neither
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private int ParseArgumentBoolean(string command)
        {
            if (!String.IsNullOrEmpty(command))
            {
                command = command.ToUpper();
                if (command == "1" || command == "TRUE" || command == "ON")
                    return 1;
                else if (command == "0" || command == "FALSE" || command == "OFF")
                    return 0;
            }
            return -1;
        }
    }
}
