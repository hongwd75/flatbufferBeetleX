using System.Collections;
using System.Data;
using System.Reflection;
using Flatbuffers.Server;
using Server.Config;

namespace Logic.ServerStartAction;

public class ConsoleStart : IAction
{
    public string Name { get => "--start"; }
    public string Syntax { get=>"--start [-config=./config/serverconfig.xml]"; }
    public string Description { get => "* 콘솔 모드에서 게임서버 시작하기"; }

    private bool StartServer()
    {
        GameServer.Instance.Start();
        return true;
    }
    
    public void OnAction(Hashtable parameters)
    {
        Console.WriteLine("# 게임서버 시작 .... 설정파일 준비");
        FileInfo configFile;
        FileInfo currentAssembly = null;
        if (parameters["-config"] != null)
        {
            Console.WriteLine("   - 설정된 config 파일: " + parameters["-config"]);
            configFile = new FileInfo((String)parameters["-config"]);
        }
        else
        {
            currentAssembly = new FileInfo(Assembly.GetEntryAssembly().Location);
            configFile = new FileInfo(currentAssembly.DirectoryName + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml");
        }
        
        var config = new BaseServerConfiguration();
        if (configFile.Exists)
        {
            config.LoadFromXMLFile(configFile);
            Console.WriteLine($"   - 설정 DB타입 : {config.DBType}");
        }
        else
        {
            if (!configFile.Directory.Exists)
            {
                configFile.Directory.Create();
            }
            
            config.SaveToXMLFile(configFile);
            if (File.Exists(currentAssembly.DirectoryName + Path.DirectorySeparatorChar + "Flatbuffers.Server.exe"))
            {
                Console.WriteLine($"   - 설정파일이 존재하지 않아서 새로 생성함. 설정 DB타입 : {config.DBType}");
            }
        }
        
        GameServer.CreateInstance(config);
        StartServer();
        
        bool run = true;
        while (run)
        {
            Console.Write("> ");
            string line = Console.ReadLine();

            switch (line.ToLower())
            {
                case "exit":
                    run = false;
                    break;
                case "stacktrace":
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    break;
            }
        }

        if (GameServer.Instance != null)
        {
            GameServer.Instance.Stop();
        }
    }
}