using System;
using System.Collections.Generic;
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
using System.Data;

namespace Concept_Testing
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {                                     
        const string ConnString = "server=################;Database=Concept_Testing;User Id=##########; Password=############;";
        public MainWindow()
        {
            try
            {
                InitializeComponent();


                DAL.DAL_SQL.Inst.InitConnStr(ConnString);

                DataTable dt = DAL.DAL_SQL.Inst.GetTable("Select * from INFORMATION_SCHEMA.Tables");
                if (1==1)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }

        }
    }
}
