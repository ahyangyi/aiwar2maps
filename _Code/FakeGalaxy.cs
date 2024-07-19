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

        public void RemoveLinkTo(FakePlanet other)
        {
            links.Remove(other);
            other.links.Remove(this);
        }
        public void RemoveAllLinks()
        {
            foreach (FakePlanet other in links)
                other.links.Remove(this);
            this.links.Clear();
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

        public void RemovePlanet(FakePlanet planet)
        {
            planet.RemoveAllLinks();
            planets.Remove(planet);
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            foreach (FakePlanet planet in planets)
            {
                planet.Wobble(planetType, wobble, rng);
            }
        }
        public bool IsConnected()
        {
            if (planets.Count == 0)
            {
                return true;
            }

            System.Collections.Generic.HashSet<FakePlanet> visited = new System.Collections.Generic.HashSet<FakePlanet>();
            System.Collections.Generic.List<FakePlanet> queue = new System.Collections.Generic.List<FakePlanet>();
            visited.Add(planets[0]);
            queue.Add(planets[0]);

            for (int i = 0; i < queue.Count; ++i)
            {
                FakePlanet cur = queue[i];
                foreach (FakePlanet neighbor in cur.links)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Add(neighbor);
                    }
                }
            }

            return queue.Count == planets.Count;
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