using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.UI;

namespace Dimlibs.UI
{
    class UINetworkConnection : UIState
    {
        private static string _message;

        public static string Message
        {
            get => _message;
            set => _message = value;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 textSize = Main.fontDeathText.MeasureString(Message);
            Vector2 drawingPos = new Vector2(Main.screenWidth / 2 - textSize.X / 2, Main.screenHeight / 2 - textSize.Y / 2);
            spriteBatch.DrawString(Main.fontDeathText, Message, drawingPos, Color.White);
        }

        
    }
}
