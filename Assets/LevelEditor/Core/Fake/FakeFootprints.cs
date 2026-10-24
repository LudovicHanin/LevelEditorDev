using System.Collections.Generic;

namespace LevelEditor.Core
{
    public class FakeFootprints : IFootprintProvider
    {
        readonly Dictionary<string, Footprint> _map = new Dictionary<string, Footprint>();
        public void Register(string assetId, Footprint fp) => _map[assetId] = fp;
        public Footprint GetFootprint(string assetId) => _map.TryGetValue(assetId, out var fp) ? fp : null;
    }
}
