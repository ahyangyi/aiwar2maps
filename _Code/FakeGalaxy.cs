using Arcen.AIW2.Core;
using Arcen.Universal;
using System;
using System.Linq;
using System.Numerics;


namespace AhyangyiMaps
{
    public struct Matrix2x2
    {
        public FInt xx, xy, yx, yy;

        public Matrix2x2(FInt xx, FInt xy, FInt yx, FInt yy)
        {
            this.xx = xx;
            this.xy = xy;
            this.yx = yx;
            this.yy = yy;
        }

        public (FInt, FInt) Apply (FInt x, FInt y)
        {
            return (this.xx * x + this.yx * y, this.xy * x + this.yy * y);
        }

        public static Matrix2x2 Identity = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 FlipX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 ProjectToY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 Rotation2 = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, (FInt)(-1));
    }

    public class FakePlanet
    {
        public ArcenPoint location;
        public System.Collections.Generic.List<FakePlanet> links;
        public System.Collections.Generic.List<FakePlanet> counterparts;
        public Matrix2x2 wobbleMatrix = Matrix2x2.Identity;

        public FakePlanet(ArcenPoint location)
        {
            this.location = location;
            links = new System.Collections.Generic.List<FakePlanet>();
            counterparts = new System.Collections.Generic.List<FakePlanet>();
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
        public void ApplyWobble(int wobble, FInt dx, FInt dy)
        {
            (dx, dy) = wobbleMatrix.Apply(dx, dy);
            location.X += (dx * wobble).GetNearestIntPreferringLower();
            location.Y += (dy * wobble).GetNearestIntPreferringLower();
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            int dx, dy;
            do
            {
                dx = rng.Next(-1000, 1000);
                dy = rng.Next(-1000, 1000);
            } while (dx * dx + dy * dy > 1000 * 1000);

            FInt dx2, dy2;
            dx2 = (FInt)(planetType.GetData().InterStellarRadius) * 3 * dx / 100000;
            dy2 = (FInt)(planetType.GetData().InterStellarRadius) * 3 * dy / 100000;

            ApplyWobble(wobble, dx2, dy2);
            foreach (FakePlanet planet in counterparts)
            {
                planet.ApplyWobble(wobble, dx2, dy2);
            }
        }
    }

    public class FakeGalaxy
    {
        public System.Collections.Generic.List<FakePlanet> planets;
        public System.Collections.Generic.List<FakePlanet> primaryPlanets;

        public FakeGalaxy()
        {
            planets = new System.Collections.Generic.List<FakePlanet>();
            primaryPlanets = new System.Collections.Generic.List<FakePlanet>();
        }

        public FakePlanet AddPlanetAt(ArcenPoint location, bool isPrimary=true)
        {
            FakePlanet ret = new FakePlanet(location);
            planets.Add(ret);
            if (isPrimary)
            {
                primaryPlanets.Add(ret);
            }
            return ret;
        }

        public void MarkSecondary(FakePlanet planet)
        {
            primaryPlanets.Remove(planet);
        }

        protected void RemovePlanet(FakePlanet planet)
        {
            planet.RemoveAllLinks();
            planets.Remove(planet);
            primaryPlanets.Remove(planet);
        }

        public void RemovePlanetAndCounterparts(FakePlanet planet)
        {
            foreach (FakePlanet counterpart in planet.counterparts)
            {
                RemovePlanet(counterpart);
            }
            RemovePlanet(planet);
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            foreach (FakePlanet planet in primaryPlanets)
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

        public System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet> MakeLocationIndex()
        {
            return planets.ToDictionary(planet => planet.location, planet => planet);
        }

        public void MakeBilateral()
        {
            int maxX = planets.Max(planet => planet.location.X);
            var locationIndex = MakeLocationIndex();

            foreach (FakePlanet planet in planets)
            {
                if (planet.location.X * 2 < maxX)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.location.X, planet.location.Y)];
                    other.wobbleMatrix = Matrix2x2.FlipX;
                    planet.counterparts.Add(other);
                    MarkSecondary(other);
                }
                else if (planet.location.X * 2 == maxX)
                {
                    planet.wobbleMatrix = Matrix2x2.ProjectToY;
                }
            }
        }

        public void MakeRotational2()
        {
            int maxX = planets.Max(planet => planet.location.X);
            int maxY = planets.Max(planet => planet.location.Y);
            var locationIndex = MakeLocationIndex();

            foreach (FakePlanet planet in planets)
            {
                if (planet.location.X * 2 < maxX || planet.location.X * 2 == maxX && planet.location.Y * 2 < maxY)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.location.X, maxY - planet.location.Y)];
                    other.wobbleMatrix = Matrix2x2.Rotation2;
                    planet.counterparts.Add(other);
                    MarkSecondary(other);
                }
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