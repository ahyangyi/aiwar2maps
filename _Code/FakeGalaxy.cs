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

        public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
        {
            return new Matrix2x2(a.xx * b.xx + a.xy * b.yx, a.xx * b.xy + a.xy * b.yy, a.yx * b.xx + a.yy * b.yx, a.yx * b.xy + a.yy * b.yy);
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

        public static Matrix2x2[] Rotation3ReflectLeft = { ProjectToY * Rotation3_1, ProjectToY * Rotation3_2, ProjectToY };
        public static Matrix2x2[] Rotation3ReflectCenter = { ProjectToY, ProjectToY * Rotation3_1, ProjectToY * Rotation3_2 };

        public static Matrix2x2[][] RotationReflectLeft = { null, null, null, Rotation3ReflectLeft, null, null, null };
        public static Matrix2x2[][] RotationReflectCenter = { null, null, null, Rotation3ReflectCenter, null, null, null };
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

        protected void RemovePlanetsButDoesNotUpdateSymmetricGroups(HashSet<FakePlanet> planetsToRemove)
        {
            foreach (FakePlanet planet in planetsToRemove)
                planet.RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy();

            var newPlanets = new System.Collections.Generic.List<FakePlanet>();
            foreach (FakePlanet planet in planets)
                if (!planetsToRemove.Contains(planet))
                {
                    newPlanets.Add(planet);
                }
            planets = newPlanets;
        }

        public void RemoveSymmetricGroup(SymmetricGroup symmetricGroup)
        {
            RemovePlanetsButDoesNotUpdateSymmetricGroups(new HashSet<FakePlanet>(symmetricGroup.planets));
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

        public void MakeRotationalGeneric(int cx, int cy, int d, int n, bool reflectional, bool autoAdvance = false)
        {
            var planetsToRemove = new System.Collections.Generic.HashSet<FakePlanet>();
            var newSymmetricGroups = new System.Collections.Generic.List<SymmetricGroup>();
            var planetsBackup = new System.Collections.Generic.List<FakePlanet>(planets);
            var rotationGroupLookup = new System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>>();
            var locationIndex = MakeLocationIndex();

            var intersectorConnection = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeRightToLeft = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeLeftToRight = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();

            var rotations = Matrix2x2.Rotations[n];
            var reflectionLeft = Matrix2x2.RotationReflectLeft[n];
            var reflectionCenter = Matrix2x2.RotationReflectCenter[n];
            FInt sectorSlope = SymmetryConstants.Rotational[n].sectorSlope;
            FInt distanceCoefficient = SymmetryConstants.Rotational[n].distanceCoefficient;
            bool hasCenter = false;

            if (autoAdvance)
            {
                cy += (d / sectorSlope / 2).ToInt() + 2;
            }
            var center = ArcenPoint.Create(cx, cy);

            foreach (FakePlanet planet in planetsBackup)
            {
                int xdiff = planet.location.X - cx;
                int ydiff = planet.location.Y - cy;

                if (ydiff > 0)
                {
                    planetsToRemove.Add(planet);
                    continue;
                }

                if (xdiff > -ydiff * sectorSlope || xdiff < ydiff * sectorSlope)
                {
                    // planet way out of the sector, removing

                    planetsToRemove.Add(planet);
                    continue;
                }

                var symPoint = ArcenPoint.Create(cx * 2 - planet.location.X, planet.location.Y);
                if ((xdiff > -ydiff * sectorSlope - d * distanceCoefficient / 2 || xdiff < ydiff * sectorSlope + d * distanceCoefficient / 2) && locationIndex.ContainsKey(symPoint))
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
                            planet.location.X = (cx + ydiff * sectorSlope).GetNearestIntPreferringLower();
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

                if (reflectional)
                {
                    if (mergeLeftToRight.ContainsKey(planet))
                    {
                        // "left" reflectional planets
                        var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, n, 1);
                        planet.wobbleMatrix = reflectionLeft[0];
                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            rot.wobbleMatrix = reflectionLeft[j];
                            group.planets.Add(rot);
                        }
                        newSymmetricGroups.Add(group);
                        rotationGroupLookup[planet] = group.planets;
                    }
                    else if (xdiff == 0)
                    {
                        // "central" reflectional planets
                        var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, n, 1);
                        planet.wobbleMatrix = reflectionCenter[0];
                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            rot.wobbleMatrix = reflectionCenter[j];
                            group.planets.Add(rot);
                        }
                        newSymmetricGroups.Add(group);
                        rotationGroupLookup[planet] = group.planets;
                    }
                    else if (xdiff < 0)
                    {
                        var other = locationIndex[symPoint];
                        var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet, other }, n, 2);
                        var subGroupLeft = new System.Collections.Generic.List<FakePlanet> { planet };
                        var subGroupRight = new System.Collections.Generic.List<FakePlanet> { other };

                        other.wobbleMatrix = Matrix2x2.FlipX;

                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            var rotOther = AddPlanetAt(rotations[j].Apply(center, -xdiff, ydiff));
                            rot.wobbleMatrix = rotations[j];
                            rotOther.wobbleMatrix = Matrix2x2.FlipX * rotations[j];
                            group.planets.Add(rot);
                            group.planets.Add(rotOther);
                            subGroupLeft.Add(rot);
                            subGroupRight.Add(rotOther);
                        }
                        newSymmetricGroups.Add(group);
                        rotationGroupLookup[planet] = subGroupLeft;
                        rotationGroupLookup[other] = subGroupRight;
                    }
                }
                else
                {
                    var group = new SymmetricGroup(new System.Collections.Generic.List<FakePlanet> { planet }, n, 1);
                    for (int j = 1; j < n; ++j)
                    {
                        var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                        rot.wobbleMatrix = rotations[j];
                        group.planets.Add(rot);
                    }
                    newSymmetricGroups.Add(group);
                    rotationGroupLookup[planet] = group.planets;
                }
            }

            // Apply various lazy operations recorded above
            RemovePlanetsButDoesNotUpdateSymmetricGroups(planetsToRemove);
            symmetricGroups = newSymmetricGroups;

            // Decide intersector connections
            foreach (FakePlanet planet in planetsBackup)
            {
                if (planetsToRemove.Contains(planet))
                {
                    continue;
                }
                if (mergeLeftToRight.ContainsKey(planet))
                {
                    continue;
                }
                int xdiff = planet.location.X - cx;
                int ydiff = planet.location.Y - cy;
                var symPoint = ArcenPoint.Create(cx * 2 - planet.location.X, planet.location.Y);

                if (xdiff <= ydiff * sectorSlope + d * distanceCoefficient && locationIndex.ContainsKey(symPoint))
                {
                    bool hasAlternativeConnection = false;
                    foreach (FakePlanet neighbor in planet.links)
                    {
                        int neighborXdiff = neighbor.location.X - cx;
                        int neighborYdiff = neighbor.location.Y - cy;

                        if ((mergeLeftToRight.ContainsKey(neighbor) || neighbor.location == center) &&
                            neighbor.location.GetDistanceTo(planet.location, false) < (xdiff - ydiff * sectorSlope) * FInt.Create(1414, false) / distanceCoefficient)
                        {
                            hasAlternativeConnection = true;
                        }
                    }

                    if (!hasAlternativeConnection)
                    {
                        var other = locationIndex[symPoint];
                        intersectorConnection[planet] = other;
                    }
                }
            }

            foreach (var group in rotationGroupLookup.Values)
            {
                var planet = group[0];
                foreach (var neighbor in planet.links)
                {
                    if (rotationGroupLookup.ContainsKey(neighbor))
                    {
                        var neighborGroup = rotationGroupLookup[neighbor];
                        for (int i = 1; i < n; ++i)
                        {
                            group[i].AddLinkTo(neighborGroup[i]);
                        }
                    }
                }

                if (intersectorConnection.ContainsKey(planet))
                {
                    var otherGroup = rotationGroupLookup[intersectorConnection[planet]];
                    for (int i = 0; i < n; ++i)
                    {
                        group[i].AddLinkTo(otherGroup[(i + n - 1) % n]);
                    }
                }
            }
            foreach (var group in rotationGroupLookup.Values)
            {
                var planet = group[0];
                if (mergeLeftToRight.ContainsKey(planet))
                {
                    var rightPlanet = mergeLeftToRight[planet];
                    foreach (var neighbor in rightPlanet.links)
                    {
                        if (!rotationGroupLookup.ContainsKey(neighbor))
                        {
                            continue;
                        }
                        var otherGroup = rotationGroupLookup[neighbor];
                        for (int i = 0; i < n; ++i)
                        {
                            group[i].AddLinkTo(otherGroup[(i + n - 1) % n]);
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
                    var neighborGroup = rotationGroupLookup[neighbor];
                    for (int i = 0; i < n; ++i)
                    {
                        centerPlanet.AddLinkTo(neighborGroup[i]);
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

    public class SymmetryConstants
    {
        // dx = dy * sectorSlope
        public FInt sectorSlope;
        // dx = d * distanceCoefficient
        public FInt distanceCoefficient;

        public SymmetryConstants(FInt sectorSlope, FInt distanceCoefficient)
        { 
            this.sectorSlope = sectorSlope;
            this.distanceCoefficient = distanceCoefficient;
        }

        public static SymmetryConstants Rotational3 = new SymmetryConstants(FInt.Create(1732, false), FInt.Create(2000, false));
        public static SymmetryConstants Rotational4 = new SymmetryConstants(FInt.Create(1000, false), FInt.Create(1414, false));
        public static SymmetryConstants Rotational5 = new SymmetryConstants(FInt.Create(727, false), FInt.Create(1236, false));
        public static SymmetryConstants Rotational6 = new SymmetryConstants(FInt.Create(577, false), FInt.Create(1155, false));
        public static SymmetryConstants[] Rotational = { null, null, null, Rotational3, Rotational4, Rotational5, Rotational6 };
    }
}