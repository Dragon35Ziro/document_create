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
using System.Windows.Shapes;

namespace PTH
{
    /// <summary>
    /// Логика взаимодействия для Progres.xaml
    /// </summary>
    public partial class Progres : Window
    {

 
        public Progres(string mesto, string project)
        {
            InitializeComponent();
            gogogo(mesto, project);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow input = new MainWindow();
            input.Show();
        }

        async void gogogo(string project, string result)
        {
            AImoment ai = new AImoment();
            Directory.CreateDirectory(result + @"\CA_21");
            Directory.CreateDirectory(result + @"\CA_22");
            Directory.CreateDirectory(result + @"\CA_23");
            Directory.CreateDirectory(result + @"\CA_24");

            await ai.Main(project, result, prBar);
           

        }
    }
}
