using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Terraria;

namespace Dimlibs.Chunks
{
    /// <summary>
    /// A chunk of tiles in the world
    /// </summary>
    [Serializable]
	public class Chunk : ISerializable
	{
		/// <summary>
		/// The size of every chunk
		/// </summary>
		public const int ChunkSize = 150;

		public bool loaded = false;

		/// <summary>
		/// The list of tiles
		/// </summary>
		private readonly List2D<Tile> _tiles;

		/// <summary>
		/// Initializes a new instance of this class
		/// </summary>
		public Chunk()
		{
			_tiles = new List2D<Tile>();
		}

		private void instantiateNewChunk()
		{
			for (int i = 0; i < ChunkSize; i++)
			{
				for (int j = 0; j < ChunkSize; j++)
				{
					_tiles[i, j] = new Tile();
				}
			}
		}

		/// <summary>
		/// Get the tile at the specified position
		/// </summary>
		/// <param name="pos">Position of the tile to get</param>
		/// <returns>The tile at the specified position</returns>
		public Tile this[Vector2 pos]
		{
			get => this[new Position2I(pos)];
			set => this[new Position2I(pos)] = value;
		}

		/// <summary>
		/// Get the tile at the specified position
		/// </summary>
		/// <param name="pos">Position of the tile to get</param>
		/// <returns>The tile at the specified position</returns>
		public Tile this[Position2I pos]
		{
			get => this[pos.X, pos.Y];
			set => this[pos.X, pos.Y] = value;
		}

		/// <summary>
		/// Get the tile at the specified position
		/// </summary>
		/// <param name="x">X position of the tile</param>
		/// <param name="y">Y position of the tile</param>
		/// <returns>The tile at the specified position</returns>
		public Tile this[int x, int y]
		{
			get => _tiles[x, y];
			set => _tiles[x, y] = value;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("tile", _tiles);
		}
	}
}
