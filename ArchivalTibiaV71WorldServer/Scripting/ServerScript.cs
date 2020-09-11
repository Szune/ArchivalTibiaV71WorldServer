using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ArchivalTibiaV71WorldServer.Scripting
{
    public class ServerScript
    {
        public static readonly ServerScript LoadScript = new ServerScript("LoadScript.cs");
        private readonly string _name;
        private Script<object> _compiled;

        public ServerScript(string name)
        {
            _name = $"scripts/{name}";
            Reload();
        }

        public void Reload()
        {
            try
            {
                var code = File.ReadAllText(_name);
                _compiled = CSharpScript.Create(code, 
                    ScriptOptions
                        .Default
                        .AddReferences("ArchivalTibiaV71WorldServer", "ArchivalTibiaV71WorldServer.Constants")
                        .AddImports("System", "ArchivalTibiaV71WorldServer", "ArchivalTibiaV71WorldServer.Constants", "ArchivalTibiaV71WorldServer.Scripting", "System.Linq", "System.Text"),
                    typeof(ScriptGlobals));
                Console.WriteLine($" > Loaded script {_name}");
            }
            catch
            {
                Console.WriteLine($"Failed to load {_name}");
                _compiled = null;
            }
        }

        public void Execute(ScriptGlobals scriptGlobals)
        {
            if (_compiled == null)
            {
                Console.WriteLine($"Tried to execute script that failed to load: {_name}");
                return;
            }

            try
            {
                _compiled.RunAsync(scriptGlobals);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception when running script {_name}:\n{ex}");
            }
        }
        
    }
}