using Arcen.AIW2.Core;
using Arcen.Universal;
using DiffLib;
using System.Collections.Generic;
using System.Linq;


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

        public (FInt, FInt) Apply(FInt x, FInt y)
        {
            return (this.xx * x + this.yx * y, this.xy * x + this.yy * y);
        }

        public static Matrix2x2 Identity = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 Zero = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 FlipX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 FlipY = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 ProjectToX = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToNegX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 ProjectToNegY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 Rotation2 = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 Rotation3_1 = new Matrix2x2(FInt.Create(-500, false), FInt.Create(866, false), FInt.Create(-866, false), FInt.Create(-500, false));
        public static Matrix2x2 Rotation3_2 = new Matrix2x2(FInt.Create(-500, false), FInt.Create(-866, false), FInt.Create(866, false), FInt.Create(-500, false));
    }
    public class FakePlanet
    {
        public ArcenPoint location;
        public System.Collections.Generic.List<FakePlanet> links;
        public Matrix2x2 wobbleMatrix = Matrix2x2.Identity;
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
        public void Wobble(int wobble, FInt dx, FInt dy)
        {
            (dx, dy) = wobbleMatrix.Apply(dx, dy);
            location.X += (dx * wobble).GetNearestIntPreferringLower();
            location.Y += (dy * wobble).GetNearestIntPreferringLower();
        }
    }

    public class SymmetricGroup
    {
        public System.Collections.Generic.List<FakePlanet> planets;
        public int rotational, reflectional;

        public SymmetricGroup(System.Collections.Generic.List<FakePlanet> planets, int rotational, int reflectional)
        {
            this.planets = planets;
            this.rotational = rotational;
            this.reflectional = reflectional;
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

            foreach (FakePlanet planet in planets)
            {
                planet.Wobble(wobble, dx2, dy2);
            }
        }
    }

    public class FakeGalaxy
    {
        public System.Collections.Generic.List<FakePlanet> planets;
        public System.Collections.Generic.List<SymmetricGroup> symmetricGroups;

        public FakeGalaxy()
        {
            planets = new System.Collections.Generic.List<FakePlanet>();
            symmetricGroups = new System.Collections.Generic.List<SymmetricGroup>();
        }

        public FakePlanet AddPlanetAt(ArcenPoint location)
        {
            FakePlanet planet = new FakePlanet(location);
            planets.Add(planet);
            symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, 1, 1));
            return planet;
        }

        protected void RemovePlanetButDoesNotUpdateSymmetricGroups(FakePlanet planet)
        {
            planet.RemoveAllLinks();
            planets.Remove(planet);
        }

        public void RemoveSymmetricGroup(SymmetricGroup symmetricGroup)
        {
            foreach (FakePlanet counterpart in symmetricGroup.planets)
            {
                RemovePlanetButDoesNotUpdateSymmetricGroups(counterpart);
            }
            symmetricGroups.Remove(symmetricGroup);
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            foreach (SymmetricGroup symmetricGroup in symmetricGroups)
            {
                symmetricGroup.Wobble(planetType, wobble, rng);
            }
        }
        public bool IsConnected()
        {
            if (planets.Count == 0)
            {
                return true;
            }

            var visited = new HashSet<FakePlanet>();
            var queue = new System.Collections.Generic.List<FakePlanet>();
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

            symmetricGroups.Clear();
            foreach (FakePlanet planet in planets)
            {
                if (planet.location.X * 2 < maxX)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.location.X, planet.location.Y)];
                    other.wobbleMatrix = Matrix2x2.FlipX;
                    symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, other }, 1, 2));
                }
                else if (planet.location.X * 2 == maxX)
                {
                    planet.wobbleMatrix = Matrix2x2.ProjectToY;
                    new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, 1, 1);
                }
            }
        }

        public void MakeRotational2()
        {
            int maxX = planets.Max(planet => planet.location.X);
            int maxY = planets.Max(planet => planet.location.Y);
            var locationIndex = MakeLocationIndex();

            symmetricGroups.Clear();
            foreach (FakePlanet planet in planets)
            {
                if (planet.location.X * 2 < maxX || planet.location.X * 2 == maxX && planet.location.Y * 2 < maxY)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.location.X, maxY - planet.location.Y)];
                    other.wobbleMatrix = Matrix2x2.Rotation2;
                    symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, other }, 2, 1));
                }
                else if (planet.location.X * 2 == maxX && planet.location.Y * 2 == maxY)
                {
                    planet.wobbleMatrix = Matrix2x2.Zero;
                    symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, 1, 1));
                }
            }
        }

        public void MakeRotational2Bilateral()
        {
            int maxX = planets.Max(planet => planet.location.X);
            int maxY = planets.Max(planet => planet.location.Y);
            var locationIndex = MakeLocationIndex();

            symmetricGroups.Clear();
            foreach (FakePlanet planet in planets)
            {
                if (planet.location.X * 2 < maxX)
                {
                    if (planet.location.Y * 2 < maxY)
                    {
                        FakePlanet rot = locationIndex[ArcenPoint.Create(maxX - planet.location.X, maxY - planet.location.Y)];
                        FakePlanet flipX = locationIndex[ArcenPoint.Create(maxX - planet.location.X, planet.location.Y)];
                        FakePlanet flipY = locationIndex[ArcenPoint.Create(planet.location.X, maxY - planet.location.Y)];
                        rot.wobbleMatrix = Matrix2x2.Rotation2;
                        flipX.wobbleMatrix = Matrix2x2.FlipX;
                        flipY.wobbleMatrix = Matrix2x2.FlipY;
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, flipX, rot, flipY}, 2, 2));
                    }
                    else if (planet.location.Y * 2 == maxY)
                    {
                        FakePlanet flipX = locationIndex[ArcenPoint.Create(maxX - planet.location.X, planet.location.Y)];
                        planet.wobbleMatrix = Matrix2x2.ProjectToX;
                        flipX.wobbleMatrix = Matrix2x2.ProjectToNegX;
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, flipX}, 1, 2));
                    }
                }
                else if (planet.location.X * 2 == maxX)
                {
                    if (planet.location.Y * 2 < maxY)
                    {
                        FakePlanet flipY = locationIndex[ArcenPoint.Create(planet.location.X, maxY - planet.location.Y)];
                        planet.wobbleMatrix = Matrix2x2.ProjectToY;
                        flipY.wobbleMatrix = Matrix2x2.ProjectToNegY;
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, flipY }, 1, 2));
                    }
                    else if (planet.location.Y * 2 == maxY)
                    {
                        planet.wobbleMatrix = Matrix2x2.Zero;
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, 1, 1));
                    }
                }
            }
        }

        // FIXME more general?
        public void MakeRotational3(int cx, int cy)
        {
            var planetsToRemove = new System.Collections.Generic.List<FakePlanet>();
            var newSymmetricGroups = new System.Collections.Generic.List<SymmetricGroup>();
            var planetsBackup = new System.Collections.Generic.List<FakePlanet>(planets);
            var rotationLookup = new System.Collections.Generic.Dictionary<(FakePlanet, int), FakePlanet>();

            foreach (FakePlanet planet in planetsBackup)
            {
                int xdiff = planet.location.X - cx;
                int ydiff = cy - planet.location.Y;
                if (xdiff > ydiff * 1.732 || xdiff <= -ydiff * 1.732)
                {
                    planetsToRemove.Add(planet);
                }
                else
                {
                    var rot1 = AddPlanetAt(ArcenPoint.Create((int)(cx + ydiff * 1.732 / 2 - xdiff * 0.5), (int)(cy + ydiff * 0.5 + xdiff * 1.732 / 2)));
                    var rot2 = AddPlanetAt(ArcenPoint.Create((int)(cx - ydiff * 1.732 / 2 - xdiff * 0.5), (int)(cy + ydiff * 0.5 - xdiff * 1.732 / 2)));
                    rot1.wobbleMatrix = Matrix2x2.Rotation3_1;
                    rot2.wobbleMatrix = Matrix2x2.Rotation3_2;
                    rotationLookup[(planet, 1)] = rot1;
                    rotationLookup[(planet, 2)] = rot2;
                    newSymmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet , rot1, rot2}, 3, 1));
                }
            }
            foreach (FakePlanet planet in planetsToRemove)
                RemovePlanetButDoesNotUpdateSymmetricGroups(planet);
            symmetricGroups = newSymmetricGroups;
            foreach (var group in symmetricGroups)
            {
                var planet = group.planets[0];
                foreach (var neighbor in planet.links)
                {
                    for (int i = 1; i < 3; ++i)
                    {
                        group.planets[i].AddLinkTo(rotationLookup[(neighbor, i)]);
                    }
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