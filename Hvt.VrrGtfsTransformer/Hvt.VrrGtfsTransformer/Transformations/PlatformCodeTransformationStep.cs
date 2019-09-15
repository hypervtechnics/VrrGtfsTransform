using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;

using Hvt.VrrGtfsTransformer.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hvt.VrrGtfsTransformer.Transformations
{
    public class PlatformCodeTransformationStep : ITransformationStep
    {
        /// <summary>
        /// Contains the terms to remove from existing platform codes.
        /// Remember: When terms contain another term they have to be in front of it in the array.
        /// Example: (Gleis) and Gleis. So the terms get replaced correctly one by one.
        /// </summary>
        private static readonly string[] PlatformTerms = new string[] { "Gl.", "Bussteig", "Steig", "(Gleis)", "U-Bahn Gleis", "Gleis" };

        public string Name { get => "Platform code"; }

        public GTFSFeed Execute(GTFSFeed feed)
        {
            var groups = VrrUtils.GroupStops(feed);
            List<(string group, Stop stop)> stops = groups.SelectMany(kv => kv.Value.Select(s => (kv.Key, s))).ToList();

            var stopsModified = 0;

            foreach (var group in stops)
            {
                var should = this.GetPlatformCode(stops, group.group, group.stop);

                if ((group.stop.PlatformCode ?? string.Empty).CompareTo(should) != 0) // ?? because sometimes PlatformCode is null
                {
                    group.stop.PlatformCode = should;
                    stopsModified++;
                }
            }

            Program.Write(this.Name + ": Platform code updated (" + stopsModified + " modified of " + stops.Count + " processed)", ConsoleColor.Gray);
            return feed;
        }

        private string GetPlatformCode(List<(string group, Stop stop)> groups, string group, Stop stop)
        {
            // Preserve already existing platform codes and only remove the unnecessary terms
            if (!string.IsNullOrEmpty(stop.PlatformCode))
            {
                string newPlatformCode = stop.PlatformCode;

                foreach (var toReplace in PlatformTerms)
                {
                    newPlatformCode = newPlatformCode.Replace(toReplace, string.Empty);
                }

                newPlatformCode = newPlatformCode.Trim();

                Program.WriteDebug(this.Name + ": Changing existing platform code for stop " + stop.Id + ": " + stop.PlatformCode + " => " + newPlatformCode);
                return newPlatformCode;
            }

            if (stop.LocationType == LocationType.Station)
            {
                // Stations/Parents should not have a platform code
                return string.Empty;
            }
            else
            {
                // If there only one or two stops (station + one/two stops) do not modify the platform_code
                var stopsInGroup = groups.Where(kv => kv.group == group).ToList();

                if (stopsInGroup.Count <= 3 && stopsInGroup.Count(kv => kv.stop.LocationType == LocationType.Station) == 1)
                {
                    return stop.PlatformCode; // Although it should already be empty
                }

                // Now get the last segment
                var lastSegment = VrrUtils.GetIdLastSegment(stop.Id).Trim();

                if (!string.IsNullOrEmpty(lastSegment) && int.TryParse(lastSegment, out int platform))
                {
                    return platform.ToString(); // Remove leading zeros by temp casting to int
                }
                else
                {
                    return lastSegment;
                }
            }
        }
    }
}
