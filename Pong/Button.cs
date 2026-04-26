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
        public Vector2 RelativePosition; // relative position of the button's center (0-1)
        public Vector2 Size;             // size in pixels
        public string Text;

        public Color NormalTextColor = Color.White;
        public Color HoverTextColor = Color.Yellow;
        public Color NormalBackGroundColor = new Color(50, 50, 50); // sets default colors of button
        public Color HoverBackGroundColor = new Color(70, 70, 70); // sets hover background color

#nullable enable
        public Action? OnClick; // event that gets called when the button is clicked, can be null if no action is needed

        private bool isHovering;
        private ButtonState prevState;
        /// <summary>
        /// Method that gets the rectangle around button
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public Rectangle GetBounds(Viewport viewport)
        {
            int x = (int)(RelativePosition.X * viewport.Width - Size.X / 2 * Game1.SpriteScale);
            int y = (int)(RelativePosition.Y * viewport.Height - Size.Y / 2 * Game1.SpriteScale);

            return new Rectangle(x, y, (int)(Size.X * Game1.SpriteScale), (int)(Size.Y * Game1.SpriteScale));
        }
        /// <summary>
        /// Method that checks for mouse input and updates button state, also invokes click event if the button was clicked
        /// </summary>
        /// <param name="mouse"></param>
        /// <param name="viewport"></param>
        public void Update(MouseState mouse, Viewport viewport)
        {
            var bounds = GetBounds(viewport);

            isHovering = bounds.Contains(mouse.Position);

            if (isHovering &&
                mouse.LeftButton == ButtonState.Released &&
                prevState == ButtonState.Pressed) // checks if the button was just released while hovering
            {
                OnClick?.Invoke(); // invokes the click event if its not null
            }

            prevState = mouse.LeftButton;
        }
        /// <summary>
        /// Method that draws the button, changes text color if hovering and centers the text on the button
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="viewport"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Viewport viewport)
        {
            var bounds = GetBounds(viewport); // gets rectangle where the button is drawn
            Color textColor = NormalTextColor; ;
            Color backGroundColor = NormalBackGroundColor;

            if (isHovering) // changes color if hovering
            {
                textColor = HoverTextColor;
                backGroundColor = HoverBackGroundColor;
            }
            
            spriteBatch.Draw(Game1.Pixel, bounds, backGroundColor); // draws the button background

            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X * Game1.SpriteScale) / 2, // draws button text centered on the button
                bounds.Y + (bounds.Height - textSize.Y * Game1.SpriteScale) / 2 // also accounts for sprite scale
            );

            spriteBatch.DrawString(
                font, 
                Text, 
                textPos, 
                textColor, 
                0f, 
                Vector2.Zero, 
                Game1.SpriteScale, 
                SpriteEffects.None, 
                0f);
        }
    }
}
