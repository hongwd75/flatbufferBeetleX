using System.Diagnostics;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace Game.Logic.Utils
{
    public class Util
    {
        public static string GetFormattedStackTraceFrom(Thread targetThread)
        {
            var sb = new StringBuilder();
            try
            {
                var dt = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, false);
                var rt = dt.ClrVersions.Single().CreateRuntime();
                ClrThread clrThread = null;
                foreach (var t in rt.Threads)
                {
                    if (t.ManagedThreadId == targetThread.ManagedThreadId)
                    {
                        clrThread = t;
                        break;
                    }
                }
                foreach (var frame in clrThread.EnumerateStackTrace())
                {
                    var method = frame.Method;
                    if (method != null)
                    {
                        sb.AppendLine($"   at {method.Signature}");
                    }
                }
            }
            catch (Exception e)
            {
                return e.StackTrace;
            }
            return sb.ToString();
        }
        
        //-------------------------------------------------------------------------------------------------------------
        const char primarySeparator = ';';
        const char secondarySeparator = '-';        
        public static List<string> SplitCSV (string str, bool rangeCheck = false)
        {
			
            if (str==null) return null;
			
            // simple parsing on priSep
            var resultat = str.Split(new char[]{primarySeparator}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!rangeCheck)
                return resultat;
			
            // advanced parsing with range handling
            List<string> advancedResultat = new List<string>();
            foreach(var currentResultat in resultat)
            {
                if (currentResultat.Contains('-'))
                {
                    int from =0;
                    int to =0;
					
                    if (int.TryParse(currentResultat.Split(secondarySeparator)[0], out from) && int.TryParse(currentResultat.Split(secondarySeparator)[1], out to))
                    {
                        if (from > to)
                        {
                            int tmp = to;
                            to = from;
                            from = tmp;
                        }
						
                        for (int i=from; i<=to; i++)
                            advancedResultat.Add(i.ToString());
                    }
                }
                else
                    advancedResultat.Add(currentResultat);
            }
            return advancedResultat;
        }
        
        //-------------------------------------------------------------------------------------------------------------
        public static void Shuffle<T>(IList<T> list)  
        {  
            int n = list.Count; 
            while (n > 1)
            {
                n--;
                int k = RandomUtil.Int(n);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }        
    }
}