﻿

using System.Collections;
using System.Reflection;
using Logic.ServerStartAction;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}
#endif

namespace Game.Logic
{
    public class Program 
    {
        private static ArrayList m_actions = new ArrayList();
        
        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "GAME.MAIN";

            RegisterActions();

            if (args.Length == 0)
            {
                args = new string[] { "--start", };
                //args = new string[] { "--makepackethandle", };
            }

            string actionName;
            Hashtable parameters;
            try
            {
                ParseParameters(args, out actionName, out parameters);
            }
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            IAction action = GetAction(actionName);
            if (action != null)
            {
                action.OnAction(parameters);
            }
            else
            {
                ShowSyntax();
            }
        }

        /// <summary>
        /// Registers a possible action
        /// </summary>
        /// <param name="action">The action to register</param>
        private static void RegisterAction(IAction action)
        {
            if(action == null)
                throw new ArgumentException("Action can't be null!","action");

            m_actions.Add(action);
        }

        /// <summary>
        /// Displays the syntax for this exectuable
        /// </summary>
        private static void ShowSyntax()
        {
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            string exeFileName = Path.GetFileName(exeFilePath);
            
            Console.WriteLine($"Syntax: {exeFileName} {{action}} [param1=value1] [param2=value2] ...");
            Console.WriteLine("Possible actions:");
            foreach (IAction action in m_actions)
            {
                if (action.Syntax != null && action.Description != null)
                    Console.WriteLine(String.Format("  {0,-20}\t{1}", action.Syntax, action.Description));
            }
        }

        /// <summary>
        /// Searches and returns an action based on the name
        /// </summary>
        /// <param name="name">the action name</param>
        /// <returns>the action class</returns>
        private static IAction GetAction(string name)
        {
            foreach (IAction action in m_actions)
            {
                if (action.Name.Equals(name))
                    return action;
            }
            return null;
        }        
        
        /// <summary>
        /// Parses the commandline parameters
        /// </summary>
        /// <param name="args">The commandline arguments</param>
        /// <param name="actionName">The action name from the commandline</param>
        /// <param name="parameters">A hashtable with all parameters and their values</param>
        private static void ParseParameters(string[] args, out string actionName, out Hashtable parameters)
        {
            parameters = new Hashtable();
            actionName = null;

            if(!args[0].StartsWith("--"))
                throw new ArgumentException("First argument must be the action");

            actionName = args[0];
            if(args.Length == 1) return;

            for(int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
                if(arg.StartsWith("--"))
                {
                    throw new ArgumentException("At least two actions given and only one action allowed!");				
                }
                else if(arg.StartsWith("-"))
                {
                    int valueIdx = arg.IndexOf('=');
                    if(valueIdx==-1)
                    {
                        parameters.Add(arg, "");
                        continue;
                    }
                    string argName = arg.Substring(0,valueIdx);
					
                    string argValue = "";
                    if(valueIdx+1 < arg.Length)
                        argValue = arg.Substring(valueIdx+1);

                    parameters.Add(argName, argValue);
                }
            }
        } 
        /// <summary>
        /// Registers all possible actions
        /// </summary>
        /// <remarks>
        /// The order in which the actions are registered
        /// is the same order in which they are displayed
        /// on syntax output
        /// </remarks>
        private static void RegisterActions()
        {
            //Start server in console mode
            RegisterAction(new ConsoleStart());
            RegisterAction(new ConsoleMakeClientPacketHandle());
        }        
        
    }
}