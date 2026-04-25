using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    public class Button
    {
        public Vector2 RelativePosition; // (0–1), střed tlačítka
        public Vector2 Size;             // v pixelech
        public string Text;

        public Color NormalColor = Color.White;
        public Color HoverColor = Color.Yellow;

        public Action? OnClick;

        private bool isHovering;
        private ButtonState prevState;

        public Rectangle GetBounds(Viewport viewport)
        {
            int x = (int)(RelativePosition.X * viewport.Width - Size.X / 2 * Game1.SpriteScale);
            int y = (int)(RelativePosition.Y * viewport.Height - Size.Y / 2 * Game1.SpriteScale);

            return new Rectangle(x, y, (int)(Size.X * Game1.SpriteScale), (int)(Size.Y * Game1.SpriteScale));
        }

        public void Update(MouseState mouse, Viewport viewport)
        {
            var bounds = GetBounds(viewport);

            isHovering = bounds.Contains(mouse.Position);

            if (isHovering &&
                mouse.LeftButton == ButtonState.Released &&
                prevState == ButtonState.Pressed)
            {
                OnClick?.Invoke();
            }

            prevState = mouse.LeftButton;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Viewport viewport)
        {
            var bounds = GetBounds(viewport);
            var color = isHovering ? HoverColor : NormalColor;

            Color BackGroundColor = new Color(50, 50, 50);

            spriteBatch.Draw(Game1.Pixel, bounds, BackGroundColor);

            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X * Game1.SpriteScale) / 2,
                bounds.Y + (bounds.Height - textSize.Y * Game1.SpriteScale) / 2
            );

            spriteBatch.DrawString(font, Text, textPos, color, 0f, Vector2.Zero, Game1.SpriteScale, SpriteEffects.None, 0f);
        }
    }
}
