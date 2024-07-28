using Arcen.AIW2.Core;
using Arcen.Universal;
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

        public static Matrix2x2 Rotation(FInt xx, FInt xy)
        {
            return new Matrix2x2(xx, xy, -xy, xx);
        }

        public (FInt, FInt) Apply(FInt x, FInt y)
        {
            return (this.xx * x + this.yx * y, this.xy * x + this.yy * y);
        }

        public (int, int) Apply(int x, int y)
        {
            return ((this.xx * x + this.yx * y).GetNearestIntPreferringLower(), (this.xy * x + this.yy * y).GetNearestIntPreferringLower());
        }

        public ArcenPoint Apply(ArcenPoint reference, int x, int y)
        {
            (x, y) = Apply(x, y);
            return ArcenPoint.Create(reference.X + x, reference.Y + y);
        }

        public static Matrix2x2 Identity = Matrix2x2.Rotation(FInt.One, FInt.Zero);
        public static Matrix2x2 Zero = Matrix2x2.Rotation(FInt.Zero, FInt.Zero);
        public static Matrix2x2 FlipX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 FlipY = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 ProjectToX = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToNegX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 ProjectToNegY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 Rotation2 = Matrix2x2.Rotation((FInt)(-1), FInt.Zero);
        public static Matrix2x2 Rotation3_1 = Matrix2x2.Rotation(FInt.Create(-500, false), FInt.Create(866, false));
        public static Matrix2x2 Rotation3_2 = Matrix2x2.Rotation(FInt.Create(-500, false), FInt.Create(-866, false));
        public static Matrix2x2[] Rotation3 = { Identity, Rotation3_1, Rotation3_2 };
        public static Matrix2x2 Rotation4_1 = Matrix2x2.Rotation(FInt.Zero, FInt.One);
        public static Matrix2x2 Rotation4_3 = Matrix2x2.Rotation(FInt.Zero, (FInt)(-1));
        public static Matrix2x2[] Rotation4 = { Identity, Rotation4_1, Rotation2, Rotation4_3 };
        public static Matrix2x2 Rotation5_1 = Matrix2x2.Rotation(FInt.Create(309, false), FInt.Create(951, false));
        public static Matrix2x2 Rotation5_2 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(588, false));
        public static Matrix2x2 Rotation5_3 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(-588, false));
        public static Matrix2x2 Rotation5_4 = Matrix2x2.Rotation(FInt.Create(309, false), FInt.Create(-951, false));
        public static Matrix2x2[] Rotation5 = { Identity, Rotation5_1, Rotation5_2, Rotation5_3, Rotation5_4 };
        public static Matrix2x2 Rotation6_1 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(866, false));
        public static Matrix2x2 Rotation6_5 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(-866, false));
        public static Matrix2x2[] Rotation6 = { Identity, Rotation6_1, Rotation3_1, Rotation2, Rotation3_2, Rotation6_5 };
        public static Matrix2x2[][] Rotations = { null, null, null, Rotation3, Rotation4, Rotation5, Rotation6 };
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

        public void RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy()
        {
            foreach (FakePlanet other in links)
                other.links.Remove(this);
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
            planet.RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy();
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
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, flipX, rot, flipY }, 2, 2));
                    }
                    else if (planet.location.Y * 2 == maxY)
                    {
                        FakePlanet flipX = locationIndex[ArcenPoint.Create(maxX - planet.location.X, planet.location.Y)];
                        planet.wobbleMatrix = Matrix2x2.ProjectToX;
                        flipX.wobbleMatrix = Matrix2x2.ProjectToNegX;
                        symmetricGroups.Add(new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, flipX }, 1, 2));
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
        public void MakeRotationalGeneric(int cx, int cy, int d, int n)
        {
            var center = ArcenPoint.Create(cx, cy);
            var planetsToRemove = new System.Collections.Generic.List<FakePlanet>();
            var newSymmetricGroups = new System.Collections.Generic.List<SymmetricGroup>();
            var planetsBackup = new System.Collections.Generic.List<FakePlanet>(planets);
            var groupLookup = new System.Collections.Generic.Dictionary<FakePlanet, SymmetricGroup>();
            var locationIndex = MakeLocationIndex();

            var intersectorConnection = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeRightToLeft = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeLeftToRight = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();

            var rotations = Matrix2x2.Rotations[n];
            FInt[] slopes = { FInt.Zero, FInt.Zero, FInt.Zero, FInt.Create(1732, false), FInt.Create(1000, false), FInt.Create(727, false), FInt.Create(577, false) };
            bool hasCenter = false;

            foreach (FakePlanet planet in planetsBackup)
            {
                int xdiff = planet.location.X - cx;
                int ydiff = planet.location.Y - cy;

                if (ydiff > 0)
                {
                    planetsToRemove.Add(planet);
                    continue;
                }

                if (xdiff >= -ydiff * slopes[n] + d / 2 || xdiff <= ydiff * slopes[n] - d / 2)
                {
                    // planet way out of the sector, removing

                    planetsToRemove.Add(planet);
                    continue;
                }

                var symPoint = ArcenPoint.Create(cx * 2 - planet.location.X, planet.location.Y);
                if ((xdiff >= -ydiff * slopes[n] - d / 2 || xdiff <= ydiff * slopes[n] + d / 2) && locationIndex.ContainsKey(symPoint))
                {
                    if (xdiff == 0)
                    {
                        // special case, no merge
                        if (ydiff == 0)
                        {
                            // planet at center of symmetry, needs special handling
                            hasCenter = true;
                            continue;
                        }
                    }
                    else
                    {
                        // planet on borderline, adjusting to the border
                        if (xdiff < 0)
                        {
                            // planet on the left side
                            var other = locationIndex[symPoint];
                            mergeLeftToRight[planet] = other;
                            mergeRightToLeft[other] = planet;
                            planet.location.X = (cx + ydiff * slopes[n]).GetNearestIntPreferringLower();
                            xdiff = planet.location.X - cx;
                        }
                        else
                        {
                            // planet on the right side
                            planetsToRemove.Add(planet);
                            continue;
                        }
                    }
                }
                else if (xdiff >= -ydiff * slopes[n] - d && locationIndex.ContainsKey(symPoint))
                {
                    var other = locationIndex[symPoint];
                    intersectorConnection[planet] = other;
                }

                var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, n, 1);
                for (int j = 1; j < n; ++j)
                {
                    var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                    rot.wobbleMatrix = rotations[j];
                    group.planets.Add(rot);
                }
                newSymmetricGroups.Add(group);
                groupLookup[planet] = group;
            }
            foreach (FakePlanet planet in planetsToRemove)
                RemovePlanetButDoesNotUpdateSymmetricGroups(planet);
            symmetricGroups = newSymmetricGroups;
            foreach (var group in symmetricGroups)
            {
                var planet = group.planets[0];
                foreach (var neighbor in planet.links)
                {
                    if (groupLookup.ContainsKey(neighbor))
                    {
                        var neighborGroup = groupLookup[neighbor];
                        for (int i = 1; i < n; ++i)
                        {
                            group.planets[i].AddLinkTo(neighborGroup.planets[i]);
                        }
                    }
                }

                if (intersectorConnection.ContainsKey(planet))
                {
                    var otherGroup = groupLookup[intersectorConnection[planet]];
                    for (int i = 0; i < n; ++i)
                    {
                        group.planets[i].AddLinkTo(otherGroup.planets[(i + 1) % n]);
                    }
                }
            }
            foreach (var group in symmetricGroups)
            {
                var planet = group.planets[0];
                if (mergeLeftToRight.ContainsKey(planet))
                {
                    var rightPlanet = mergeLeftToRight[planet];
                    foreach (var neighbor in rightPlanet.links)
                    {
                        if (!groupLookup.ContainsKey(neighbor))
                        {
                            continue;
                        }
                        var otherGroup = groupLookup[neighbor];
                        for (int i = 0; i < n; ++i)
                        {
                            group.planets[i].AddLinkTo(otherGroup.planets[(i + n - 1) % n]);
                        }
                    }
                }
            }

            if (hasCenter)
            {
                var centerPlanet = locationIndex[center];
                var neighbors = centerPlanet.links.ToList();

                centerPlanet.wobbleMatrix = Matrix2x2.Zero;
                var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { centerPlanet }, 1, 1);
                newSymmetricGroups.Add(group);

                foreach (var neighbor in neighbors)
                {
                    var neighborGroup = groupLookup[neighbor];
                    for (int i = 0; i < n; ++i)
                    {
                        centerPlanet.AddLinkTo(neighborGroup.planets[i]);
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