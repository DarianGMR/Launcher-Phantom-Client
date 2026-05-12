using System;
using System.Windows.Controls;
using System.Diagnostics;

namespace LauncherPhantom.Views
{
    public partial class BibliotecaPage : Page
    {
        public BibliotecaPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BibliotecaPage] Error en constructor: {ex.Message}");
            }
        }
    }
}