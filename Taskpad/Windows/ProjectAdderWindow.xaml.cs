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
using Taskpad.Scripts;

namespace Taskpad.Windows
{
    /// <summary>
    /// Interaction logic for ProjectAdderWindow.xaml
    /// </summary>
    public partial class ProjectAdderWindow : Window
    {
        public ProjectAdderWindow()
        {
            InitializeComponent();
        }

        public string GetName()
        {
            return project_namebox.Text;
        }

        private void copy_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            if (copy_radio_btn.IsChecked == true)
            {
                if (err_display.Visibility == Visibility.Visible)
                    return;

                task_adder_frame.Navigate(new CopyWindows.CopyTasksWindow(project_namebox.Text, this));
            }
        }

        private void new_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            if (new_radio_btn.IsChecked == true)
            {
                task_adder_frame.Navigate(new CopyWindows.CreateTasksWindows(project_namebox.Text, this));
            }
        }

        private void project_namebox_TextChanged(object sender, TextChangedEventArgs e)
        {
           if (string.IsNullOrEmpty(project_namebox.Text))
            {
                err_display.Visibility = Visibility.Visible;
                err_display.Text = "Name cannot be empty!";
            }
           else if (Utility.DoesNameAlreadyExist(project_namebox.Text))
            {
                err_display.Visibility = Visibility.Visible;
                err_display.Text = "Name already exists! Chose a different name!";
            }
           else
            {
                err_display.Visibility = Visibility.Hidden;
                err_display.Text = string.Empty;
                if (task_adder_frame.Content == null)
                    task_adder_frame.Navigate(new CopyWindows.CreateTasksWindows(project_namebox.Text, this));
            }
        }

        private void err_display_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (err_display.Visibility == Visibility.Visible)
                task_adder_frame.Content = null;
        }
    }
}
