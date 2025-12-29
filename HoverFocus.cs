using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

public class HoverFocusWindow : Form
{
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_TOOLWINDOW = 0x00000080;

    private IntPtr targetWindow = IntPtr.Zero;
    private Label statusLabel;
    private Button captureButton;
    private bool capturing = false;

    public HoverFocusWindow()
    {
        this.Text = "HoverFocus";
        this.Size = new Size(200, 80);
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.TopMost = true;
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(10, 10);

        // Hide from alt-tab
        int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
        SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW);

        captureButton = new Button();
        captureButton.Text = "Click, then target window";
        captureButton.Location = new Point(10, 10);
        captureButton.Size = new Size(175, 25);
        captureButton.Click += CaptureButton_Click;
        this.Controls.Add(captureButton);

        statusLabel = new Label();
        statusLabel.Text = "No target";
        statusLabel.Location = new Point(10, 40);
        statusLabel.Size = new Size(175, 20);
        statusLabel.ForeColor = Color.Gray;
        this.Controls.Add(statusLabel);

        this.MouseEnter += OnMouseEnter;
        captureButton.MouseEnter += OnMouseEnter;
        statusLabel.MouseEnter += OnMouseEnter;

        // Global hook for capture
        Timer captureTimer = new Timer();
        captureTimer.Interval = 50;
        captureTimer.Tick += CaptureTimer_Tick;
        captureTimer.Start();
    }

    private void CaptureButton_Click(object sender, EventArgs e)
    {
        capturing = true;
        captureButton.Text = "Click target window...";
        captureButton.Enabled = false;
    }

    private void CaptureTimer_Tick(object sender, EventArgs e)
    {
        if (!capturing) return;

        if ((Control.MouseButtons & MouseButtons.Left) != 0)
        {
            IntPtr fg = GetForegroundWindow();
            if (fg != IntPtr.Zero && fg != this.Handle)
            {
                targetWindow = fg;
                string title = GetWindowTitle(fg);
                if (title.Length > 20) title = title.Substring(0, 20) + "...";
                statusLabel.Text = title;
                statusLabel.ForeColor = Color.DarkGreen;
                capturing = false;
                captureButton.Text = "Click to retarget";
                captureButton.Enabled = true;
            }
        }
    }

    private void OnMouseEnter(object sender, EventArgs e)
    {
        if (targetWindow != IntPtr.Zero && !capturing)
        {
            SetForegroundWindow(targetWindow);
        }
    }

    private string GetWindowTitle(IntPtr hWnd)
    {
        var sb = new System.Text.StringBuilder(256);
        GetWindowText(hWnd, sb, 256);
        return sb.ToString();
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new HoverFocusWindow());
    }
}
