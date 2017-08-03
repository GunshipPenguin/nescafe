using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

class Display : Form {
    Bitmap currBitmap;
    Console console;

    public Display(Console console) {
        Text = "Nes Emulator";
        Size = new Size(720, 486);
        ResizeRedraw = true;
        
        Paint += new PaintEventHandler(OnPaint);
        CenterToScreen();

        this.console = console;
        console.drawAction = drawBitmap;

        Thread nesThread = new Thread(new ThreadStart(startNes));
        nesThread.IsBackground = true;

        nesThread.Start();
    }

    void startNes() {
        console.start();
    }

    void drawBitmap(Bitmap bitmap) {
        currBitmap = bitmap;
        Invalidate();
    }

    void OnPaint(object sender, PaintEventArgs e) {
        if (currBitmap != null) {
            e.Graphics.DrawImage(currBitmap, 0, 0, 720, 486);
        }
    }
}