using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;

namespace AhyangyiMaps
{
    public class TessellationTypeGenerator : IMapGenerator
    {
        public TessellationTypeGenerator()
        {
        }

        public void ClearAllMyDataForQuitToMainMenuOrBeforeNewMap()
        {

        }

        public void GenerateMapStructureOnly(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, MapTypeData mapType)
        {
            if (MapgenLogger.IsActive)
                MapgenLogger.Log("GenerateMapStructureOnly: Mapgen_Honeycomb : " + galaxy.GetTotalPlanetCount() + " planets at start.");

            this.InnerGenerate(galaxy, Context, mapConfig, PlanetType.Normal, mapType);
        }

        protected void InnerGenerate(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, PlanetType planetType, MapTypeData mapType)
        {
            int minRings = 0;
            int cellsAtRing = 0;
            int numberToSeed = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            for (; minRings < this.maxCellsByRingCount.Length; minRings++)
            {
                cellsAtRing = this.maxCellsByRingCount[minRings];
                if (cellsAtRing >= numberToSeed)
                    break;
            }

            bool isSolarSnake = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "SolarSnake").RelatedIntValue > 0;
            bool isHoneycomb = !isSolarSnake;

            if (isHoneycomb)
            {
                int extraCellCount = cellsAtRing - numberToSeed;

                FInt extraCellRatio = ((FInt)extraCellCount / (FInt)numberToSeed);
                if (extraCellRatio < FInt.FromParts(0, 100))
                {
                    if (Context.RandomToUse.Next(0, 2) == 0)
                        minRings++;
                }
                else if (extraCellRatio < FInt.FromParts(0, 200))
                {
                    if (Context.RandomToUse.Next(0, 3) == 0)
                        minRings++;
                }
                else if (extraCellRatio < FInt.FromParts(0, 300))
                {
                    if (Context.RandomToUse.Next(0, 4) == 0)
                        minRings++;
                }
            }

            if (minRings > 8)
                minRings = 8;

            int numberOfRows = (int)((float)(minRings * BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue) / 100.0f) - 1;
            //2) Determine number of points on central horizontal (just rings times 2, minus 1)
            int numberOfColumnsOnCentralRow = numberOfRows;

            ArcenPoint[][] pointRows = new ArcenPoint[numberOfRows][];

            int distanceBetweenPoints = planetType.GetData().InterStellarRadius * 4;

            ArcenRectangle seedingArea;
            seedingArea.Width = numberOfRows * distanceBetweenPoints;
            seedingArea.Height = numberOfRows * distanceBetweenPoints;
            seedingArea.X = Engine_AIW2.Instance.GalaxyMapOnly_GalaxyCenter.X - seedingArea.Width / 2;
            seedingArea.Y = Engine_AIW2.Instance.GalaxyMapOnly_GalaxyCenter.Y - seedingArea.Height / 2;

            //int centerX = seedingArea.CalculateCenterPoint().X;
            int centerY = seedingArea.CalculateCenterPoint().Y;

            int centralRowIndex = minRings - 1;
            pointRows[centralRowIndex] = this.Helper_GetHexagonalCoordinatesForRow(numberOfColumnsOnCentralRow, centerY, distanceBetweenPoints, seedingArea.X);

            int rowY = centerY;
            int numberOfCellsOnRow = numberOfColumnsOnCentralRow;
            FInt offset = FInt.Zero;
            for (int i = centralRowIndex + 1; i < pointRows.Length; i++)
            {
                numberOfCellsOnRow -= 1;
                rowY += distanceBetweenPoints;
                offset += FInt.FromParts(0, 500);
                pointRows[i] = this.Helper_GetHexagonalCoordinatesForRow(numberOfCellsOnRow, rowY, distanceBetweenPoints, seedingArea.X + (offset * distanceBetweenPoints).IntValue);
            }

            rowY = centerY;
            numberOfCellsOnRow = numberOfColumnsOnCentralRow;
            offset = FInt.Zero;
            for (int i = centralRowIndex - 1; i >= 0; i--)
            {
                numberOfCellsOnRow -= 1;
                rowY -= distanceBetweenPoints;
                offset += FInt.FromParts(0, 500);
                pointRows[i] = this.Helper_GetHexagonalCoordinatesForRow(numberOfCellsOnRow, rowY, distanceBetweenPoints, seedingArea.X + (offset * distanceBetweenPoints).IntValue);
            }

