using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;

using Hvt.VrrGtfsTransformer.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Hvt.VrrGtfsTransformer.Transformations
{
    /// <summary>
    /// General:
    /// Gets all stops by grouping the first 3 segments (divided by ':').
    /// Then filters those with more than 1 stop. 
    /// 
    /// What qualifies as valid group: 
    /// At least one stop id ending with "_Parent" and all others having parent_station assigned.
    /// 
    /// Actions taken:
    /// Sets station type to 0 for the stops and assigns the id of the created parent station.
    /// Sets station type to 1 for the created station. The coordinates are the center coordinates of the children. The id will be constructed from the group with the suffix "_Parent".
    /// </summary>
    public class ParentStationTransformationStep : ITransformationStep
    {
        private const string ParentSuffix = "_Parent";

        public string Name { get => "Parent station"; }

        public GTFSFeed Execute(GTFSFeed feed)
        {
            // First collect all groups of stations
            var grouped = VrrUtils.GroupStops(feed);

            // Then extract those with more than one station and where there is no stop with the suffix "_Parent" as Id
            var needingParent = this.ExtractWithoutParents(grouped);

            // Finally add them and modify the stops
            this.AddParentStops(feed, needingParent);

            return feed;
        }

        private void AddParentStops(GTFSFeed feed, Dictionary<string, List<Stop>> needingParent)
        {
            Program.Write(this.Name + ": Adding parents to groups", ConsoleColor.Gray);

            var stopsAdded = 0;
            var stopsModified = 0;

            foreach (var group in needingParent)
            {
                //Debug.WriteLine("Group: " + group.Key);
                var parentStop = this.InterpolateParentStop(group);

                foreach (var stop in group.Value)
                {
                    stop.ParentStation = parentStop.Id;
                    stop.LocationType = LocationType.Stop;

                    stopsModified++;
                }

                feed.Stops.Add(parentStop);
                stopsAdded++;
            }

            Program.Write(this.Name + ": Parents added (" + stopsAdded + " new and " + stopsModified + " modified)", ConsoleColor.Gray);
        }

        private Dictionary<string, List<Stop>> ExtractWithoutParents(Dictionary<string, List<Stop>> grouped)
        {
            Program.Write(this.Name + ": Extracting groups missing a parent", ConsoleColor.Gray);

            var result = new Dictionary<string, List<Stop>>();
            var stopCount = 0;
            var skippedCount = 0;

            foreach (var group in grouped)
            {
                if (group.Value.Count <= 1)
                {
                    // Skip those with only one or no (however this happened) stop
                    Program.WriteDebug(this.Name + ": Group " + group.Key + " does not need a parent station because it is only one station");
                    skippedCount++;
                    continue;
                }

                if (group.Value.Any(s => s.LocationType == LocationType.Station))
                {
                    // Skip those where there is already a parent station which is correctly assigned
                    Program.WriteDebug(this.Name + ": Group " + group.Key + " already has a parent station");
                    skippedCount++;
                    continue;
                }

                result.Add(group.Key, group.Value);

                stopCount += group.Value.Count;
            }

            Program.Write(this.Name + ": Extracted " + result.Count + " groups (Total of " + stopCount + " stops and skipped " + skippedCount + ")", ConsoleColor.Gray);
            return result;
        }

        private Stop InterpolateParentStop(KeyValuePair<string, List<Stop>> group)
        {
            var parentId = group.Key + ParentSuffix;
            var parentCoordinates = CoordinateUtils.GetCentralGeoCoordinate(group.Value.Select(s => (s.Latitude, s.Longitude)).ToList());

            return new Stop()
            {
                Id = parentId,
                Name = group.Value[0].Name,
                LocationType = LocationType.Station,
                Latitude = parentCoordinates.lat,
                Longitude = parentCoordinates.lon
            };
        }
    }
}
