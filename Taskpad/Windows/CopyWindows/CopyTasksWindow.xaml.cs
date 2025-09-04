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
using Taskpad.Objects;
using Taskpad.Scripts;

namespace Taskpad.Windows.CopyWindows
{
    /// <summary>
    /// Interaction logic for CopyTasksWindow.xaml
    /// </summary>
    public partial class CopyTasksWindow : Page
    {
        List<ProjectObject>? fetched_projects = null;
        List<CheckBox> checked_tasks = new List<CheckBox>();
        private string ?project_name_string = null;
        private ProjectAdderWindow ?paw = null;
        private void PopulateProjects()
        {
            fetched_projects = DatabaseHandler.FetchProjects();

            if (fetched_projects == null || fetched_projects.Count <= 0)
                return;

            foreach (ProjectObject project in fetched_projects)
            {
                copy_project_combo.Items.Add(project.Name);
            }
            copy_project_combo.Text = fetched_projects[0].Name;
            copy_project_contents_btn.IsEnabled = true;
        }

        private List<TaskObject> ?FetchProjectTasks(string project_name)
        {
            if (fetched_projects == null)
                return null;

            foreach (ProjectObject project in fetched_projects)
            {
                if (project.Name == project_name)
                {
#pragma warning disable CS8604
                   return new List<TaskObject>(project.TaskList);
#pragma warning restore CS8604
                }
            }

            return new List<TaskObject>();
        }

        public CopyTasksWindow(string project_name, ProjectAdderWindow project_window)
        {
            InitializeComponent();
            PopulateProjects();
            project_name_string = project_name;
            paw = project_window;
        }

        private void confirm_btn_Click(object sender, RoutedEventArgs e)
        {
            List<TaskObject> tasks_to_copy = new List<TaskObject>();

            foreach (CheckBox checkbox in checked_tasks)
            {
                if (checkbox.IsChecked == true)
                {
                    tasks_to_copy.Add((TaskObject)checkbox.Tag);
                }
            }
            
            if (tasks_to_copy.Count > 0 && !string.IsNullOrEmpty(project_name_string))
            {
                DatabaseHandler.SaveProject(project_name_string, tasks_to_copy);
                if (paw != null)
                    paw.Close();
            }
            else
            {
                MessageBox.Show("Please select 1 or more tasks to copy!", "No tasks selected!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void copy_project_contents_btn_Click(object sender, RoutedEventArgs e)
        {
            string selected_project_name = copy_project_combo.Text;
            List<TaskObject>? fetched_tasks = FetchProjectTasks(selected_project_name);

            if (imported_tasks_list.Items.Count > 0)
            {
                if (MessageBox.Show("You currently have previously opened tasks, would you like to overwrite them?", "Overwrite currently opened tasks...", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    imported_tasks_list.Items.Clear();
                }
                else
                {
                    return;
                }
            }
            if (fetched_tasks != null && fetched_tasks.Count > 0)
            {

                foreach (TaskObject task in fetched_tasks)
                {
                    StackPanel row_panel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    CheckBox box = new CheckBox
                    {
                        IsChecked = true,
                        Content = "",
                        Width = 50,
                        Tag = task
                    };
                    box.Click += Box_Click;

                    row_panel.Children.Add(box);
                    row_panel.Children.Add(new TextBlock { Text = task.Name, Width = 75 });
                    row_panel.Children.Add(new TextBlock { Text = task.Priority.ToString(), Width = 60 });
                    row_panel.Children.Add(new TextBlock { Text = task.DueDate, Width = 45 });
                    row_panel.Children.Add(new TextBlock { Text = task.Completed.ToString().ToLower(), Width = 30 });

                    imported_tasks_list.Items.Add(row_panel);
                    checked_tasks.Add(box);
                }
                select_all_btn.IsEnabled = true;
                deselect_all_btn.IsEnabled = true;
                confirm_btn.IsEnabled = true;
            }
            else
            {
                select_all_btn.IsEnabled = false;
                deselect_all_btn.IsEnabled = false;
                confirm_btn.IsEnabled = false;
            }
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ?box = e.Source as CheckBox;

            if (box != null)
            {
                if (box.IsChecked == true && !checked_tasks.Contains(box))
                {
                    checked_tasks.Add(box);
                }
                else if (box.IsChecked == false && checked_tasks.Contains(box))
                {
                    checked_tasks.Remove(box);
                }
            }

            if (checked_tasks.Count > 0)
            {
                confirm_btn.IsEnabled = true;
            }
            else
            {
                confirm_btn.IsEnabled = false;
            }
        }

        private void select_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel stack in imported_tasks_list.Items)
            {
                CheckBox ?checkBox = stack.Children.Cast<CheckBox>().FirstOrDefault();
                if (checkBox == null)
                    return;
                checkBox.IsChecked = true;
                if (!checked_tasks.Contains(checkBox))
                {
                    checked_tasks.Add(checkBox);
                }
            }
            confirm_btn.IsEnabled = true;
        }

        private void deselect_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel stack in imported_tasks_list.Items)
            {
                CheckBox? checkbox = stack.Children.Cast<CheckBox>().FirstOrDefault();
                if (checkbox == null)
                    return;
                checkbox.IsChecked = false;
            }
            checked_tasks.Clear();
            confirm_btn.IsEnabled = false;
        }

        private void copy_project_combo_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            copy_project_contents_btn.IsEnabled = (string.IsNullOrEmpty(copy_project_combo.Text)) ? false : true;
        }
    }
}