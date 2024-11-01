using AhyangyiMaps.Tessellation;
using System.Collections.Generic;

namespace AhyangyiMaps
{

    class TableGen
    {
        public static void Main(string[] args)
        {
            var planetNumbers = new List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= 300; i += 5) planetNumbers.Add(i);

            SquareGrid.TableGen(planetNumbers, "XMLMods\\AhyangyiMaps\\_Code\\Tessellation\\SquareGridTable.cs");
        }
    }
}