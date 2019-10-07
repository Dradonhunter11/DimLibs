using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Dimlibs.API.DimensionHandler
{
    public sealed partial class DimensionHandler
    {
        public static bool FreezeWorldUpdate = false;

        private Chest[] _chest;
        private readonly String _Path = "";
        private readonly int maxTileX;
        private readonly int maxTileY;
        private readonly int spawnX;
        private readonly int spawnY;

        private readonly float bottomWorld;
        private readonly float topWorld;
        private readonly float leftWorld;
        private readonly float rightWorld;

	    public string ActiveDimensionName => Dimlibs.dimension;

        public bool loading = false;

        public DimensionHandler(String name)
        {
        }	
    }
}
