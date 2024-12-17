using System.Collections.Concurrent;
using System.Reflection;
using BeetleX;
using BeetleX.Clients;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Client.Network;

public class ClientPacketMethodsManager
{
    private readonly ConcurrentDictionary<ClientPackets, (MethodInfo, object obj)> sendPacketMethods = new();
    
    public void Register()
    {
        Assembly assembly = Assembly.LoadFrom("Flatbuffers.Messages.dll");
        //Assembly assembly = Assembly.GetExecutingAssembly();
        sendPacketRegister(assembly);
    }

    // 전송 패킷 등록
    private void sendPacketRegister(Assembly assembly)
    {
        var PacketNames = Enum.GetNames(typeof(ClientPackets)).ToHashSet();

        // "NetworkMessage" 네임스페이스 안의 모든 클래스 검색
        var fbsClasses = assembly.GetTypes()
            .Where(t => t.Namespace == "NetworkMessage"
                        && t.Name.EndsWith("_FBS")
                        && PacketNames.Contains(t.Name.Substring(0, t.Name.Length - 4)))
            .Select(t => t.Name.Substring(0, t.Name.Length - 4))
            .ToList();

        fbsClasses.ForEach(name =>
        {
            Type? type = assembly.GetType($"NetworkMessage.{name}");
            if (type != null)
            {
                var packMethod = type.GetMethod("Pack", BindingFlags.Public | BindingFlags.Static);
                if (packMethod != null)
                {
                    object? obj = Activator.CreateInstance(type);
                    if (obj != null)
                    {
                        ClientPackets id = (ClientPackets)Enum.Parse(typeof(ClientPackets), name);
                        sendPacketMethods[id] = (packMethod, obj);
                    }
                }
            }
        });            
    }
        
    //----------------------------------------------------------------------------------------------------------
    public (MethodInfo method, object obj) GetClientPacketType<T>(ClientPackets id, T packet)
    {
        if (sendPacketMethods.TryGetValue(id, out var packFunc))
        {
            return packFunc;
        }

        throw new ArgumentException($"지원되지 않는 메시지 타입입니다: {typeof(T).Name}");
    }    
}