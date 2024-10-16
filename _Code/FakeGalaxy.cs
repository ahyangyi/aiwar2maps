using Arcen.AIW2.Core;
using Arcen.Universal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AhyangyiMaps
{
    public class FakePlanet
    {
        public ArcenPoint Location;

        public Matrix2x2 WobbleMatrix = Matrix2x2.Identity;
        public FakePlanet Rotate, Reflect, TranslatePrevious, TranslateNext;

        public int X { get => Location.X; set => Location.X = value; }
        public int Y { get => Location.Y; set => Location.Y = value; }

        public FakePlanet(ArcenPoint location)
        {
            this.Location = location;
            Rotate = null;
            Reflect = null;
            TranslatePrevious = null;
            TranslateNext = null;
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
        public bool stick;

        public SymmetricGroup(System.Collections.Generic.List<FakePlanet> planets)
        {
            this.planets = planets;
            stick = false;
        }

        public void Wobble(PlanetType planetType, int wobble, RandomGenerator rng)
        {
            int dx, dy;
            do
            {
                dx = rng.NextInclus(-1000, 1000);
                dy = rng.NextInclus(-1000, 1000);
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

    public class Outline
    {
        public System.Collections.Generic.List<FakePlanet> Planets;
        public System.Collections.Generic.Dictionary<FakePlanet, int> PlanetIndex;

        int N { get => Planets.Count; }

        public Outline(System.Collections.Generic.List<FakePlanet> planets)
        {
            this.Planets = planets;
            PlanetIndex = new System.Collections.Generic.Dictionary<FakePlanet, int>();
            for (int i = 0; i < N; ++i)
                PlanetIndex[Planets[i]] = i;
        }
        public bool Contains(FakePlanet planet)
        {
            return PlanetIndex.ContainsKey(planet);
        }

        public bool ContainsLink(FakePlanet a, FakePlanet b)
        {
            if (!PlanetIndex.ContainsKey(a)) return false;
            if (!PlanetIndex.ContainsKey(b)) return false;

            int ai = PlanetIndex[a];
            int bi = PlanetIndex[b];

            return ai == (bi + 1) % N || bi == (ai + 1) % N;
        }

        public bool VenturesOutside(FakePlanet a, FakePlanet b)
        {
            if (Contains(a))
            {
                int ai = PlanetIndex[a];
                var prev = Planets[(ai + N - 1) % N].Location;
                var next = Planets[(ai + 1) % N].Location;

                int nextRegion = Geometry.RegionNumber(a.Location, prev, next);
                int bRegion = Geometry.RegionNumber(a.Location, prev, b.Location);
                if (bRegion > nextRegion || bRegion == nextRegion && (b.Location - a.Location).CrossProduct(next - a.Location) > 0)
                {
                    return true;
                }
            }
            if (Contains(b))
            {
                int bi = PlanetIndex[b];
                var prev = Planets[(bi + N - 1) % N].Location;
                var next = Planets[(bi + 1) % N].Location;

                int nextRegion = Geometry.RegionNumber(b.Location, prev, next);
                int aRegion = Geometry.RegionNumber(b.Location, prev, a.Location);
                if (aRegion > nextRegion || aRegion == nextRegion && (a.Location - b.Location).CrossProduct(next - b.Location) > 0)
                {
                    return true;
                }
            }
            for (int i = 0; i < N; ++i)
            {
                if (Geometry.LineSegmentIntersectsLineSegment(a.Location, b.Location, Planets[i].Location, Planets[(i + 1) % N].Location, true))
                    return true;
            }

            return false;
        }
    }

    public class FakePlanetCollection
    {
        public System.Collections.Generic.List<FakePlanet> planets;
        public System.Collections.Generic.List<SymmetricGroup> symmetricGroups;
        public System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet> locationIndex;

        public FakePlanetCollection()
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
    }

    public class FakeGalaxy
    {
        public FakePlanetCollection planetCollection;
        protected System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>> links;

        public FakeGalaxy()
        {
            planetCollection = new FakePlanetCollection();
            links = new System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>>();
        }

        public FakeGalaxy(FakePlanetCollection planetCollection)
        {
            this.planetCollection = planetCollection;
            links = new System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>>();
            foreach (var planet in planets)
            {
                links[planet] = new System.Collections.Generic.List<FakePlanet>();
            }
        }

        public System.Collections.Generic.List<FakePlanet> planets { get => planetCollection.planets; set => planetCollection.planets = value; }
        public System.Collections.Generic.List<SymmetricGroup> symmetricGroups { get => planetCollection.symmetricGroups; set => planetCollection.symmetricGroups = value; }
        public System.Collections.Generic.Dictionary<ArcenPoint, FakePlanet> locationIndex { get => planetCollection.locationIndex; set => planetCollection.locationIndex = value; }

        public FakePlanet AddPlanetAt(ArcenPoint location)
        {
            FakePlanet planet = planetCollection.AddPlanetAt(location);
            links[planet] = new System.Collections.Generic.List<FakePlanet>();
            return planet;
        }

        public bool AddLink(FakePlanet a, FakePlanet b)
        {
            if (a == b) return false;
            if (links[a].Contains(b)) return false;
            links[a].Add(b);
            links[b].Add(a);

            return true;
        }

        public bool RemoveLink(FakePlanet a, FakePlanet b)
        {
            if (a == b) return false;
            if (!links[a].Contains(b)) return false;
            links[a].Remove(b);
            links[b].Remove(a);

            return true;
        }

        protected void RemovePlanetButDoesNotUpdateSymmetricGroups(FakePlanet planet)
        {
            foreach (FakePlanet other in links[planet].ToList())
            {
                RemoveLink(planet, other);
            }

            planets.Remove(planet);
            locationIndex.Remove(planet.Location);
        }

        protected void RemovePlanetsButDoesNotUpdateSymmetricGroups(HashSet<FakePlanet> planetsToRemove)
        {
            foreach (FakePlanet planet in planetsToRemove)
            {
                foreach (FakePlanet other in links[planet].ToList())
                {
                    RemoveLink(planet, other);
                }
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
                foreach (FakePlanet neighbor in links[cur])
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
                    AddSymmetricLinks(chosen, chosenNeighbor);
                }

                queue.Enqueue(chosen);
                visited.Add(chosen);
                shortestDistance.Remove(chosen);

                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    foreach (FakePlanet neighbor in links[cur])
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
                        int distance = ListSymmetricEdges(cur, planet).Select(x => x.Item1.Location.GetDistanceTo(x.Item2.Location, false)).Max();
                        if (distance < shortestDistance[planet].Item1 || shortestDistance[planet].Item2 == null)
                        {
                            shortestDistance[planet] = (distance, cur);
                        }
                    }
                }
            }
        }

        private int AddSymmetricLinks(FakePlanet chosen, FakePlanet chosenNeighbor)
        {
            int ret = 0;
            foreach (var (a, b) in ListSymmetricEdges(chosen, chosenNeighbor))
            {
                if (AddLink(a, b))
                    ++ret;
            }
            return ret;
        }

        protected FakePlanet LeftMostNeighbor(FakePlanet cur, ArcenPoint prev)
        {
            if (links[cur].Count == 0)
                return cur;
            var linksCopy = links[cur].ToList();
            FakePlanet ret = linksCopy[0];
            int retRegion = 0;

            foreach (FakePlanet neighbor in linksCopy)
            {
                int curRegion = Geometry.RegionNumber(cur.Location, prev, neighbor.Location);
                if (curRegion > retRegion || curRegion == retRegion && (neighbor.Location - cur.Location).CrossProduct(ret.Location - cur.Location) > 0)
                {
                    ret = neighbor;
                    retRegion = curRegion;
                }
            }

            return ret;
        }

        public System.Collections.Generic.List<FakePlanet> FindOutline()
        {
            var ret = new System.Collections.Generic.List<FakePlanet>();
            FakePlanet startingPoint = planets.Min(p => (p.X, p.Y, p)).p;
            ret.Add(startingPoint);

            FakePlanet cur = startingPoint;
            ArcenPoint prev = startingPoint.Location - ArcenPoint.Create(0, 1);

            while (true)
            {
                FakePlanet backup = cur;
                cur = LeftMostNeighbor(cur, prev);
                prev = backup.Location;

                if (cur == startingPoint)
                    break;
                ret.Add(cur);
            }

            return ret;
        }

        public void MakeBeltWay()
        {
            int minX = planets.Min(planet => planet.X);
            int maxX = planets.Max(planet => planet.X);
            int minY = planets.Min(planet => planet.Y);
            int maxY = planets.Max(planet => planet.Y);

            var p0 = AddPlanetAt(ArcenPoint.Create(minX - 320, minY - 320));
            var p1 = AddPlanetAt(ArcenPoint.Create(maxX + 320, minY - 320));
            var p2 = AddPlanetAt(ArcenPoint.Create(maxX + 320, maxY + 320));
            var p3 = AddPlanetAt(ArcenPoint.Create(minX - 320, maxY + 320));

            AddLink(p0, p1);
            AddLink(p1, p2);
            AddLink(p2, p3);
            AddLink(p3, p0);

            // XXX: not adding these to symmetry groups seems good enough for now?
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
            var rightLinkBackup = new System.Collections.Generic.Dictionary<FakePlanet, System.Collections.Generic.List<FakePlanet>>();

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
                            rightLinkBackup[planet] = links[planet].ToList();
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
                    foreach (FakePlanet neighbor in links[planet])
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
                foreach (var neighbor in links[planet])
                {
                    if (rotationGroupLookup.ContainsKey(neighbor))
                    {
                        var neighborGroup = rotationGroupLookup[neighbor];
                        for (int i = 1; i < n; ++i)
                        {
                            AddLink(group[i], neighborGroup[i]);
                        }
                    }
                }

                if (intersectorConnection.ContainsKey(planet))
                {
                    var otherGroup = rotationGroupLookup[intersectorConnection[planet]];
                    for (int i = 0; i < n; ++i)
                    {
                        AddLink(group[i], otherGroup[(i + n - 1) % n]);
                    }
                }
            }
            foreach (var group in rotationGroupLookup.Values)
            {
                var planet = group[0];
                if (mergeLeftToRight.ContainsKey(planet))
                {
                    var rightPlanet = mergeLeftToRight[planet];
                    foreach (var neighbor in rightLinkBackup[rightPlanet])
                    {
                        if (!rotationGroupLookup.ContainsKey(neighbor))
                        {
                            continue;
                        }
                        var otherGroup = rotationGroupLookup[neighbor];
                        for (int i = 0; i < n; ++i)
                        {
                            AddLink(group[i], otherGroup[(i + n - 1) % n]);
                        }
                    }
                }
            }

            if (hasCenter)
            {
                var centerPlanet = locationIndex[center];
                var neighbors = links[centerPlanet].ToList();

                centerPlanet.WobbleMatrix = Matrix2x2.Zero;
                centerPlanet.Rotate = centerPlanet;
                centerPlanet.SetReflect(centerPlanet);

                foreach (var neighbor in neighbors)
                {
                    var neighborGroup = rotationGroupLookup[neighbor];
                    for (int i = 0; i < n; ++i)
                    {
                        AddLink(centerPlanet, neighborGroup[i]);
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
        public void MakeDuplexBarrier(int unit, int xWidth, int yWidth)
        {
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);
            var planetsBackup = planets.ToList();

            // Find (and save) the outline
            var outline = new HashSet<FakePlanet>(FindOutline());

            // Recognize rotation symmetry
            MakeRotational2();

            // Create polygons
            // Will be part of the extracted interface
            var center = ArcenPoint.Create(maxX / 2, maxY / 2);
            var innerAnnulusInnerCircle = new System.Collections.Generic.List<ArcenPoint> {
                ArcenPoint.Create(xWidth, yWidth),
                ArcenPoint.Create(maxX - xWidth, yWidth),
                ArcenPoint.Create(maxX - xWidth, maxY - yWidth),
                ArcenPoint.Create(xWidth, maxY - yWidth),
            };
            var innerAnnulusOuterCircle = new System.Collections.Generic.List<ArcenPoint> {
                ArcenPoint.Create(0, 0),
                ArcenPoint.Create(maxX, 0),
                ArcenPoint.Create(maxX, maxY),
                ArcenPoint.Create(0, maxY),
            };
            var outerAnnulusInnerCircle = new System.Collections.Generic.List<ArcenPoint> {
                ArcenPoint.Create(-unit, -unit),
                ArcenPoint.Create(maxX+unit, -unit),
                ArcenPoint.Create(maxX+unit, maxY+unit),
                ArcenPoint.Create(-unit, maxY+unit),
            };
            var outerAnnulusOuterCircle = new System.Collections.Generic.List<ArcenPoint> {
                ArcenPoint.Create(-unit-xWidth, -unit-yWidth),
                ArcenPoint.Create(maxX+unit+xWidth, -unit-yWidth),
                ArcenPoint.Create(maxX+unit+xWidth, maxY+unit+yWidth),
                ArcenPoint.Create(-unit-xWidth, maxY+unit+yWidth),
            };

            // Create "reflections"
            foreach (FakePlanet planet in planetsBackup)
            {
                double psi = (planet.Location - center).AccurateAngleInRadian();
                double oldRho1 = findIntersection(psi, center, innerAnnulusInnerCircle).AccurateDistanceTo(center);
                double oldRho2 = findIntersection(psi, center, innerAnnulusOuterCircle).AccurateDistanceTo(center);
                double newRho1 = findIntersection(psi, center, outerAnnulusInnerCircle).AccurateDistanceTo(center);
                double newRho2 = findIntersection(psi, center, outerAnnulusOuterCircle).AccurateDistanceTo(center);
                double rho = planet.Location.AccurateDistanceTo(center);
                double reflectedRho = (1 - (rho - oldRho1) / (oldRho2 - oldRho1)) * (newRho2 - newRho1) + newRho1;
                var newLocation = ArcenPoint.Create(
                    (int)Math.Round(center.X + Math.Cos(psi) * reflectedRho),
                    (int)Math.Round(center.Y + Math.Sin(psi) * reflectedRho)
                    );

                FakePlanet other = AddPlanetAt(newLocation);
                // FIXME
                other.WobbleMatrix = planet.WobbleMatrix;
                planet.SetReflect(other);
            }

            // Add edges
            foreach (FakePlanet planet in planetsBackup)
            {
                foreach (FakePlanet neighbor in links[planet].ToList())
                {
                    AddLink(planet.Reflect, neighbor.Reflect);
                }
            }

            // Connect each "outline" planet to its counterpart
            foreach (FakePlanet planet in outline)
            {
                AddLink(planet, planet.Reflect);
            }
        }

        private ArcenPoint findIntersection(double radian, ArcenPoint center, System.Collections.Generic.List<ArcenPoint> circle)
        {
            double dx = Math.Cos(radian);
            double dy = Math.Sin(radian);
            for (int i = 0; i < circle.Count; ++i)
            {
                var p1 = circle[i];
                var p2 = circle[(i + 1) % circle.Count];
                double radian1 = (p1 - center).AccurateAngleInRadian();
                double radian2 = (p2 - center).AccurateAngleInRadian();
                if (radian1 <= radian && radian <= radian2 || radian2 <= radian1 && (radian <= radian2 || radian >= radian1))
                {
                    double vx = p2.X - p1.X;
                    double vy = p2.Y - p1.Y;
                    double r = ((center.Y - circle[i].Y) * vx - (center.X - circle[i].X) * vy) / (dx * vy - dy * vx);
                    return ArcenPoint.Create((int)Math.Round(r * dx), (int)Math.Round(r * dy)) + center;
                }
            }
            throw new NotImplementedException();
        }

        public void MakeDoubleSpark()
        {
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);
            int centerY = planets.Where(planet => planet.X * 2 == maxX).Min(planet => planet.Y);

            ArcenPoint center = ArcenPoint.Create(maxX / 2, centerY);
            var rotationA = Matrix2x2.Rotation12_1;
            var rotationB = Matrix2x2.FlipX * rotationA * Matrix2x2.FlipX;

            var reflection = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mapA = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();
            var mapB = new System.Collections.Generic.Dictionary<FakePlanet, FakePlanet>();

            // Step 1: Remove excess planets
            {
                var planetsToRemove = new System.Collections.Generic.HashSet<FakePlanet>();

                foreach (FakePlanet planet in planets)
                {
                    if (planet.Y < System.Math.Abs(planet.X - maxX / 2))
                    {
                        planetsToRemove.Add(planet);
                    }
                }
                RemovePlanetsButDoesNotUpdateSymmetricGroups(planetsToRemove);
            }

            // Step 2: Create rotated planets and add links
            var planetsBackup = planets.ToList();

            foreach (FakePlanet planet in planetsBackup)
            {
                reflection[planet] = locationIndex[ArcenPoint.Create(maxX - planet.X, planet.Y)];
            }

            planets.Clear();
            locationIndex.Clear();

            foreach (FakePlanet planet in planetsBackup)
            {
                mapA[planet] = AddPlanetAt(rotationA.AbsoluteApply(center, planet.Location));
                if (planet.Location == center)
                    mapB[planet] = mapA[planet];
                else
                    mapB[planet] = AddPlanetAt(rotationB.AbsoluteApply(center, planet.Location));
            }

            foreach (FakePlanet planet in planetsBackup)
            {
                Matrix2x2 myMatrix = Matrix2x2.Identity;
                if (planet.X * 2 == maxX)
                {
                    myMatrix = Matrix2x2.ProjectToY;
                }
                else if (planet.X * 2 > maxX)
                {
                    myMatrix = Matrix2x2.FlipX;
                }

                if (mapA[planet] != mapB[planet])
                {
                    mapA[planet].WobbleMatrix = myMatrix * rotationA;
                    mapB[planet].WobbleMatrix = myMatrix * rotationB;
                }
                else
                {
                    mapA[planet].WobbleMatrix = myMatrix;
                }

                ConnectRotatedPlanets(new System.Collections.Generic.List<FakePlanet> { mapA[planet], mapB[planet] });
                mapA[planet].SetReflect(mapA[reflection[planet]]);
                mapB[planet].SetReflect(mapB[reflection[planet]]);
                foreach (FakePlanet neighbor in links[planet])
                {
                    AddLink(mapA[planet], mapA[neighbor]);
                    AddLink(mapB[planet], mapB[neighbor]);
                }
            }
        }
        public void MakeY(AspectRatio e, int d, int xSpan)
        {
            Matrix2x2 rotation = Matrix2x2.Identity;
            if ((int)e == 0)
            {
                rotation = Matrix2x2.Rotation8_1;
            }
            else if ((int)e == 1)
            {
                rotation = Matrix2x2.Rotation10_1;
            }
            else if ((int)e == 2)
            {
                rotation = Matrix2x2.Rotation12_1;
            }
            var transformation = Matrix2x2.FlipY * rotation * Matrix2x2.FlipY * Matrix2x2.FlipX;
            int maxX = planets.Max(planet => planet.X);
            int maxY = planets.Max(planet => planet.Y);
            var planetsBackup = planets.ToList();
            var planetsToRemove = new HashSet<FakePlanet>();
            var invSlope = rotation.xx / rotation.xy;
            var translationBase = ArcenPoint.Create((-d * rotation.xy).GetNearestIntPreferringLower(), (maxY - xSpan / invSlope + d * rotation.xx).GetNearestIntPreferringLower());

            // Step 1: remove excess points & create translated counterparts
            foreach (FakePlanet planet in planetsBackup)
            {
                if (System.Math.Abs(xSpan - planet.X) > invSlope * (maxY - planet.Y))
                {
                    planetsToRemove.Add(planet);
                    continue;
                }
            }
            RemovePlanetsButDoesNotUpdateSymmetricGroups(planetsToRemove);

            planetsBackup = planets.ToList();
            foreach (FakePlanet planet in planetsBackup)
            {
                if (planet.X == xSpan)
                {
                    planet.WobbleMatrix = Matrix2x2.ProjectToY;
                }
                else if (planet.X > xSpan)
                {
                    planet.WobbleMatrix = Matrix2x2.FlipX;
                }

                var translated = AddPlanetAt(transformation.Apply(translationBase, planet.X - maxX, planet.Y));
                planet.SetNextTranslation(translated);
                translated.WobbleMatrix = planet.WobbleMatrix * transformation;
            }

            foreach (FakePlanet planet in planetsBackup)
            {
                foreach (FakePlanet neighbor in links[planet].ToList())
                {
                    if (planet.TranslateNext != null && neighbor.TranslateNext != null)
                    {
                        AddLink(planet.TranslateNext, neighbor.TranslateNext);
                    }
                }
            }

            // Step 2: connect the two parts
            //     we probably need better implementations, but this should do in a pinch
            {
                var distanceThreshold = FInt.Create(1731, false) * d;
                var planetsBackupSet = new HashSet<FakePlanet>(planetsBackup);
                var partA = (from FakePlanet planet in planetsBackup
                             where planet.Y >= maxY - xSpan / invSlope - distanceThreshold * 2
                             select planet).ToList();
                var partB = (from FakePlanet planet in planets
                             where !planetsBackupSet.Contains(planet) && planet.Y <= maxY + distanceThreshold * 2
                             select planet).ToList();
                foreach (FakePlanet a in partA)
                    foreach (FakePlanet b in partB)
                        if (a.Location.GetDistanceTo(b.Location, false) <= distanceThreshold)
                        {
                            AddLink(a, b);
                        }
            }

            // Step 3: create mirror planets
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
                foreach (FakePlanet neighbor in links[planet].ToList())
                {
                    if (planet.Reflect != null && neighbor.Reflect != null)
                    {
                        AddLink(planet.Reflect, neighbor.Reflect);
                    }
                }
            }
            foreach (FakePlanet planet in planetsBackup)
            {
                if (planet.X < xSpan) continue;
                foreach (FakePlanet neighbor in links[planet].ToList())
                {
                    if (neighbor.X >= xSpan && planet.Reflect != null && neighbor.Reflect != null && !links[planet.Reflect].Contains(neighbor.Reflect))
                    {
                        RemoveLink(planet, neighbor);
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
                foreach (FakePlanet neighbor in links[planet])
                {
                    dict[planet].AddLinkTo(dict[neighbor]);
                }
            }
        }

        public bool CrossAtMostLinks(FakePlanet firstPlanet, FakePlanet secondPlanet, int limit)
        {
            if (links[firstPlanet].Contains(secondPlanet))
                return false;

            // We count every edge twice, so...
            limit *= 2;

            foreach (FakePlanet planet in planets)
            {
                if (planet == firstPlanet || planet == secondPlanet)
                {
                    continue;
                }

                foreach (FakePlanet neighbor in links[planet])
                {
                    if (neighbor == firstPlanet || neighbor == secondPlanet)
                    {
                        continue;
                    }
                    if (Geometry.LineSegmentIntersectsLineSegment(firstPlanet.Location, secondPlanet.Location, planet.Location, neighbor.Location))
                    {
                        if (--limit < 0)
                            return false;
                    }
                }
            }

            return true;
        }
        internal void AddExtraLinks(int density, int maxIntersections, RandomGenerator rng, Outline outline)
        {
            FakeGalaxy extra = new FakeGalaxy(planetCollection);
            int linksToAdd = (planets.Count - 1) * density / 100;
            int retry = 0;

            while (linksToAdd > 0)
            {
                FakePlanet a = planets[rng.NextInclus(0, planets.Count - 1)];
                var candidates = planets.Where(x => a != x &&
                    !links[a].Contains(x) &&
                    !extra.links[a].Contains(x) &&
                    CrossAtMostLinks(a, x, 0) &&
                    extra.CrossAtMostLinks(a, x, maxIntersections) &&
                    !outline.VenturesOutside(a, x)
                    ).ToList();

                if (candidates.Count == 0)
                {
                    if (++retry == 1000)
                    {
                        break;
                    }
                    continue;
                }

                FakePlanet b = candidates[rng.NextInclus(0, candidates.Count - 1)];
                var symEdges = ListSymmetricEdges(a, b);
                bool ok = true;
                int newEdges = 0;
                for (int i = 0; i < symEdges.Count; ++i)
                {
                    var (c, d) = symEdges[i];
                    if (links[c].Contains(d) || extra.links[c].Contains(d))
                    {
                        continue;
                    }
                    if (!CrossAtMostLinks(c, d, 0) || !extra.CrossAtMostLinks(c, d, maxIntersections) || outline.VenturesOutside(c, d))
                    {
                        // This link group isn't actually valid, rolling back
                        for (int j = 0; j < i; ++j)
                        {
                            var (e, f) = symEdges[j];
                            extra.RemoveLink(e, f);
                        }

                        ok = false;
                        break;
                    }

                    if (extra.AddLink(c, d))
                    {
                        ++newEdges;
                    }
                }

                if (ok)
                {
                    linksToAdd -= newEdges;
                    retry = 0;
                }
                else
                {
                    if (++retry == 1000)
                    {
                        break;
                    }
                }
            }

            foreach (var planet in extra.planets)
            {
                foreach (var neighbor in extra.links[planet])
                {
                    AddLink(planet, neighbor);
                }
            }
        }

        internal FakeGalaxy MakeSpanningGraph(int traversability, RandomGenerator rng)
        {
            FakeGalaxy spanningGraph = new FakeGalaxy(planetCollection);

            var visited = new HashSet<FakePlanet>();
            var queue = new System.Collections.Generic.Queue<FakePlanet>();
            var shortestDistance = new System.Collections.Generic.Dictionary<FakePlanet, (int, FakePlanet)>();
            var edgeWeight = new System.Collections.Generic.Dictionary<(FakePlanet, FakePlanet), int>();

            foreach (FakePlanet planet in planets)
            {
                foreach (FakePlanet neighbor in links[planet])
                {
                    if (edgeWeight.ContainsKey((planet, neighbor))) continue;

                    int weight = rng.NextInclus(0, 9999);

                    foreach (var (a, b) in ListSymmetricEdges(planet, neighbor))
                    {
                        edgeWeight[(a, b)] = edgeWeight[(b, a)] = weight;
                    }
                }
            }

            foreach (FakePlanet planet in planets)
            {
                shortestDistance[planet] = (int.MaxValue, null);
            }
            shortestDistance[planets[rng.NextInclus(0, planets.Count - 1)]] = (0, null);

            while (visited.Count < planets.Count)
            {
                FakePlanet chosen = null, chosenNeighbor = null;
                int chosenDistance = int.MaxValue;

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
                    spanningGraph.AddSymmetricLinks(chosen, chosenNeighbor);
                }

                queue.Enqueue(chosen);
                visited.Add(chosen);
                shortestDistance.Remove(chosen);

                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    foreach (FakePlanet neighbor in spanningGraph.links[cur])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            shortestDistance.Remove(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }

                    foreach (FakePlanet neighbor in links[cur])
                    {
                        if (shortestDistance.ContainsKey(neighbor))
                        {
                            int distance = edgeWeight[(cur, neighbor)];
                            if (distance < shortestDistance[neighbor].Item1 || shortestDistance[neighbor].Item2 == null)
                            {
                                shortestDistance[neighbor] = (distance, cur);
                            }
                        }
                    }
                }
            }

            return spanningGraph;
        }

        internal void AddEdges(FakeGalaxy subgraph, int connectivity, int traversability, RandomGenerator rng)
        {
            int linksToAdd = Math.Min(planets.Count * connectivity / 20, links.Select(x => x.Value.Count).Sum() / 2) - subgraph.links.Select(x => x.Value.Count).Sum() / 2;
            int retries = 1000;
            while (linksToAdd > 0)
            {
                var cur = planets[rng.NextInclus(0, planets.Count - 1)];
                var neighbors = links[cur].Where(x => !subgraph.links[cur].Contains(x)).ToList();
                if (neighbors.Count == 0) { if (--retries == 0) return; continue; }

                var neighbor = neighbors[rng.NextInclus(0, neighbors.Count - 1)];
                var edgeChange = subgraph.AddSymmetricLinks(cur, neighbor);
                linksToAdd -= edgeChange;
                if (edgeChange == 0)
                {
                    if (--retries == 0) return;
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
                foreach (var neighbor in links[planet])
                    ret.AddLink(planetMap[planet], planetMap[neighbor]);

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
                    galaxy.RemoveLink(planetA, planetB);
                }
            }

            foreach (FakePlanet a in planets)
            {
                ArcenPoint locationA = a.Location + offset;
                foreach (FakePlanet b in links[a])
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
                        galaxy.AddLink(planetA, planetB);
                    }
                }
            }
        }
    }
}