using System.Windows.Controls;

namespace VoiceToText.App.Views;

public partial class TrayToolTip : UserControl
{
    public TrayToolTip()
    {
        InitializeComponent();
    }

    public void UpdateHotkey(string hotkeyText)
    {
        HotkeyText.Text = hotkeyText;
    }
}
