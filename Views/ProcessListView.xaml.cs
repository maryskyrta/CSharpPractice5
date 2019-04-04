using System.Windows.Controls;
using CSharpPractice5.ViewModels;

namespace CSharpPractice5.Views
{
    /// <summary>
    /// Логика взаимодействия для ProcessListView.xaml
    /// </summary>
    public partial class ProcessListView : UserControl
    {
        public ProcessListView()
        {
            InitializeComponent();
            DataContext = new ProcessListViewModel();
        }
    }
}
