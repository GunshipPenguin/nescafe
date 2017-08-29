namespace Nescafe
{
    /// <summary>
    /// Represents a NES controller.
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// Enumeration representing a button on the controller.
        /// </summary>
        public enum Button
        {
            /// <summary>
            /// the A button
            /// </summary>
            A = 0,
            /// <summary>
            /// the B button
            /// </summary>
            B,
            /// <summary>
            /// the select button
            /// </summary>
            Select,
            /// <summary>
            /// the start button
            /// </summary>
            Start,
            /// <summary>
            /// the up arrow button
            /// </summary>
            Up,
            /// <summary>
            /// the down arrow button
            /// </summary>
            Down,
            /// <summary>
            /// the left arrow button
            /// </summary>
            Left,
            /// <summary>
            /// the right arrow button
            /// </summary>
            Right
        };

        bool[] _buttonStates;
        bool _strobe;

        int _currButtonIndex;

        /// <summary>
        /// Construct a new controller.
        /// </summary>
        public Controller()
        {
            _buttonStates = new bool[8];
            _strobe = false;
        }

        /// <summary>
        /// Sets the state of a button (up/down).
        /// </summary>
        /// <param name="button">the button to set the state of</param>
        /// <param name="state"><c>true</c> if button is down, <c>false</c> if button is up</param>
        public void setButtonState(Button button, bool state)
        {
            _buttonStates[(int)button] = state;
        }

        /// <summary>
        /// Writes the specified byte to the controller input bus.
        /// </summary>
        /// <param name="input">the byte to write to the controller input bus</param>
        public void WriteControllerInput(byte input)
        {
            // 7  bit  0
            // --------
            // xxxx xxxS
            //         |
            //         +-Controller shift register strobe
            _strobe = (input & 1) == 1;
            if (_strobe) _currButtonIndex = 0;
        }

        /// <summary>
        /// Reads a byte from the controller output bus.
        /// </summary>
        /// <returns>the byte read from the controller output bus</returns>
        public byte ReadControllerOutput()
        {
            // 7  bit  0
            // --------
            // oooX XZXD
            // |||| ||||
            // |||| |||+- Serial controller data
            // |||+-+-+-- Always 0
            // |||   +--- Open bus on NES - 101 $4016; 0 otherwise
            // +++------- Open bus

            // If out of buttons, return 1
            if (_currButtonIndex > 7) return 1;

            bool state = _buttonStates[_currButtonIndex];
            if (!_strobe) _currButtonIndex++;

            return (byte)(state ? 1 : 0);
        }
    }
}
