using GTFS;
using GTFS.Entities;

using System;
using System.Collections.Generic;

namespace Hvt.VrrGtfsTransformer.Utils
{
    public static class VrrUtils
    {
        public static string GetIdGroup(string id)
        {
            var idSegments = id.Split(":", StringSplitOptions.None);

            if (idSegments.Length < 3)
            {
                Program.WriteWarn("Found strange id " + id);
                return id;
            }

            var baseResult = string.Join(":", idSegments[0], idSegments[1], idSegments[2]);

            if (baseResult.EndsWith("_Parent"))
            {
                // Parent stations are added to it too
                // So remove the _Parent thing
                baseResult = baseResult.Substring(0, baseResult.Length - "_Parent".Length);
            }

            return baseResult;
        }

        public static string GetIdLastSegment(string id)
        {
            var idSegments = id.Split(":", StringSplitOptions.None);

            if (idSegments.Length < 5)
            {
                Program.WriteWarn("Found strange id " + id);
                return string.Empty;
            }

            return idSegments[4];
        }

        public static Dictionary<string, List<Stop>> GroupStops(GTFSFeed feed)
        {
            Program.Write("Getting stop groups", ConsoleColor.Gray);

            var result = new Dictionary<string, List<Stop>>();
            var stopCount = 0;

            foreach (var stop in feed.Stops)
            {
                if (string.IsNullOrEmpty(stop.Id))
                {
                    // Skip if there is no valid id
                    Program.WriteWarn("Stop with no id!");
                    continue;
                }

                var id = GetIdGroup(stop.Id);

                if (result.ContainsKey(id))
                {
                    result[id].Add(stop);
                }
                else
                {
                    result.Add(id, new List<Stop>() { stop });
                }

                stopCount++;
            }

            Program.Write("Found " + result.Count + " groups (Total: " + stopCount + " stops)", ConsoleColor.Gray);
            return result;
        }
    }
}
