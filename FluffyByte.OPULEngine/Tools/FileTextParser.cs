using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyByte.OPULEngine.Tools;

public static class FileTextParser
{
    public static void ParseLines(IEnumerable<string> lines, Dictionary<string, Action<string>> handlers)
    {
        foreach(string rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith('#'))
                continue; // skip comments

            var parts = rawLine.Split('=', 2);
            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            if(handlers.TryGetValue(key, out var handler))
            {
                handler(value);
            }

        }
    }
}