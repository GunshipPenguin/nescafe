
public class Controller
{
    public enum Button
    {
        A = 0,
        B,
        Select,
        Start,
        Up,
        Down,
        Left,
        Right
    };

    bool[] _buttonStates;
    bool _strobe;

    int _currButtonIndex;

    public Controller()
    {
        _buttonStates = new bool[8];
        _strobe = false;
    }

    public void setButtonState(Button button, bool state)
    {
        _buttonStates[(int)button] = state;
    }

    public void WriteControllerInput(byte input)
    {
        _strobe = (input & 1) == 1;
        if (_strobe) _currButtonIndex = 0;
    }

    public byte ReadControllerOutput()
    {
        // If out of buttons, return 1
        if (_currButtonIndex > 7) return 1;

        bool state = _buttonStates[_currButtonIndex];
        if (!_strobe) _currButtonIndex++;

        return (byte)(state ? 1 : 0);
    }
}