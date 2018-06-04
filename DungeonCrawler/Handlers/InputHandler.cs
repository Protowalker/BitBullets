using DungeonCrawler.Actions;
using DungeonCrawler.States;
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
            if (!buttonDown.ContainsKey(button)) buttonDown.Add(button, Mouse.IsButtonPressed(button));
            if (!buttonPressed.ContainsKey(button)) buttonPressed.Add(button, Mouse.IsButtonPressed(button));
            return buttonDown[button];
        }
    }
}
