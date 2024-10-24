using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;

namespace ChessChallenge.Application
{
    class Dropdown<T>
    {
        private List<T> options;
        public bool dropdownOpen = false;
        private int dropdownWidth;
        private int dropdownHeight;
        private int dropdownX;
        private int dropdownY;

        public Dropdown(List<T> options, Vector2 position, Vector2 size)
        {
            this.options = options;
            dropdownX = (int)position.X;
            dropdownY = (int)position.Y;
            dropdownWidth = (int)size.X;
            dropdownHeight = (int)size.Y;
        }

        public void Draw(ref int selectedOption, ref bool dropdownOpen)
        {
            this.dropdownOpen = dropdownOpen;
            Vector2 mousePos = Raylib.GetMousePosition();

            // Check if the dropdown is clicked
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                // Toggle dropdown open/close if clicked on dropdown header
                if (Raylib.CheckCollisionPointRec(mousePos, new Rectangle(dropdownX, dropdownY, dropdownWidth, dropdownHeight)))
                {
                    dropdownOpen = !dropdownOpen;
                }
                else if (dropdownOpen) 
                {
                    // Check if clicking on an option
                    for (int i = 0; i < options.Count; i++)
                    {
                        Rectangle optionRect = new Rectangle(dropdownX, dropdownY + dropdownHeight * (i + 1), dropdownWidth, dropdownHeight);
                        if (Raylib.CheckCollisionPointRec(mousePos, optionRect))
                        {
                            selectedOption = i;
                            dropdownOpen = false;
                        }
                    }
                }
            }

            // Draw the dropdown header
            Raylib.DrawRectangle(dropdownX, dropdownY, dropdownWidth, dropdownHeight, Color.DARKGRAY);
            Raylib.DrawText(options[selectedOption].ToString(), dropdownX + 5, dropdownY + 5, 20, Color.WHITE);

            // Draw the dropdown options if open
            if (dropdownOpen)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    Rectangle optionRect = new Rectangle(dropdownX, dropdownY + dropdownHeight * (i + 1), dropdownWidth, dropdownHeight);
                    Color optionColor = Raylib.CheckCollisionPointRec(mousePos, optionRect) ? Color.LIGHTGRAY : Color.GRAY;
                    Raylib.DrawRectangleRec(optionRect, optionColor);
                    Raylib.DrawText(options[i].ToString(), dropdownX + 5, dropdownY + dropdownHeight * (i + 1) + 5, 20, Color.WHITE);
                }
            }
        }
    }
}