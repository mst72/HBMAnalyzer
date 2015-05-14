using System;
using System.ComponentModel;
using System.Windows;

namespace ElasticView.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            Loaded += OnLoaded;
            Closed += OnClose;
        }

        private void OnCloseRequest(object sender, EventArgs e)
        {
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            // OnLoad(dockManager);
        }
        private void OnClose(object sender, EventArgs e)
        {
            Closed -= OnClose;
            // OnSave(dockManager);
        }

    }
}
