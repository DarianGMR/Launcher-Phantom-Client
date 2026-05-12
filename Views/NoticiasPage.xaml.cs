using System;
using System.Windows.Controls;
using System.Diagnostics;

namespace LauncherPhantom.Views
{
    public partial class NoticiasPage : Page
    {
        public NoticiasPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NoticiasPage] Error en constructor: {ex.Message}");
            }
        }
    }
}