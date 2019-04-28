using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixSharp;
using System.Diagnostics;
using ANTLRStudio.WixSharpInstaller;

public partial class CustomDialog : WixCLRDialog
{
    public CustomDialog()
    {
        InitializeComponent();
    }

    public CustomDialog(Session session)
        : base(session)
    {
        InitializeComponent();
    }

    void backBtn_Click(object sender, EventArgs e)
    {
        MSIBack();

    }

    void nextBtn_Click(object sender, EventArgs e)
    {
        MSINext();
    }

    void cancelBtn_Click(object sender, EventArgs e)
    {
        MSICancel();
    }


    private void WPFRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        Program.GraphicsBackend = "Eto.Wpf.dll";
    }

    private void WinFormsRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        Program.GraphicsBackend = "Eto.Windows.dll";
    }
}
