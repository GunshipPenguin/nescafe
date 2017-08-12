
public class Controller {
    public enum Button {
        A = 0,
        B,
        Select,
        Start,
        Up,
        Down,
        Left,
        Right
    };

    bool[] buttonStates;
    bool strobe;

    int buttonIndex;

    public Controller() {
        buttonStates = new bool[8];
        strobe = false;
    }

    public void setButtonState(Button button, bool state) {
        buttonStates[(int) button] = state;
    }

    public void writeControllerInput(byte input) {
        strobe = (input & 1) == 1;
        if (strobe) {
            buttonIndex = 0;
        }
    }

    public byte readControllerOutput() {
        // If out of buttons, return 1
        if (buttonIndex > 7) {
            return 1;
        }

        bool state = buttonStates[buttonIndex];

        if (!strobe) {
            buttonIndex ++;
        }

        return (byte) (state ? 1 : 0);
    }
}