using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;


namespace Battleship
{
    public class BattleshipLoader
    {
        public static BattleshipAgent LoadSingleAgent(string dllName)
        {
            string battleshipAgentBaseClassName = "Battleship.BattleshipAgent";
            Assembly agentAssembly = Assembly.LoadFile(dllName);

            foreach (Type type in agentAssembly.ExportedTypes)
            {
                if (type.BaseType.FullName == battleshipAgentBaseClassName)
                {
                    BattleshipAgent agent = Activator.CreateInstance(type, null) as BattleshipAgent;
                    return agent;
                }
            }
            string msg = $"Module {dllName} does not implement {battleshipAgentBaseClassName}";
            throw new NotSupportedException(msg);
        }

        public static List<BattleshipAgent> LoadAllAgents(string pluginDirectory = null)
        {
            List<BattleshipAgent> battleshipAgents = new List<BattleshipAgent>();

            foreach (string module in BattleshipLoader.GetModuleList(pluginDirectory))
            {
                try
                {
                    var agent = LoadSingleAgent(module);
                    battleshipAgents.Add(agent);
                }
                catch (NotSupportedException)
                {
                    continue;
                }
            }

            return battleshipAgents;
        }

        private static bool IsClassSpecificMethod(string methodName)
        {
            string[] baseObjMethods = { "Equals", "GetType", "GetHashCode", "ToString" };
            foreach (string method in baseObjMethods)
            {
                if (string.Compare(methodName, method, true) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string[] GetPublicMethodPrototypeList(Type type)
        {
            char[] parameterSeparators = new char[] { ',', ' ' };
            MethodInfo[] fullMethodList = type.GetMethods();
            string[] prototypeList = new string[fullMethodList.Length];

            int n = 0;
            foreach (MethodInfo method in fullMethodList)
            {
                if (method.IsPublic && IsClassSpecificMethod(method.Name))
                {
                    string prototype = $"public ";

                    // return type
                    if (method.ReturnType.GenericTypeArguments.Length > 1)
                    {
                        prototype += "(";
                        foreach (Type t in method.ReturnType.GenericTypeArguments)
                        {
                            prototype += $"{t.Name}, ";
                        }
                        prototype = prototype.TrimEnd(parameterSeparators) + ") ";
                    }
                    else
                    {
                        prototype += $"{method.ReturnType.Name} ";
                    }

                    // name of the method
                    prototype += $"{method.Name}";

                    // parameters
                    prototype += "(";
                    foreach (ParameterInfo parm in method.GetParameters())
                    {
                        prototype += $"{parm.ParameterType.Name} {parm.Name}, ";
                    }
                    prototype = prototype.TrimEnd(parameterSeparators) + ")";

                    prototypeList[n++] = prototype;
                }
            }

            return prototypeList;
        }

        public static string[] GetModuleList(string pluginDirectory = null)
        {
            List<string> moduleList = new List<string>();

            if (!string.IsNullOrWhiteSpace(pluginDirectory))
            {
                if (!Directory.Exists(pluginDirectory))
                {
                    Console.WriteLine($"Error: Directory {pluginDirectory} does not exist");
                    return new string[0];
                }

                // restrict plugin search to specified directory
                moduleList.AddRange(Directory.EnumerateFiles(pluginDirectory, "*.dll"));
            }
            else
            {
                // no plugin directory specified, try the current directory
                string pwd = Directory.GetCurrentDirectory();
                moduleList.AddRange(Directory.EnumerateFiles(pwd, "*.dll"));

                // no plugin directory specified, maybe we are a MSVC project
                string csharpProjectPath = pwd + @"\..\..\..\";
                foreach (string dir in Directory.EnumerateDirectories(csharpProjectPath))
                {
                    string csharpPath = @"\bin\debug\netstandard2.0";
                    string pluginPath = Path.GetFullPath(dir + csharpPath);
                    if (Directory.Exists(pluginPath))
                    {
                        moduleList.AddRange(Directory.EnumerateFiles(pluginPath, "*.dll"));
                    }
                }
            }

            return moduleList.ToArray();
        }
    }
}
