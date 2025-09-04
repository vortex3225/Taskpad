using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Taskpad.Objects;
using Taskpad.Scripts;

namespace Taskpad.Windows
{
    /// <summary>
    /// Interaction logic for ProjectSelectorWindow.xaml
    /// </summary>
    public partial class ProjectSelectorWindow : Window
    {
        private ProjectObject ?selected_project = null;
        private bool changed = false;
        private bool delete_project_mode = false;
        public ProjectSelectorWindow(bool delete_project = false)
        {
            InitializeComponent();
            delete_project_mode = delete_project;
            if (delete_project_mode)
            {
                accept_btn.Content = "Delete project";
                accept_btn.Background = Brushes.Red;
                this.Title = "Delete a project";
            }
            List<ProjectObject> ?projects = DatabaseHandler.FetchProjects();
            if (projects != null)
            {
                foreach (ProjectObject project in projects)
                {
                    project_combo.Items.Add(project.Name);
                }
                project_combo.Text = projects[0].Name;
                selected_project = projects[0];
            }
        }

        private void project_combo_DropDownClosed(object sender, EventArgs e)
        {
            if (selected_project == null || project_combo.Text != selected_project.Name)
            {
                selected_project = DatabaseHandler.FetchProject(project_combo.Text);
                changed = true;
            }
        }

        private void accept_btn_Click(object sender, RoutedEventArgs e)
        {
            if (delete_project_mode && MessageBox.Show("Are you sure you wish to delete the following project: {selected_project.Name}?\nThis action cannot be reversed!", $"Deleting project {selected_project.Name}", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DatabaseHandler.DeleteProject(project: selected_project);
            }
            else if (!delete_project_mode)
            {
                MainWindow.current_project = selected_project;
            }
            this.DialogResult = true;
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.current_project = selected_project;
            this.DialogResult = true;
            this.Close();
        }
    }
}
