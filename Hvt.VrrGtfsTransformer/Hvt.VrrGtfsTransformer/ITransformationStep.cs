using GTFS;

namespace Hvt.VrrGtfsTransformer
{
    public interface ITransformationStep
    {
        GTFSFeed Execute(GTFSFeed feed);

        string Name { get; }
    }
}
