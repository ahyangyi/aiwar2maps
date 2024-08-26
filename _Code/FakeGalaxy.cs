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
        public static Matrix2x2 Rotation5_4 = Matrix2x2.Rotation(FInt.Create(309, false), FInt.Create(-951, false));
        public static Matrix2x2 Rotation5_2 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(588, false));
        public static Matrix2x2 Rotation5_3 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(-588, false));
        public static Matrix2x2[] Rotation5 = { Identity, Rotation5_1, Rotation5_2, Rotation5_3, Rotation5_4 };
        public static Matrix2x2 Rotation6_1 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(866, false));
        public static Matrix2x2 Rotation6_5 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(-866, false));
        public static Matrix2x2[] Rotation6 = { Identity, Rotation6_1, Rotation3_1, Rotation2, Rotation3_2, Rotation6_5 };
        public static Matrix2x2 Rotation7_1 = Matrix2x2.Rotation(FInt.Create(623, false), FInt.Create(782, false));
        public static Matrix2x2 Rotation7_6 = Matrix2x2.Rotation(FInt.Create(623, false), FInt.Create(-782, false));
        public static Matrix2x2 Rotation7_2 = Matrix2x2.Rotation(FInt.Create(-223, false), FInt.Create(975, false));
        public static Matrix2x2 Rotation7_5 = Matrix2x2.Rotation(FInt.Create(-223, false), FInt.Create(-975, false));
        public static Matrix2x2 Rotation7_3 = Matrix2x2.Rotation(FInt.Create(-901, false), FInt.Create(434, false));
        public static Matrix2x2 Rotation7_4 = Matrix2x2.Rotation(FInt.Create(-901, false), FInt.Create(-434, false));
        public static Matrix2x2[] Rotation7 = { Identity, Rotation7_1, Rotation7_2, Rotation7_3, Rotation7_4, Rotation7_5, Rotation7_6 };
        public static Matrix2x2 Rotation8_1 = Matrix2x2.Rotation(FInt.Create(707, false), FInt.Create(707, false));
        public static Matrix2x2 Rotation8_7 = Matrix2x2.Rotation(FInt.Create(707, false), FInt.Create(-707, false));
        public static Matrix2x2 Rotation8_3 = Matrix2x2.Rotation(FInt.Create(-707, false), FInt.Create(707, false));
        public static Matrix2x2 Rotation8_5 = Matrix2x2.Rotation(FInt.Create(-707, false), FInt.Create(-707, false));
        public static Matrix2x2[] Rotation8 = { Identity, Rotation8_1, Rotation4_1, Rotation8_3, Rotation2, Rotation8_5, Rotation4_3, Rotation8_7 };
        public static Matrix2x2[][] Rotations = { null, null, null, Rotation3, Rotation4, Rotation5, Rotation6, Rotation7, Rotation8 };

        public static Matrix2x2[] Rotation3ReflectLeft = { ProjectToY * Rotation3_1, ProjectToY * Rotation3_2, ProjectToY };
        public static Matrix2x2[] Rotation3ReflectCenter = { ProjectToY, ProjectToY * Rotation3_1, ProjectToY * Rotation3_2 };

        public static Matrix2x2 ProjectToXY = new Matrix2x2(FInt.Create(707, false), FInt.Create(707, false), FInt.Create(707, false), FInt.Create(707, false));
        public static Matrix2x2[] Rotation4ReflectLeft = { ProjectToXY, ProjectToXY * Rotation4_1, ProjectToXY * Rotation2, ProjectToXY * Rotation4_3 };
        public static Matrix2x2[] Rotation4ReflectCenter = { ProjectToY, ProjectToY * Rotation4_1, ProjectToY * Rotation2, ProjectToY * Rotation4_3 };

        public static Matrix2x2[][] RotationReflectLeft = { null, null, null, Rotation3ReflectLeft, Rotation4ReflectLeft, null, null, null, null };
        public static Matrix2x2[][] RotationReflectCenter = { null, null, null, Rotation3ReflectCenter, Rotation4ReflectCenter, null, null, null, null };
    }
    public class FakePlanet
    {
        public ArcenPoint Location;
        public HashSet<FakePlanet> Links;
        public Matrix2x2 WobbleMatrix = Matrix2x2.Identity;
        public FakePlanet Rotate, Reflect, TranslatePrevious, TranslateNext;

        public int X { get => Location.X; set => Location.X = value; }
        public int Y { get => Location.Y; set => Location.Y = value; }

        public FakePlanet(ArcenPoint location)
        {
            this.Location = location;
            Links = new HashSet<FakePlanet>();
            Rotate = null;
            Reflect = null;
            TranslatePrevious = null;
            TranslateNext = null;
        }

        public void AddLinkTo(FakePlanet other)
        {
            if (this == other) return;
            Links.Add(other);
            other.Links.Add(this);
        }

        public void RemoveLinkTo(FakePlanet other)
        {
            Links.Remove(other);
            other.Links.Remove(this);
        }

        public void RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy()
        {
            foreach (FakePlanet other in Links)
            {
                other.Links.Remove(this);
            }
        }
        public void Wobble(int wobble, FInt dx, FInt dy)
        {
            (dx, dy) = WobbleMatrix.Apply(dx, dy);
            X += (dx * wobble).GetNearestIntPreferringLower();
            Y += (dy * wobble).GetNearestIntPreferringLower();
        }
        public void FakeReflect(FakePlanet other)
        {
            Rotate = other;
            other.Rotate = this;
        }

        public void SetReflect(FakePlanet other)
        {
            Reflect = other;
            other.Reflect = this;
        }

        public void SetNextTranslation(FakePlanet other)
        {
            TranslateNext = other;
            other.TranslatePrevious = this;
        }
    }

    public class SymmetricGroup
    {
        public System.Collections.Generic.List<FakePlanet> planets;

        public SymmetricGroup(System.Collections.Generic.List<FakePlanet> planets)
        {
            this.planets = planets;
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
        public System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet> locationIndex;

        public FakeGalaxy()
        {
            planets = new System.Collections.Generic.List<FakePlanet>();
            symmetricGroups = new System.Collections.Generic.List<SymmetricGroup>();
            locationIndex = new System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet>();
        }

        public FakePlanet AddPlanetAt(ArcenPoint location)
        {
            FakePlanet planet = new FakePlanet(location);
            planets.Add(planet);
            locationIndex[planet.Location] = planet;
            return planet;
        }

        protected void RemovePlanetButDoesNotUpdateSymmetricGroups(FakePlanet planet)
        {
            planet.RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy();
            planets.Remove(planet);
            locationIndex.Remove(planet.Location);
        }

        protected void RemovePlanetsButDoesNotUpdateSymmetricGroups(HashSet<FakePlanet> planetsToRemove)
        {
            foreach (FakePlanet planet in planetsToRemove)
            {
                planet.RemoveAllLinksOnlyUsableForRemovingPlanetFromGalaxy();
                locationIndex.Remove(planet.Location);
            }

            var newPlanets = new System.Collections.Generic.List<FakePlanet>();
            foreach (FakePlanet planet in planets)
                if (!planetsToRemove.Contains(planet))
                {
                    newPlanets.Add(planet);
                }
            planets = newPlanets;
        }

        protected void ConnectRotatedPlanets(System.Collections.Generic.List<FakePlanet> planets)
        {
            for (int i = 0; i < planets.Count; ++i)
            {
                planets[i].Rotate = planets[(i + 1) % planets.Count];
            }
        }

        public void MakeSymmetricGroups()
        {
            var visited = new HashSet<FakePlanet>();
            symmetricGroups.Clear();

            foreach (FakePlanet planet in planets)
            {
                if (visited.Contains(planet))
                    continue;

                var planetsInGroup = new System.Collections.Generic.List<FakePlanet> { planet };
                visited.Add(planet);

                for (int i = 0; i < planetsInGroup.Count; ++i)
                {
                    FakePlanet cur = planetsInGroup[i];
                    if (cur.Rotate != null && !visited.Contains(cur.Rotate))
                    {
                        planetsInGroup.Add(cur.Rotate);
                        visited.Add(cur.Rotate);
                    }
                    if (cur.Reflect != null && !visited.Contains(cur.Reflect))
                    {
                        planetsInGroup.Add(cur.Reflect);
                        visited.Add(cur.Reflect);
                    }
                    if (cur.TranslatePrevious != null && !visited.Contains(cur.TranslatePrevious))
                    {
                        planetsInGroup.Add(cur.TranslatePrevious);
                        visited.Add(cur.TranslatePrevious);
                    }
                    if (cur.TranslateNext != null && !visited.Contains(cur.TranslateNext))
                    {
                        planetsInGroup.Add(cur.TranslateNext);
                        visited.Add(cur.TranslateNext);
                    }
                }

                symmetricGroups.Add(new SymmetricGroup(planetsInGroup));
            }
        }

        protected System.Collections.Generic.List<(FakePlanet, FakePlanet)> ListSymmetricEdges(FakePlanet a, FakePlanet b)
        {
            var visited = new HashSet<(FakePlanet, FakePlanet)>();
            var queue = new System.Collections.Generic.Queue<(FakePlanet, FakePlanet)>();

            visited.Add((a, b));
            queue.Enqueue((a, b));
            while (queue.Count > 0)
            {
                var (c, d) = queue.Dequeue();

                if (c.Rotate != null && d.Rotate != null && !visited.Contains((c.Rotate, d.Rotate)))
                {
                    visited.Add((c.Rotate, d.Rotate));
                    queue.Enqueue((c.Rotate, d.Rotate));
                }
                if (c.Reflect != null && d.Reflect != null && !visited.Contains((c.Reflect, d.Reflect)))
                {
                    visited.Add((c.Reflect, d.Reflect));
                    queue.Enqueue((c.Reflect, d.Reflect));
                }
                if (c.TranslatePrevious != null && d.TranslatePrevious != null && !visited.Contains((c.TranslatePrevious, d.TranslatePrevious)))
                {
                    visited.Add((c.TranslatePrevious, d.TranslatePrevious));
                    queue.Enqueue((c.TranslatePrevious, d.TranslatePrevious));
                }
                if (c.TranslateNext != null && d.TranslateNext != null && !visited.Contains((c.TranslateNext, d.TranslateNext)))
                {
                    visited.Add((c.TranslateNext, d.TranslateNext));
                    queue.Enqueue((c.TranslateNext, d.TranslateNext));
                }
            }
            return visited.ToList();
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
            var queue = new System.Collections.Generic.Queue<FakePlanet>();
            visited.Add(planets[0]);
            queue.Enqueue(planets[0]);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (FakePlanet neighbor in cur.Links)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return visited.Count == planets.Count;
        }

        public System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet> MakeLocationIndex()
        {
            return planets.ToDictionary(planet => planet.Location, planet => planet);
        }

        public void EnsureConnectivity()
        {
            if (planets.Count == 0)
            {
                return;
            }

            var visited = new HashSet<FakePlanet>();
            var queue = new System.Collections.Generic.Queue<FakePlanet>();
            var shortestDistance = new System.Collections.Generic.Dictionary<FakePlanet, (int, FakePlanet)>();
            foreach (FakePlanet planet in planets)
            {
                shortestDistance[planet] = (int.MaxValue, null);
            }
            shortestDistance[planets[0]] = (0, null);

            while (visited.Count < planets.Count)
            {
                FakePlanet chosen = null, chosenNeighbor = null;
                int chosenDistance = int.MaxValue; // unnecessary but makes lint happy

                foreach (var kv in shortestDistance)
                {
                    FakePlanet planet = kv.Key;
                    if (visited.Contains(planet))
                    {
                        continue;
                    }

                    if (chosen == null || kv.Value.Item1 < chosenDistance)
                    {
                        chosen = kv.Key;
                        chosenDistance = kv.Value.Item1;
                        chosenNeighbor = kv.Value.Item2;
                    }
                }

                if (chosenNeighbor != null)
                {
                    foreach (var (a, b) in ListSymmetricEdges(chosen, chosenNeighbor))
                    {
                        a.AddLinkTo(b);
                    }
                }

                queue.Enqueue(chosen);
                visited.Add(chosen);
                shortestDistance.Remove(chosen);

                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    foreach (FakePlanet neighbor in cur.Links)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            shortestDistance.Remove(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }

                    foreach (FakePlanet planet in shortestDistance.Keys.ToList())
                    {
                        int distance = cur.Location.GetDistanceTo(planet.Location, false);
                        if (distance < shortestDistance[planet].Item1 || shortestDistance[planet].Item2 == null)
                        {
                            shortestDistance[planet] = (distance, cur);
                        }
                    }
                }
            }
        }

        public void MakeBilateral()
        {
            int maxX = planets.Max(planet => planet.X);

            foreach (FakePlanet planet in planets)
            {
                if (planet.X * 2 < maxX)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.X, planet.Y)];
                    other.WobbleMatrix = Matrix2x2.FlipX;
                    planet.SetReflect(other);
                }
                else if (planet.X * 2 == maxX)
                {
                    planet.WobbleMatrix = Matrix2x2.ProjectToY;
                    planet.SetReflect(planet);
                }
            }
        }

        public void MakeRotational2()
        {
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);

            foreach (FakePlanet planet in planets)
            {
                if (planet.X * 2 < maxX || planet.X * 2 == maxX && planet.Y * 2 < maxY)
                {
                    FakePlanet other = locationIndex[ArcenPoint.Create(maxX - planet.X, maxY - planet.Y)];
                    other.WobbleMatrix = Matrix2x2.Rotation2;
                    ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet, other });
                }
                else if (planet.X * 2 == maxX && planet.Y * 2 == maxY)
                {
                    planet.WobbleMatrix = Matrix2x2.Zero;
                    ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet });
                }
            }
        }

        public void MakeRotational2Bilateral()
        {
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);

            symmetricGroups.Clear();
            foreach (FakePlanet planet in planets)
            {
                if (planet.X * 2 < maxX)
                {
                    if (planet.Y * 2 < maxY)
                    {
                        FakePlanet rot = locationIndex[ArcenPoint.Create(maxX - planet.X, maxY - planet.Y)];
                        FakePlanet flipX = locationIndex[ArcenPoint.Create(maxX - planet.X, planet.Y)];
                        FakePlanet flipY = locationIndex[ArcenPoint.Create(planet.X, maxY - planet.Y)];
                        rot.WobbleMatrix = Matrix2x2.Rotation2;
                        flipX.WobbleMatrix = Matrix2x2.FlipX;
                        flipY.WobbleMatrix = Matrix2x2.FlipY;
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet, rot });
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { flipX, flipY });
                        planet.SetReflect(flipX);
                        rot.SetReflect(flipY);
                    }
                    else if (planet.Y * 2 == maxY)
                    {
                        FakePlanet flipX = locationIndex[ArcenPoint.Create(maxX - planet.X, planet.Y)];
                        planet.WobbleMatrix = Matrix2x2.ProjectToX;
                        flipX.WobbleMatrix = Matrix2x2.ProjectToNegX;
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet, flipX });
                        planet.SetReflect(flipX);
                    }
                }
                else if (planet.X * 2 == maxX)
                {
                    if (planet.Y * 2 < maxY)
                    {
                        FakePlanet flipY = locationIndex[ArcenPoint.Create(planet.X, maxY - planet.Y)];
                        planet.WobbleMatrix = Matrix2x2.ProjectToY;
                        flipY.WobbleMatrix = Matrix2x2.ProjectToNegY;
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet, flipY });
                        planet.SetReflect(planet);
                        flipY.SetReflect(flipY);
                    }
                    else if (planet.Y * 2 == maxY)
                    {
                        planet.WobbleMatrix = Matrix2x2.Zero;
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet });
                        planet.SetReflect(planet);
                    }
                }
            }
        }
        public void MakeRotationalGeneric(int cx, int cy, int d, int n, bool reflectional, bool autoAdvance = false)
        {
            var planetsToRemove = new HashSet<FakePlanet>();
            var planetsBackup = new System.Collections.Generic.List<FakePlanet>(planets);
            var rotationGroupLookup = new System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>>();

            var intersectorConnection = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeRightToLeft = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mergeLeftToRight = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();

            var rotations = Matrix2x2.Rotations[n];
            var reflectionLeft = Matrix2x2.RotationReflectLeft[n];
            var reflectionCenter = Matrix2x2.RotationReflectCenter[n];
            FInt sectorSlope = SymmetryConstants.Rotational[n].sectorSlope;
            FInt distanceCoefficient = SymmetryConstants.Rotational[n].distanceCoefficient;
            FInt mergeMargin = d * distanceCoefficient * FInt.Create(400, false);
            bool hasCenter = false;

            if (autoAdvance)
            {
                cy += (d / sectorSlope / 2).ToInt() + 2;
            }
            var center = ArcenPoint.Create(cx, cy);

            foreach (FakePlanet planet in planetsBackup)
            {
                int xdiff = planet.X - cx;
                int ydiff = planet.Y - cy;

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

                var symPoint = ArcenPoint.Create(cx * 2 - planet.X, planet.Y);
                if ((xdiff > -ydiff * sectorSlope - mergeMargin || xdiff < ydiff * sectorSlope + mergeMargin) && locationIndex.ContainsKey(symPoint))
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
                            planet.X = (cx + ydiff * sectorSlope).GetNearestIntPreferringLower();
                            xdiff = planet.X - cx;
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
                        var group = new System.Collections.Generic.List<FakePlanet> { planet };
                        planet.WobbleMatrix = reflectionLeft[0];
                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            rot.WobbleMatrix = reflectionLeft[j];
                            group.Add(rot);
                        }
                        ConnectRotatedPlanets(group);
                        for (int j = 0; j < n; ++j)
                        {
                            group[j].SetReflect(group[(n + 1 - j) % n]);
                        }
                        rotationGroupLookup[planet] = group;
                    }
                    else if (xdiff == 0)
                    {
                        // "central" reflectional planets
                        var group = new System.Collections.Generic.List<FakePlanet> { planet };
                        planet.WobbleMatrix = reflectionCenter[0];
                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            rot.WobbleMatrix = reflectionCenter[j];
                            group.Add(rot);
                        }
                        ConnectRotatedPlanets(group);
                        for (int j = 0; j < n; ++j)
                        {
                            group[j].SetReflect(group[(n - j) % n]);
                        }
                        rotationGroupLookup[planet] = group;
                    }
                    else if (xdiff < 0)
                    {
                        var other = locationIndex[symPoint];
                        var subGroupLeft = new System.Collections.Generic.List<FakePlanet> { planet };
                        var subGroupRight = new System.Collections.Generic.List<FakePlanet> { other };

                        other.WobbleMatrix = Matrix2x2.FlipX;

                        for (int j = 1; j < n; ++j)
                        {
                            var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                            var rotOther = AddPlanetAt(rotations[j].Apply(center, -xdiff, ydiff));
                            rot.WobbleMatrix = rotations[j];
                            rotOther.WobbleMatrix = Matrix2x2.FlipX * rotations[j];
                            subGroupLeft.Add(rot);
                            subGroupRight.Add(rotOther);
                        }
                        ConnectRotatedPlanets(subGroupLeft);
                        ConnectRotatedPlanets(subGroupRight);
                        for (int j = 0; j < n; ++j)
                        {
                            subGroupLeft[j].SetReflect(subGroupRight[(n - j) % n]);
                        }
                        rotationGroupLookup[planet] = subGroupLeft;
                        rotationGroupLookup[other] = subGroupRight;
                    }
                }
                else
                {
                    var group = new System.Collections.Generic.List<FakePlanet> { planet };
                    for (int j = 1; j < n; ++j)
                    {
                        var rot = AddPlanetAt(rotations[j].Apply(center, xdiff, ydiff));
                        rot.WobbleMatrix = rotations[j];
                        group.Add(rot);
                    }
                    ConnectRotatedPlanets(group);
                    rotationGroupLookup[planet] = group;
                }
            }

            // Apply various lazy operations recorded above
            RemovePlanetsButDoesNotUpdateSymmetricGroups(planetsToRemove);

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
                int xdiff = planet.X - cx;
                int ydiff = planet.Y - cy;
                var symPoint = ArcenPoint.Create(cx * 2 - planet.X, planet.Y);

                if (xdiff <= ydiff * sectorSlope + d * distanceCoefficient && locationIndex.ContainsKey(symPoint))
                {
                    bool hasAlternativeConnection = false;
                    foreach (FakePlanet neighbor in planet.Links)
                    {
                        int neighborXdiff = neighbor.X - cx;
                        int neighborYdiff = neighbor.Y - cy;

                        if ((mergeLeftToRight.ContainsKey(neighbor) || neighbor.Location == center) &&
                            neighbor.Location.GetDistanceTo(planet.Location, false) < (xdiff - ydiff * sectorSlope) * FInt.Create(1414, false) / distanceCoefficient)
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
                foreach (var neighbor in planet.Links)
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
                    foreach (var neighbor in rightPlanet.Links)
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
                var neighbors = centerPlanet.Links.ToList();

                centerPlanet.WobbleMatrix = Matrix2x2.Zero;
                centerPlanet.Rotate = centerPlanet;
                centerPlanet.SetReflect(centerPlanet);

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
        public void MakeTranslational2(int xDiff)
        {
            foreach (FakePlanet planet in planets)
            {
                ArcenPoint newPoint = ArcenPoint.Create(planet.X + xDiff, planet.Y);
                if (locationIndex.ContainsKey(newPoint))
                {
                    FakePlanet other = locationIndex[newPoint];
                    planet.SetNextTranslation(other);
                }
            }
        }
        public void MakeTriptych(int xDiff)
        {
            foreach (FakePlanet planet in planets)
            {
                {
                    ArcenPoint newPoint = ArcenPoint.Create(xDiff * 2 - planet.X, planet.Y);
                    if (locationIndex.ContainsKey(newPoint))
                    {
                        FakePlanet other = locationIndex[newPoint];
                        planet.SetReflect(other);
                    }
                }
                {
                    ArcenPoint newPoint = ArcenPoint.Create(xDiff * 4 - planet.X, planet.Y);
                    if (locationIndex.ContainsKey(newPoint))
                    {
                        FakePlanet other = locationIndex[newPoint];
                        planet.FakeReflect(other);
                    }
                }
                if (planet.X % xDiff == 0)
                {
                    planet.WobbleMatrix = Matrix2x2.ProjectToY;
                }
                else if (planet.X > xDiff && planet.X < xDiff * 2)
                {
                    planet.WobbleMatrix = Matrix2x2.FlipX;
                }
            }
        }
        public void MakeDualGalaxy(int xDiff)
        {
            int maxX = planets.Max(planet => planet.X);
            int xRotate = maxX - xDiff;
            int maxY = planets.Max(planet => planet.Y);

            foreach (FakePlanet planet in planets)
            {
                {
                    ArcenPoint newPoint = ArcenPoint.Create(planet.X + xDiff, planet.Y);
                    if (locationIndex.ContainsKey(newPoint))
                    {
                        FakePlanet other = locationIndex[newPoint];
                        planet.SetNextTranslation(other);
                    }
                }
                {
                    ArcenPoint newPoint = ArcenPoint.Create(maxX - planet.X, maxY - planet.Y);
                    if (locationIndex.ContainsKey(newPoint))
                    {
                        FakePlanet other = locationIndex[newPoint];
                        ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { planet, other });
                    }
                }
                int x = planet.X % xDiff;
                if (x <= maxX - xDiff)
                {
                    if (x * 2 == maxX - xDiff && planet.Y * 2 == maxY)
                    {
                        planet.WobbleMatrix = Matrix2x2.Zero;
                    }
                    else if (x * 2 > maxX - xDiff || x * 2 == maxX - xDiff && planet.Y * 2 > maxY)
                    {
                        planet.WobbleMatrix = Matrix2x2.Rotation2;
                    }
                }
                else
                {
                    x -= maxX - xDiff;
                    if (x * 2 == xDiff * 2 - maxX && planet.Y * 2 == maxY)
                    {
                        planet.WobbleMatrix = Matrix2x2.Zero;
                    }
                    else if (x * 2 > xDiff * 2 - maxX || x * 2 == xDiff * 2 - maxX && planet.Y * 2 > maxY)
                    {
                        planet.WobbleMatrix = Matrix2x2.Rotation2;
                    }
                }
            }
        }
        public void MakeDuplexBarrier()
        {
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);
            var smallToLarge = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var planetsBackup = planets.ToList();

            foreach (FakePlanet planet in planetsBackup)
            {

            }
        }
        public void MakeDoubleSpark()
        {

        }
        public void MakeY(Matrix2x2 rotation, int d, int xSpan)
        {
            var transformation = Matrix2x2.FlipY * rotation * Matrix2x2.FlipY * Matrix2x2.FlipX;
            int maxY = planets.Max(planet => planet.Y);
            var planetsBackup = planets.ToList();
            var planetsToRemove = new HashSet<FakePlanet>();

            // Step 1: remove excess points & create translated counterparts
            foreach (FakePlanet planet in planetsBackup)
            {
                if (xSpan - planet.X > maxY - planet.Y)
                {
                    planetsToRemove.Add(planet);
                    continue;
                }

                if (planet.X == xSpan)
                {
                    planet.WobbleMatrix = Matrix2x2.ProjectToY;
                }
                else if (planet.X > xSpan)
                {
                    planet.WobbleMatrix = Matrix2x2.FlipX;
                }

                var translated = AddPlanetAt(transformation.Apply(ArcenPoint.Create(-(int)(d * 1.414 / 2), maxY - xSpan + (int)(d * 1.414 / 2)), planet.X - xSpan, planet.Y));
                planet.SetNextTranslation(translated);
                translated.WobbleMatrix = planet.WobbleMatrix * transformation;
            }

            RemovePlanetsButDoesNotUpdateSymmetricGroups(planetsToRemove);

            foreach (FakePlanet planet in planetsBackup)
            {
                if (planetsToRemove.Contains(planet))
                    continue;

                foreach (FakePlanet neighbor in planet.Links)
                {
                    if (planet.TranslateNext != null && neighbor.TranslateNext != null)
                    {
                        planet.TranslateNext.AddLinkTo(neighbor.TranslateNext);
                    }
                }
            }

            // Step 2: create mirror planets
            planetsBackup = planets.ToList();

            foreach (FakePlanet planet in planetsBackup)
            {
                var otherLocation = ArcenPoint.Create(xSpan * 2 - planet.X, planet.Y);
                FakePlanet other = null;
                if (locationIndex.ContainsKey(otherLocation))
                {
                    other = locationIndex[otherLocation];
                }
                else
                {
                    other = AddPlanetAt(otherLocation);
                    other.WobbleMatrix = planet.WobbleMatrix * Matrix2x2.FlipX;
                }

                planet.SetReflect(other);
            }
            foreach (FakePlanet planet in planetsBackup)
            {
                if (planet.X >= xSpan) continue;
                foreach (FakePlanet neighbor in planet.Links.ToList())
                {
                    if (planet.Reflect != null && neighbor.Reflect != null)
                    {
                        planet.Reflect.AddLinkTo(neighbor.Reflect);
                    }
                }
            }
            foreach (FakePlanet planet in planetsBackup)
            {
                if (planet.X < xSpan) continue;
                foreach (FakePlanet neighbor in planet.Links.ToList())
                {
                    if (neighbor.X >= xSpan && planet.Reflect != null && neighbor.Reflect != null && !planet.Reflect.Links.Contains(neighbor.Reflect))
                    {
                        planet.RemoveLinkTo(neighbor);
                    }
                }
            }
        }

        public void Populate(Galaxy galaxy, PlanetType planetType, RandomGenerator rng)
        {
            System.Collections.Generic.Dictionary<FakePlanet, Planet> dict = new System.Collections.Generic.Dictionary<FakePlanet, Planet>();
            foreach (FakePlanet planet in planets)
            {
                dict[planet] = galaxy.AddPlanet(planetType, planet.Location,
                        World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(rng, PlanetPopulationType.None));
            }
            foreach (FakePlanet planet in planets)
            {
                foreach (FakePlanet neighbor in planet.Links)
                {
                    dict[planet].AddLinkTo(dict[neighbor]);
                }
            }
        }
    }

    public class FakePattern : FakeGalaxy
    {
        public System.Collections.Generic.List<(ArcenPoint, ArcenPoint)> connectionsToBreak;
        public System.Collections.Generic.Dictionary<(ArcenPoint, ArcenPoint), System.Collections.Generic.List<ArcenPoint>> breakpoints;


        public FakePattern() : base()
        {
            connectionsToBreak = new System.Collections.Generic.List<(ArcenPoint, ArcenPoint)>();
            breakpoints = new System.Collections.Generic.Dictionary<(ArcenPoint, ArcenPoint), System.Collections.Generic.List<ArcenPoint>>();
        }

        public delegate ArcenPoint CoordinateTransformation(ArcenPoint point);

        public FakePattern Transform(CoordinateTransformation trans)
        {
            FakePattern ret = new FakePattern();
            var planetMap = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            int maxY = planets.Max(planet => planet.Y);

            foreach (FakePlanet planet in planets)
                planetMap[planet] = ret.AddPlanetAt(trans(planet.Location));

            foreach (FakePlanet planet in planets)
                foreach (var neighbor in planet.Links)
                    planetMap[planet].AddLinkTo(planetMap[neighbor]);

            ret.connectionsToBreak = connectionsToBreak.Select(p => (trans(p.Item1), trans(p.Item2))).ToList();

            foreach (var (a, b) in breakpoints.Keys)
                ret.breakpoints[(trans(a), trans(b))] = (from p in breakpoints[(a, b)]
                                                         select trans(p)).ToList();

            return ret;
        }
        public FakePattern FlipX()
        {
            int maxX = planets.Max(planet => planet.X);
            return Transform(p => ArcenPoint.Create(maxX - p.X, p.Y));
        }

        public FakePattern FlipY()
        {
            int maxY = planets.Max(planet => planet.Y);
            return Transform(p => ArcenPoint.Create(p.X, maxY - p.Y));
        }
        public FakePattern RotateLeft()
        {
            int maxY = planets.Max(planet => planet.Y);
            return Transform(p => ArcenPoint.Create(maxY - p.Y, p.X));
        }

        public void Imprint(FakeGalaxy galaxy, ArcenPoint offset)
        {
            foreach (FakePlanet planet in planets)
            {
                var offsettedLocation = planet.Location + offset;
                if (!galaxy.locationIndex.ContainsKey(offsettedLocation))
                {
                    galaxy.AddPlanetAt(offsettedLocation);
                }
            }

            foreach (var (a, b) in connectionsToBreak)
            {
                ArcenPoint locationA = a + offset;
                ArcenPoint locationB = b + offset;
                if (galaxy.locationIndex.ContainsKey(locationA) && galaxy.locationIndex.ContainsKey(locationB))
                {
                    var planetA = galaxy.locationIndex[locationA];
                    var planetB = galaxy.locationIndex[locationB];
                    planetA.RemoveLinkTo(planetB);
                }
            }

            foreach (FakePlanet a in planets)
            {
                ArcenPoint locationA = a.Location + offset;
                foreach (FakePlanet b in a.Links)
                {
                    ArcenPoint locationB = b.Location + offset;
                    var planetA = galaxy.locationIndex[locationA];
                    var planetB = galaxy.locationIndex[locationB];
                    bool ok = true;
                    if (breakpoints.ContainsKey((a.Location, b.Location)))
                    {
                        foreach (ArcenPoint c in breakpoints[(a.Location, b.Location)])
                        {
                            if (galaxy.locationIndex.ContainsKey(c + offset))
                            {
                                ok = false;
                            }
                        }
                    }
                    if (breakpoints.ContainsKey((b.Location, a.Location)))
                    {
                        foreach (ArcenPoint c in breakpoints[(b.Location, a.Location)])
                        {
                            if (galaxy.locationIndex.ContainsKey(c + offset))
                            {
                                ok = false;
                            }
                        }
                    }
                    if (ok)
                    {
                        planetA.AddLinkTo(planetB);
                    }
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
        public static SymmetryConstants Rotational7 = new SymmetryConstants(FInt.Create(482, false), FInt.Create(1110, false));
        public static SymmetryConstants Rotational8 = new SymmetryConstants(FInt.Create(414, false), FInt.Create(1082, false));
        public static SymmetryConstants[] Rotational = { null, null, null, Rotational3, Rotational4, Rotational5, Rotational6, Rotational7, Rotational8 };
    }
}