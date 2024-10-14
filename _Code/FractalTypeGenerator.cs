using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps
{
    public class FractalTypeGenerator : IMapGenerator
    {
        public FractalTypeGenerator()
        {
        }

        public void ClearAllMyDataForQuitToMainMenuOrBeforeNewMap()
        {

        }

        public void GenerateMapStructureOnly(Galaxy galaxy, ArcenHostOnlySimContext context, MapConfiguration mapConfig, MapTypeData mapType)
        {
            FakeGalaxy g = MakeRepTile(context.RandomToUse);
            g.Populate(galaxy, PlanetType.Normal, context.RandomToUse);
        }

        public FakeGalaxy MakeRepTile(RandomGenerator rng)
        {
            FakeGalaxy g;
            do
            {
                g = new FakeGalaxy();
                for (int i = 0; i < 5; ++i)
                {
                    g.AddPlanetAt(ArcenPoint.Create(rng.Next(-1000, 1000), rng.Next(-1000, 1000)));
                }

                for (int i = 0; i < 5; ++i)
                    for (int j = i + 1; j < 5; ++j)
                        if (rng.Next(0, 999) < 382)
                        {
                            g.AddLink(g.planets[i], g.planets[j]);
                        }
            } while (!g.IsConnected());
            return g;
        }
    }
}