            // remove excess points
            ArcenPoint[] pointRow;
            ArcenPoint point;
            {
                int totalPoints = 0;
                for (int i = 0; i < pointRows.Length; i++)
                    totalPoints += pointRows[i].Length;
                if (isHoneycomb)
                {
                    int randomRowIndex;
                    int randomCellIndex;
                    while (totalPoints > numberToSeed)
                    {
                        randomRowIndex = Context.RandomToUse.Next(0, pointRows.Length);
                        pointRow = pointRows[randomRowIndex];
                        randomCellIndex = Context.RandomToUse.Next(0, pointRow.Length);
                        point = pointRow[randomCellIndex];
                        if (point.X == 0 && point.Y == 0)
                            continue;
                        pointRow[randomCellIndex] = ArcenPoint.ZeroZeroPoint;
                        totalPoints--;
                    }
                }
                else
                {
                    int numberToRemove = totalPoints - numberToSeed;
                    int numberToRemoveFromTop = numberToRemove / 2;
                    int numberToRemoveFromBottom = numberToRemoveFromTop;
                    if (numberToRemoveFromTop + numberToRemoveFromBottom < numberToRemove)
                        numberToRemoveFromTop++;
                    totalPoints -= this.HoneycombHelper_RemovePointsFromTopOrBottom(pointRows, centralRowIndex, numberToRemoveFromTop, true);
                    totalPoints -= this.HoneycombHelper_RemovePointsFromTopOrBottom(pointRows, centralRowIndex, numberToRemoveFromBottom, false);
                }
            }

            //5) place planets at all points
            Planet[][] planetRows = new Planet[numberOfRows][];
            Planet[] planetRow;
            for (int rowIndex = 0; rowIndex < pointRows.Length; rowIndex++)
            {
                pointRow = pointRows[rowIndex];
                planetRows[rowIndex] = planetRow = new Planet[pointRow.Length];
                for (int columnIndex = 0; columnIndex < pointRow.Length; columnIndex++)
                {
                    point = pointRow[columnIndex];
                    if (point.X == 0 && point.Y == 0)
                        continue;
                    planetRow[columnIndex] = galaxy.AddPlanet(planetType, point,
                        World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(Context.RandomToUse, PlanetPopulationType.None));
                }
            }

            //6) for each row:
            Planet cellPlanet;
            Planet[] nextPlanetRow;
            Planet planetToLink;
            if (isSolarSnake)
            {
                Planet lastPlanet = null;
                bool goingRight = true;
                for (int rowIndex = 0; rowIndex < planetRows.Length; rowIndex++)
                {
                    planetRow = planetRows[rowIndex];
                    if (goingRight)
                    {
                        for (int columnIndex = 0; columnIndex < planetRow.Length; columnIndex++)
                        {
                            cellPlanet = planetRow[columnIndex];
                            if (cellPlanet == null)
                                continue;
                            if (lastPlanet != null)
                                lastPlanet.AddLinkTo(cellPlanet);
                            lastPlanet = cellPlanet;
                        }
                    }
                    else
                    {
                        for (int columnIndex = planetRow.Length - 1; columnIndex >= 0; columnIndex--)
                        {
                            cellPlanet = planetRow[columnIndex];
                            if (cellPlanet == null)
                                continue;
                            if (lastPlanet != null)
                                lastPlanet.AddLinkTo(cellPlanet);
                            lastPlanet = cellPlanet;
                        }
                    }
                    goingRight = !goingRight;
                }
            }
            else
            {
                for (int rowIndex = 0; rowIndex < planetRows.Length; rowIndex++)
                {
                    planetRow = planetRows[rowIndex];
                    //-- for each cell:
                    for (int columnIndex = 0; columnIndex < planetRow.Length; columnIndex++)
                    {
                        cellPlanet = planetRow[columnIndex];
                        if (cellPlanet == null)
                            continue;
                        //--- connect to the next cell
                        if (columnIndex + 1 < planetRow.Length)
                        {
                            planetToLink = planetRow[columnIndex + 1];
                            if (planetToLink != null)
                                cellPlanet.AddLinkTo(planetToLink);
                        }
                        if (rowIndex + 1 < planetRows.Length)
                        {
                            nextPlanetRow = planetRows[rowIndex + 1];
                            //--- if row below has a cell with the same index, connect to it
                            if (columnIndex < nextPlanetRow.Length)
                            {
                                planetToLink = nextPlanetRow[columnIndex];
                                if (planetToLink != null)
                                    cellPlanet.AddLinkTo(planetToLink);
                            }
                            if (rowIndex < centralRowIndex)
                            {
                                //--- if row below has a cell with the same index + 1, connect to it
                                if (columnIndex + 1 < nextPlanetRow.Length)
                                {
                                    planetToLink = nextPlanetRow[columnIndex + 1];
                                    if (planetToLink != null)
                                        cellPlanet.AddLinkTo(planetToLink);
                                }
                            }
                            else
                            {
                                if (columnIndex > 0 && columnIndex - 1 < nextPlanetRow.Length)
                                {
                                    planetToLink = nextPlanetRow[columnIndex - 1];
                                    if (planetToLink != null)
                                        cellPlanet.AddLinkTo(planetToLink);
                                }
                            }
                        }
                    }
                }
            }

