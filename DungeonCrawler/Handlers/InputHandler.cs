using DungeonCrawler.Actions;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Handlers
{
    class InputHandler
    {
        static Dictionary<Mouse.Button, bool> buttonPressed = new Dictionary<Mouse.Button, bool>();
        static Dictionary<Mouse.Button, bool> buttonDown = new Dictionary<Mouse.Button, bool>();

        public static void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            Vector2f mousePos = Game.app.MapPixelToCoords(new Vector2i(e.X,e.Y));
            Vector2f relPos = mousePos - Game.player.rect.Position;
            float angle = (float)Math.Atan2(relPos.Y, relPos.X);
            Game.player.direction = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
            //Game.player.direction = relPos;
        }

        public static void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if(!buttonDown.ContainsKey(e.Button)) buttonDown.Add(e.Button, true);
            if(!buttonPressed.ContainsKey(e.Button)) buttonPressed.Add(e.Button, true);
            buttonDown[e.Button] = true;
            buttonPressed[e.Button] = true;
        }

        public static void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            buttonDown[e.Button] = false;
            if (buttonPressed.ContainsKey(e.Button)) buttonPressed[e.Button] = false;
        }

        public static bool MouseButtonPressed(Mouse.Button button)
        {
            if (!buttonDown.ContainsKey(button)) buttonDown.Add(button, true);
            if (!buttonPressed.ContainsKey(button)) buttonPressed.Add(button, true);
            bool pressed = buttonPressed[button];
            if (pressed) buttonPressed[button] = false;
            return pressed;
        }

        public static bool MouseButtonDown(Mouse.Button button)
        {
            if (!buttonDown.ContainsKey(button)) buttonDown.Add(button, true);
            if (!buttonPressed.ContainsKey(button)) buttonPressed.Add(button, true);
            return buttonDown[button];
        }
    }
}
