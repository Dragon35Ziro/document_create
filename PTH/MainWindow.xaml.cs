using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;


namespace PTH
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();



            openFileDialog.ShowDialog();


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                project.Text = openFileDialog.SelectedPath;
                
            }
                
        }

        private void result_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            openFileDialog.ShowDialog();


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                result.Text = openFileDialog.SelectedPath;
            }
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Progres input = new Progres(project.Text , result.Text);
            input.Show();

            

            
        }
    }
}