            Planet firstPlanet = galaxy.GetFirstNonDestroyedPlanet();

            ThrowawayListCanMemLeak<Planet> planetsNotFoundInCurrentSearch = new ThrowawayListCanMemLeak<Planet>(500);
            ThrowawayListCanMemLeak<Planet> planetsFoundInCurrentSearch = new ThrowawayListCanMemLeak<Planet>(500);
            bool needToCheck = true;
            Planet planetToCheck;
            int lastUnconnected = -1;
            while (needToCheck)
            {
                needToCheck = false;
                planetsNotFoundInCurrentSearch.Clear();
                planetsFoundInCurrentSearch.Clear();

                planetsNotFoundInCurrentSearch.AddRange(galaxy.GetRawListOfPlanetsForMapGenAndNotMuchElseOrItBreaksYourLegs());

                planetsFoundInCurrentSearch.Add(firstPlanet);
                planetsNotFoundInCurrentSearch.Remove(firstPlanet);

                for (int i = 0; i < planetsFoundInCurrentSearch.Count; i++)
                {
                    planetToCheck = planetsFoundInCurrentSearch[i];
                    planetToCheck.DoForLinkedNeighbors(false, delegate (Planet linkedPlanet)
                    {
                        if (planetsFoundInCurrentSearch.Contains(linkedPlanet))
                            return DelReturn.Continue;
                        planetsFoundInCurrentSearch.Add(linkedPlanet);
                        planetsNotFoundInCurrentSearch.Remove(linkedPlanet);
                        return DelReturn.Continue;
                    });
                }

                if (planetsNotFoundInCurrentSearch.Count > 0)
                {
                    needToCheck = true;
                    bool suppressCrossoverAvoidance = false;
                    if (lastUnconnected > 0 && lastUnconnected == planetsNotFoundInCurrentSearch.Count)
                        suppressCrossoverAvoidance = true;
                    lastUnconnected = planetsNotFoundInCurrentSearch.Count;
                    UtilityMethods.Helper_ConnectPlanetLists(planetsFoundInCurrentSearch, planetsNotFoundInCurrentSearch, suppressCrossoverAvoidance, false);
                }
            }

            int randomExtraConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "RandomExtraConnections").RelatedIntValue;
            BadgerUtilityMethods.RandomlyConnectXPlanetsWithoutIntersectingOthers(galaxy, randomExtraConnections, 40, 20, Context);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }

        private int HoneycombHelper_RemovePointsFromTopOrBottom(ArcenPoint[][] pointRows, int centralRowIndex, int numberToRemoveFromTop, bool testingTop)
        {
            int numberRemoved = 0;
            ArcenPoint[] pointRow;
            int rowOffset = 0;
            int columnOffset = 0;
            bool testingLeft = true;
            while (numberToRemoveFromTop > 0)
            {
                if (rowOffset >= centralRowIndex)
                    break;
                int rowIndex = testingTop ? rowOffset : pointRows.Length - 1 - rowOffset;
                pointRow = pointRows[rowIndex];
                int length = pointRow.Length;
                int columnIndex = testingLeft ? columnOffset : length - 1 - columnOffset;
                if (pointRow[columnIndex] != ArcenPoint.ZeroZeroPoint)
                {
                    pointRow[columnIndex] = ArcenPoint.ZeroZeroPoint;
                    numberToRemoveFromTop--;
                    numberRemoved++;
                }
                if (testingLeft)
                    testingLeft = false;
                else
                {
                    testingLeft = true;
                    columnOffset++;
                    FInt maxOffset = (FInt)length / 2;
                    if (columnOffset > maxOffset)
                    {
                        rowOffset++;
                        columnOffset = 0;
                        continue;
                    }
                }
            }
            return numberRemoved;
        }

        private readonly int[] maxCellsByRingCount = new int[] { 0, 1, 7, 19, 37, 61, 91, 127 };
        private ArcenPoint[] Helper_GetHexagonalCoordinatesForRow(int CellCount, int RowHeight, int DistanceBetweenPoints, int StartingX)
        {
            ArcenPoint[] result = new ArcenPoint[CellCount];

            int pointX = StartingX;

            for (int i = 0; i < CellCount; i++)
            {
                result[i] = ArcenPoint.Create(pointX, RowHeight);
                pointX += DistanceBetweenPoints;
            }

            return result;
        }
    }
}