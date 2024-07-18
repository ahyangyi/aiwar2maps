using Arcen.AIW2.Core;
using Arcen.Universal;


namespace AhyangyiMaps
{
    public class FakePlanet
    {
        public ArcenPoint location;
        public System.Collections.Generic.List<FakePlanet> links;

        public FakePlanet(ArcenPoint location)
        {
            this.location = location;
            links = new System.Collections.Generic.List<FakePlanet>();
        }

        public void AddLinkTo(FakePlanet other)
        {
            links.Add(other);
            other.links.Add(this);
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            int dx, dy;
            do
            {
                dx = rng.Next(-1000, 1000);
                dy = rng.Next(-1000, 1000);
            } while (dx * dx + dy * dy > 1000 * 1000);

            location.X += planetType.GetData().InterStellarRadius * wobble * dx * 3 / 100000;
            location.Y += planetType.GetData().InterStellarRadius * wobble * dy * 3 / 100000;
        }
    }

    public class FakeGalaxy
    {
        public System.Collections.Generic.List<FakePlanet> planets;

        public FakeGalaxy()
        {
            planets = new System.Collections.Generic.List<FakePlanet>();
        }

        public FakePlanet AddPlanetAt(ArcenPoint location)
        {
            FakePlanet ret = new FakePlanet(location);
            planets.Add(ret);
            return ret;
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            foreach (FakePlanet planet in planets)
            {
                planet.Wobble(planetType, wobble, rng);
            }
        }

        public void Populate(Galaxy galaxy, PlanetType planetType, RandomGenerator rng)
        {
            System.Collections.Generic.Dictionary<FakePlanet, Planet> dict = new System.Collections.Generic.Dictionary<FakePlanet, Planet>();
            foreach (FakePlanet planet in planets)
            {
                dict[planet] = galaxy.AddPlanet(planetType, planet.location,
                        World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(rng, PlanetPopulationType.None));
            }
            foreach (FakePlanet planet in planets)
            {
                foreach (FakePlanet neighbor in planet.links)
                {
                    dict[planet].AddLinkTo(dict[neighbor]);
                }
            }
        }
    }
}