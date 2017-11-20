using System;

namespace AlkaronEngine.Input
{
    public enum KeyEventType
    {
        /// <summary>
        /// Key was just pressed this frame
        /// </summary>
        Pressed,
        /// <summary>
        /// Key was pressed in the past and is still held down
        /// </summary>
        Down,
        /// <summary>
        /// Key was released in this frame
        /// </summary>
        Released
    }
}
