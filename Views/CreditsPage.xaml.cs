using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace LauncherPhantom.Views
{
    public partial class CreditsPage : Page
    {
        public CreditsPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreditsPage] Error en constructor: {ex.Message}");
            }
        }
    }
}