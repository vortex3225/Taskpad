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
using System.Windows.Shapes;
using Taskpad.Objects;

namespace Taskpad.Windows.CopyWindows
{
    /// <summary>
    /// Interaction logic for CopyPageDisplayWindow.xaml
    /// </summary>
    public partial class CopyPageDisplayWindow : Window
    {
        private static MainWindow ?main_wind_inst = null;
        public List<TaskObject> new_tasks = new List<TaskObject>();
        private string[] ?arguments = null;
        public CopyPageDisplayWindow(MainWindow main_window_instance, string[] ?args = null)
        {
            InitializeComponent();
            main_wind_inst = main_window_instance;
            arguments = args;
            this.Closing += CopyPageDisplayWindow_Closing;
        }
        private void CopyPageDisplayWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            /*

             Currently valid arguments:

             dont_delete_if_empty --> wont refresh the list if the new_tasks is empty
             is_editing_flag --> if the window was opened with the Edit Task menu button, it wont refresh the list.
             
             */

            if (main_wind_inst == null)
                return;

            if (arguments != null)
            {
                if (arguments.Contains("dont_delete_if_empty") && new_tasks.Count == 0) { return; }
                else if (arguments.Contains("is_editing_flag")) { return; }
            }

            main_wind_inst.RefreshTaskList(new_tasks);
        }

        public void SetContent(object window_content)
        {
            display_frame.Navigate(window_content);
        }
    }
}
