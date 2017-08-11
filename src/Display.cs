using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

class Display : Form {
    Bitmap frame;
    Console console;

    public Display(Console console) {
        Text = "Nes Emulator";
        Size = new Size(720, 486);
        ResizeRedraw = true;
        
        Paint += new PaintEventHandler(OnPaint);
        CenterToScreen();

        frame = new Bitmap(256, 240, PixelFormat.Format8bppIndexed);
        initPalette();

        this.console = console;
        console.drawAction = draw;

        Thread nesThread = new Thread(new ThreadStart(startNes));
        nesThread.IsBackground = true;

        nesThread.Start();
    }

    void initPalette() {
        ColorPalette palette = frame.Palette;
        palette.Entries[0x0] = Color.FromArgb(84,84,84);
        palette.Entries[0x1] = Color.FromArgb(0,30,116);
        palette.Entries[0x2] = Color.FromArgb(8,16,144);
        palette.Entries[0x3] = Color.FromArgb(48,0,136);
        palette.Entries[0x4] = Color.FromArgb(68,0,100);
        palette.Entries[0x5] = Color.FromArgb(92,0,48);
        palette.Entries[0x6] = Color.FromArgb(84,4,0);
        palette.Entries[0x7] = Color.FromArgb(60,24,0);
        palette.Entries[0x8] = Color.FromArgb(32,42,0);
        palette.Entries[0x9] = Color.FromArgb(8,58,0);
        palette.Entries[0xa] = Color.FromArgb(0,64,0);
        palette.Entries[0xb] = Color.FromArgb(0,60,0);
        palette.Entries[0xc] = Color.FromArgb(0,50,60);
        palette.Entries[0xd] = Color.FromArgb(0,0,0);
        palette.Entries[0xe] = Color.FromArgb(0,0,0);
        palette.Entries[0xf] = Color.FromArgb(0,0,0);
        palette.Entries[0x10] = Color.FromArgb(152,150,152);
        palette.Entries[0x11] = Color.FromArgb(8,76,196);
        palette.Entries[0x12] = Color.FromArgb(48,50,236);
        palette.Entries[0x13] = Color.FromArgb(92,30,228);
        palette.Entries[0x14] = Color.FromArgb(136,20,176);
        palette.Entries[0x15] = Color.FromArgb(160,20,100);
        palette.Entries[0x16] = Color.FromArgb(152,34,32);
        palette.Entries[0x17] = Color.FromArgb(120,60,0);
        palette.Entries[0x18] = Color.FromArgb(84,90,0);
        palette.Entries[0x19] = Color.FromArgb(40,114,0);
        palette.Entries[0x1a] = Color.FromArgb(8,124,0);
        palette.Entries[0x1b] = Color.FromArgb(0,118,40);
        palette.Entries[0x1c] = Color.FromArgb(0,102,120);
        palette.Entries[0x1d] = Color.FromArgb(0,0,0);
        palette.Entries[0x1e] = Color.FromArgb(0,0,0);
        palette.Entries[0x1f] = Color.FromArgb(0,0,0);
        palette.Entries[0x20] = Color.FromArgb(236,238,236);
        palette.Entries[0x21] = Color.FromArgb(76,154,236);
        palette.Entries[0x22] = Color.FromArgb(120,124,236);
        palette.Entries[0x23] = Color.FromArgb(176,98,236);
        palette.Entries[0x24] = Color.FromArgb(228,84,236);
        palette.Entries[0x25] = Color.FromArgb(236,88,180);
        palette.Entries[0x26] = Color.FromArgb(236,106,100);
        palette.Entries[0x27] = Color.FromArgb(212,136,32);
        palette.Entries[0x28] = Color.FromArgb(160,170,0);
        palette.Entries[0x29] = Color.FromArgb(116,196,0);
        palette.Entries[0x2a] = Color.FromArgb(76,208,32);
        palette.Entries[0x2b] = Color.FromArgb(56,204,108);
        palette.Entries[0x2c] = Color.FromArgb(56,180,204);
        palette.Entries[0x2d] = Color.FromArgb(60,60,60);
        palette.Entries[0x2e] = Color.FromArgb(0,0,0);
        palette.Entries[0x2f] = Color.FromArgb(0,0,0);
        palette.Entries[0x30] = Color.FromArgb(236,238,236);
        palette.Entries[0x31] = Color.FromArgb(168,204,236);
        palette.Entries[0x32] = Color.FromArgb(188,188,236);
        palette.Entries[0x33] = Color.FromArgb(212,178,236);
        palette.Entries[0x34] = Color.FromArgb(236,174,236);
        palette.Entries[0x35] = Color.FromArgb(236,174,212);
        palette.Entries[0x36] = Color.FromArgb(236,180,176);
        palette.Entries[0x37] = Color.FromArgb(228,196,144);
        palette.Entries[0x38] = Color.FromArgb(204,210,120);
        palette.Entries[0x39] = Color.FromArgb(180,222,120);
        palette.Entries[0x3a] = Color.FromArgb(168,226,144);
        palette.Entries[0x3b] = Color.FromArgb(152,226,180);
        palette.Entries[0x3c] = Color.FromArgb(160,214,228);
        palette.Entries[0x3d] = Color.FromArgb(160,162,160);
        palette.Entries[0x3e] = Color.FromArgb(0,0,0);
        palette.Entries[0x3f] = Color.FromArgb(0,0,0);
        
        frame.Palette = palette;
    }

    void startNes() {
        console.start();
    }

    unsafe void draw(byte[] screen) {
        BitmapData frameData = frame.LockBits(new Rectangle(0, 0, 256, 240), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        byte* ptr = (byte*) frameData.Scan0;
        for (int i=0;i<256*240;i++) {
            ptr[i] = screen[i];
        }
        frame.UnlockBits(frameData);

        Invalidate();
    }

    void OnPaint(object sender, PaintEventArgs e) {
        e.Graphics.DrawImage(frame, 0, 0, 720, 486);
    }
}