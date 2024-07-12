using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;

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

        private int numPlanetTotalToHave;
        public void GenerateMapStructureOnly(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, MapTypeData mapType)
        {
            if (MapgenLogger.IsActive)
                MapgenLogger.Log("GenerateMapStructureOnly: Mapgen_X : " + galaxy.GetTotalPlanetCount() + " planets at start.");

            bool goForPerfectSymmetry = true;

            AngleDegrees startingAngleOffset;
            if (goForPerfectSymmetry)
                startingAngleOffset = AngleDegrees.Create((float)(-45));
            else
            {
                switch (Context.RandomToUse.Next(0, 4))
                {
                    case 0:
                        startingAngleOffset = AngleDegrees.Create((float)(-45));
                        break;
                    case 1:
                        startingAngleOffset = AngleDegrees.Create((float)(45));
                        break;
                    case 2:
                        startingAngleOffset = AngleDegrees.Create((float)(135));
                        break;
                    default:
                        startingAngleOffset = AngleDegrees.Create((float)(225));
                        break;
                }
            }

            FInt distanceToChildren;
            int numPlanets = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            numPlanetTotalToHave = numPlanets;
            if (numPlanets < 80)
                distanceToChildren = FInt.FromParts(400, 000);
            else if (numPlanets < 120)
                distanceToChildren = FInt.FromParts(500, 000);
            else if (numPlanets > 150)
                distanceToChildren = FInt.FromParts(550, 000);
            else if (numPlanets > 250)
                distanceToChildren = FInt.FromParts(600, 000);
            else
                distanceToChildren = FInt.FromParts(700, 000);
            recursionDepth = 0;
            this.InnerGenerate(galaxy, Context, numPlanets, mapType, Engine_AIW2.Instance.GalaxyMapOnly_GalaxyCenter, (FInt)distanceToChildren, startingAngleOffset, null, goForPerfectSymmetry);

            int randomExtraConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "RandomExtraConnections").RelatedIntValue;
            BadgerUtilityMethods.RandomlyConnectXPlanetsWithoutIntersectingOthers(galaxy, randomExtraConnections, 40, 20, Context);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }

        static int recursionDepth = 0;
        private void InnerGenerate(Galaxy galaxy, ArcenHostOnlySimContext Context, int sizeOfSubTree, MapTypeData mapType, ArcenPoint CenterOfSubTree, FInt distanceToChildren,
            AngleDegrees startingAngleOffset, Planet Parent, bool goForPerfectSymmetry)
        {
            PlanetType planetType = PlanetType.Normal;
            int planetsToPlace = sizeOfSubTree;

            if (galaxy.GetCountOfTotalPlanetsDestroyedAndOtherwise() >= numPlanetTotalToHave && !goForPerfectSymmetry)
                return;

            Planet center = galaxy.AddPlanet(planetType, CenterOfSubTree,
                    World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(Context.RandomToUse, PlanetPopulationType.None));
            planetsToPlace--;

            if (Parent != null)
                Parent.AddLinkTo(center);

            if (planetsToPlace <= 0)
                return;

            AngleDegrees startingAngleOffsetForChildren = startingAngleOffset.Add(AngleDegrees.Create((float)Context.RandomToUse.Next(40, 50)));
            //FInt distanceToChildrenForChildren = distanceToChildren / FInt.FromParts( 2, 250 );
            FInt distanceToChildrenForChildren = distanceToChildren / FInt.FromParts(2, 150);

            int defaultNumChildren = 4;
            if (!goForPerfectSymmetry)
            {
                int rand = Context.RandomToUse.Next(0, 100);
                if (recursionDepth >= 2 && rand < 40)
                {
                    //sometimes only do 3 instead of 4 spokes. Just to add a bit of visual variety
                    defaultNumChildren--;
                }
            }

            int numberOfChildren = Math.Min(defaultNumChildren, planetsToPlace);
            AngleDegrees degreesBetweenChildren = AngleDegrees.Create(((float)360) / numberOfChildren);
            int nodesPerChildTree = planetsToPlace / numberOfChildren;
            int extraNodesForChildTrees = planetsToPlace % numberOfChildren;
            if (goForPerfectSymmetry && recursionDepth < 1)
            {
                // We want the main branches of a symmetrical map to have the
                // same number of planets, so if the desired number of planets
                // doesn't divide equally, round them up.
                if (extraNodesForChildTrees > 0)
                {
                    nodesPerChildTree += 1;
                    extraNodesForChildTrees = 0;
                }
            }

            AngleDegrees angleToNextChild = startingAngleOffset;
            if (numberOfChildren == 3)
                angleToNextChild -= 25;
            for (int i = 0; i < numberOfChildren; i++)
            {
                int nodesForThisSubTree = nodesPerChildTree;
                Planet extra = null;
                ArcenPoint childPoint;
                //put a bit of visual wobble into the longer lines for aesthetics
                if (recursionDepth < 2)
                    childPoint = CenterOfSubTree.GetPointAtAngleAndDistance(angleToNextChild + Context.RandomToUse.Next(-10, 10), distanceToChildren.IntValue);
                else
                    childPoint = CenterOfSubTree.GetPointAtAngleAndDistance(angleToNextChild, distanceToChildren.IntValue);
                if (recursionDepth < 2)
                {
                    if (galaxy.GetCountOfTotalPlanetsDestroyedAndOtherwise() >= numPlanetTotalToHave && !goForPerfectSymmetry)
                        return;

                    //Add some extra hops to the inner planets
                    extra = galaxy.AddPlanet(planetType, childPoint,
                        World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(Context.RandomToUse, PlanetPopulationType.None));
                    extra.AddLinkTo(center);
                    childPoint = CenterOfSubTree.GetPointAtAngleAndDistance(angleToNextChild, distanceToChildren.IntValue * 2);
                    planetsToPlace--;
                    nodesForThisSubTree--;
                }

                // If the planets don't divide evenly into our children, we
                // want to distribute those extra nodes evenly among the
                // subtrees.
                if (extraNodesForChildTrees > 0)
                {
                    nodesForThisSubTree++;
                    extraNodesForChildTrees--;
                }

                recursionDepth++;
                Planet planetToPass = center;
                if (extra != null)
                    planetToPass = extra; //this is to make sure the extra planet we've added in will be linked correctly
                this.InnerGenerate(galaxy, Context, nodesForThisSubTree, mapType, childPoint, distanceToChildrenForChildren, startingAngleOffsetForChildren, planetToPass, goForPerfectSymmetry);
                angleToNextChild += degreesBetweenChildren;
                recursionDepth--;
            }
        }
    }
}