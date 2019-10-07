using Dimlibs.API;
using Dimlibs.API.DimensionHandler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace Dimlibs.UI
{
    class UIDimensionLoading : UIState
    {
        private DimensionHandler currentHandler;

        public UIDimensionLoading(DimensionHandler currentHandlerToLoad)
        {
            currentHandler = currentHandlerToLoad;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Viewport dimension = Main.graphics.GraphicsDevice.Viewport;
            Texture2D texture = Dimlibs.Instance.GetTexture("Texture/LoadingScreen3");
            Texture2D logo = Dimlibs.Instance.GetTexture("Texture/TerrariaLogo");
            //if (!currentHandler.generator.DrawCustomBackground(spriteBatch))
            {
                for (int i = 0; i < dimension.Width; i += texture.Width)
                {
                    for (int j = 0; j < dimension.Height; j += texture.Height)
                    {
                        Main.spriteBatch.Draw(texture, new Rectangle(i, j, texture.Width, texture.Height), null, Color.White, 0f,
                            Vector2.Zero, SpriteEffects.None, 0f);
                    }
                }

                Vector2 position = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) -
                                   Main.fontDeathText.MeasureString(Main.statusText) / 2;
                Utils.DrawBorderStringFourWay(spriteBatch, Main.fontDeathText, Main.statusText, position.X, position.Y,
                    Color.White, Color.Black, Vector2.Zero, 0.75f);
                Vector2 logoDrawingPosition = new Vector2();
            }
        }
    }
}
