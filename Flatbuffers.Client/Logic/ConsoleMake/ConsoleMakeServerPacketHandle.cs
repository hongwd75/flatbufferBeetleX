using System.Reflection;
using System.Text;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Game.Client.Logic.ConsoleMake;

public class ConsoleMakeServerPacketHandle
{
    public static void make()
    {
        string exePath = AppContext.BaseDirectory;
        // 프로젝트 폴더 경로로 이동 (보통 bin\Debug\netX.X\에서 두 단계 위로 이동)
        string saveTo = $"{Directory.GetParent(exePath).Parent.Parent.Parent.FullName}\\Logic\\network\\ReceivePacket\\";
        
        // 패킷 리스트 획득
        List<ServerPackets> packetlist = Enum.GetValues(typeof(ServerPackets)).Cast<ServerPackets>()
            .Where(id => id.ToString() is var idString && idString.StartsWith("SC")).ToList();

        // 존재하는 패킷은 리스트에서 제거
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass == false) continue;

            if (type.GetInterface("Network.Protocol.IPacketMessage") == null) continue;

            var packetattribute =
                (ServerPacketMessageAttribute[])type.GetCustomAttributes(typeof(ServerPacketMessageAttribute), true);
            if (packetattribute.Length > 0)
            {
                packetlist.Remove(packetattribute[0].ID);
            }
        }
        
        //
        packetlist.ForEach(x =>
        {
            CreatePacketCSFile(x, saveTo);
        });        
    }

    protected static void CreatePacketCSFile(ServerPackets id, string path)
    {
        StringBuilder csfile = new StringBuilder();
        csfile.AppendLine($"// ** {id} 패킷 메시지");
        csfile.AppendLine("using System.Threading.Tasks;");
        csfile.AppendLine("using BeetleX;");
        csfile.AppendLine("using Google.FlatBuffers;");
        csfile.AppendLine("using Network.Protocol.IPacketMessage;");
        csfile.AppendLine("using NetworkMessage;");
        csfile.AppendLine("");
        csfile.AppendLine("namespace Network.Protocol");
        csfile.AppendLine("{");
        csfile.AppendLine($"\t[ServerPacketMessageAttribute(ServerPackets.{id})]");
        csfile.AppendLine($"\tpublic class {id}_handler : IClientPacketMessage");
        csfile.AppendLine("\t{");
        csfile.AppendLine("\t\t#pragma warning disable CS1998");
        csfile.AppendLine("\t\tpublic async Task Packet(ByteBuffer byteBuffer)");
        csfile.AppendLine("\t\t#pragma warning restore CS1998");
        csfile.AppendLine("\t\t{");
        csfile.AppendLine($"\t\t\t{id}_FBS packet = {id}.GetRootAs{id}(byteBuffer).UnPack();");
        csfile.AppendLine("\t\t\t//Todo 코드 작업 필요");
        csfile.AppendLine("\t\t}");
        csfile.AppendLine("\t}");
        csfile.AppendLine("}");

        string savefile = $"{path}{id}_handler.cs";
        StreamWriter writer = new StreamWriter(savefile, false, new UTF8Encoding(true));
        writer.Write(csfile.ToString());
        writer.Flush();
        writer.Close();        
    }
}