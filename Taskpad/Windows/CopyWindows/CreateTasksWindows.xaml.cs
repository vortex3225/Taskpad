using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using static System.Net.Mime.MediaTypeNames;

namespace Taskpad.Windows.CopyWindows
{
    /// <summary>
    /// Interaction logic for CreateTasksWindows.xaml
    /// </summary>
    /// 

    public class ListTaskObject
    {
        public string Name { get; set; }
        public string Priority { get; set; }
        public string DueDate { get; set; }
    }
    public partial class CreateTasksWindows : Page
    {
        private TaskObject currentTask = new TaskObject
        {
            DueDate = "none"
        };
        private List<TaskObject> created_tasks = new List<TaskObject>();
        private List<TaskObject> newly_added_tasks = new List<TaskObject>();
        private string ?project_n = null;
        private object ?parent_window = null;
        private MainWindow ?main_window_object = null; // only used for editing
        private string mode = "project";
        private string ?original_name = null;

        private void InitPriorities()
        {
            foreach (TaskPriority priority in Enum.GetValues(typeof(TaskPriority)))
            {
                priority_combo.Items.Add(priority);
            }

            priority_combo.Text = priority_combo.Items[0].ToString();
        }

        private string[] GetMonthsAsArray()
        {
            string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            return months;
        }
        private int GetMonthCountFromString(string month)
        {
            string[] months = GetMonthsAsArray();
            for (int i = 0; i < 12; i++)
            {
                if (months[i] == month)
                {
                    return i;
                }
            }
            return 0;
        }
        private void InitDateComboboxes()
        {
            // day box init
            for (int i = 1; i <= 31; i++)
            {
                day_combo.Items.Add(i);
            }
            
            // month box init

            string[] months = GetMonthsAsArray();

            foreach (string month in months)
            {
                month_combo.Items.Add(month);
            }
            day_combo.Text = "1"; 
            month_combo.Text = months[0];  
        }

        private void UpdateDayBoxDependingOnMonth(string selectedMonth)
        {
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, (GetMonthCountFromString(selectedMonth) + 1));

            int current_days = day_combo.Items.Count;
            if (current_days < daysInMonth)
            {
                int diff = daysInMonth - current_days;
                for (int i = ++current_days; i <= daysInMonth; i++)
                {
                    day_combo.Items.Add(i);
                }
            }
            else if (current_days > daysInMonth)
            {
                for (int i = current_days; i > daysInMonth; i--)
                {
                    day_combo.Items.Remove(i);
                }
                day_combo.Text = daysInMonth.ToString();
            }
        }

        private string ReturnDueDate()
        { // todo implement some settings which can determine if the month is fully outputed or just the first 3 letters/4 letters
            string day = day_combo.Text;
            string month = month_combo.Text;
            return $"{day} {month}";
        }

        public CreateTasksWindows(string project_name, object p_window, string creation_mode = "project", string task_name = "", bool is_edit_mode = false, TaskObject ?task_to_edit = null, MainWindow ?mw = null) // creation_mode --> if the create_btn creates a new project or a new task
        {
            InitializeComponent();
            InitPriorities();
            InitDateComboboxes();
            project_n = project_name;
            parent_window = p_window;
            mode = creation_mode;
            main_window_object = mw;
            if (mode != "project")
            {
                create_btn.Content = "Update Task List";
                search_panel.Visibility = Visibility.Hidden;
                ProjectObject ?project = DatabaseHandler.FetchProject(project_name);
                if (project != null && !is_edit_mode)
                {
                    foreach (TaskObject task in project.TaskList)
                    {
                        created_tasks.Add(task);
                        added_tasks_list.Items.Add(new TaskObject { Name = task.Name, DueDate = task.DueDate, Priority = task.Priority });
                    }
                }
                else if (is_edit_mode && task_to_edit != null)
                {
                    created_tasks.Add(task_to_edit);
                    search_panel.Visibility = Visibility.Hidden;
                    task_namebox.Text = task_to_edit.Name;
                    if (task_to_edit.DueDate != "none")
                    {
                        yes_due_date_radio.IsChecked = true;
                        date_selector_panel.Visibility = Visibility.Visible;
                        string[] date_comps = task_to_edit.DueDate.Split(" ");
                        day_combo.Text = date_comps[0];
                        month_combo.Text = date_comps[1];
                    }
                    priority_combo.Text = task_to_edit.Priority.ToString();
                    create_btn.Content = "Update task";
                    currentTask = new TaskObject
                    {
                        Name = task_to_edit.Name,
                        Completed = task_to_edit.Completed,
                        DueDate = task_to_edit.DueDate,
                        Priority = task_to_edit.Priority,
                    };
                    add_task_btn.Visibility = Visibility.Hidden;
                    create_btn.IsEnabled = true;
                    original_name = task_to_edit.Name;
                }
                if (task_name != string.Empty)
                {
                    task_namebox.Text = task_name;
                }
            }
        }

        private void yes_due_date_radio_Click(object sender, RoutedEventArgs e)
        {
            if (yes_due_date_radio.IsChecked == true)
            {
                date_selector_panel.Visibility = Visibility.Visible;
            }
        }

        private void no_due_date_radio_Click(object sender, RoutedEventArgs e)
        {
            if (no_due_date_radio.IsChecked == true)
            {
                date_selector_panel.Visibility = Visibility.Collapsed;
                currentTask.DueDate = "none";
            }
        }

        private void set_prior_btn_Click(object sender, RoutedEventArgs e)
        {
            currentTask.Priority = Enum.Parse<TaskPriority>(priority_combo.Text);
        }

        private void set_due_btn_Click(object sender, RoutedEventArgs e)
        {
            currentTask.DueDate = ReturnDueDate();
        }

        private bool DoesTaskAlreadyExist(string task)
        {
            foreach (TaskObject already_created in created_tasks)
            {
                if (already_created.Name == task)
                {
                    return true;
                }
            }

            return false;
        }

        private void Reset()
        {
            currentTask = new TaskObject
            {
                DueDate = "none"
            };
            no_due_date_radio.IsChecked = true;
            date_selector_panel.Visibility = Visibility.Collapsed;
            task_namebox.Text = string.Empty;
            add_task_btn.IsEnabled = false;
            priority_combo.Text = "Low";
        }

        private void add_task_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!DoesTaskAlreadyExist(task_namebox.Text))
            {
                added_tasks_list.Items.Add(new ListTaskObject { Name = currentTask.Name, Priority = currentTask.Priority.ToString(), DueDate = currentTask.DueDate });
                created_tasks.Add(currentTask);
                
                if (mode != "project")
                {
                    newly_added_tasks.Add(currentTask);
                }

                add_task_btn.IsEnabled = false;
                Reset();
                create_btn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show($"{task_namebox.Text} already exists! Please choose a different name!", "Name already exists...", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<ListTaskObject> excluded_search_items = new List<ListTaskObject>();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(list_searchbox.Text))
            {
                search_btn.IsEnabled = true;
            }
            else
            {
                foreach (ListTaskObject item in excluded_search_items)
                {
                    added_tasks_list.Items.Add(item);
                }
                excluded_search_items.Clear();
                search_btn.IsEnabled = false;
            }
        }

        private void search_btn_Click(object sender, RoutedEventArgs e)
        {
            if (list_searchbox.Text != string.Empty)
            {
                for (int i = 0; i < added_tasks_list.Items.Count; i++)
                {
#pragma warning disable
                    ListTaskObject item = added_tasks_list.Items[i] as ListTaskObject;
                    if (!item.Name.ToLower().Contains(list_searchbox.Text.ToLower()))
                    {
                        excluded_search_items.Add(item);
                        added_tasks_list.Items.RemoveAt(i);
                    }
                }
            }
        }
#pragma warning enable

        private void CombineLists()
        {
            foreach (TaskObject created_task in created_tasks)
            {
                if (!newly_added_tasks.Contains(created_task))
                {
                    newly_added_tasks.Add(created_task);
                }
            }
        }

        private void create_btn_Click(object sender, RoutedEventArgs e)
        {
            if (created_tasks.Count <= 0)
            {
                if (mode == "project")
                {
                    MessageBox.Show("Please create atleast 1 task before attempting to create a new project!");
                }
                else if (mode != "edit")
                {
                    MessageBox.Show("Cannot add zero new tasks! Please create atleast 1 task before attempting to update existing task list!");
                }
                return;
            }

            string SetTaskString(object data)
            {
                List<TaskObject> list = data as List<TaskObject>;
                string str = string.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    TaskObject task = created_tasks[i];

                    str += task.Name;
                    if (i < created_tasks.Count - 1)
                    {
                        str += "\n";
                    }
                }
                return str;
            }

            string task_string = string.Empty;

            if (mode == "project")
            {
                ProjectAdderWindow paw = parent_window as ProjectAdderWindow;
                project_n = paw.GetName();
                task_string = SetTaskString(created_tasks);
            }
            else
            {
                CombineLists();
                task_string = SetTaskString(newly_added_tasks);
            }

            string messagebox_message = string.Empty;

            switch (mode)
            {
                case "project":
                    messagebox_message = $"""
            Are you wish to create a new project with the name: {project_n}
            and the following tasks:
            {task_string}
            """;
                    break;
                case "update":
                    messagebox_message = $"""
            Are you wish to update the following project {project_n}
            with the following tasks:
            {task_string}
            """;
                    break;
                case "edit":
                    messagebox_message = $"""
                        Are you sure you wish to edit the following task: {original_name}?
                        """;
                    break;
            }

            if (MessageBox.Show(messagebox_message, (mode == "project") ? "Creating project..." : "Updating project tasks...", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                switch (mode)
                {
                    case ("project"):
                        DatabaseHandler.SaveProject(project_n, created_tasks);                         
                        break;
                    case ("update"):
                        ProjectObject project = DatabaseHandler.FetchProject(project_n);
                        CombineLists();
                        project.TaskList.Clear();
                        foreach (TaskObject task in newly_added_tasks)
                        {
                            project.TaskList.Add(task);
                        }
                        CopyPageDisplayWindow window = (CopyPageDisplayWindow)parent_window;
                        window.new_tasks = new List<TaskObject>(project.TaskList);
                        DatabaseHandler.UpdateProjectTaskList(project); //  <-- maybe add autosaving later for this part.
                        break;
                    case ("edit"):
                        if (main_window_object != null)
                        {
                            main_window_object.EditTask(currentTask, original_name);
                        }
                        break;
                }
                Window window_obj = parent_window as Window;
                window_obj.Close();
            }
        }

        private string previous_month_value = "January";
        private void month_combo_DropDownClosed(object sender, EventArgs e)
        {
            if (month_combo.Text != previous_month_value)
            {
                previous_month_value = month_combo.Text;
                UpdateDayBoxDependingOnMonth(month_combo.Text);
            }
        }

        private void task_namebox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(task_namebox.Text) && !DoesTaskAlreadyExist(task_namebox.Text))
            {
                if (mode != "edit")
                {
                    add_task_btn.IsEnabled = true;
                }
                currentTask.Name = task_namebox.Text;
            }
            else
            {
                add_task_btn.IsEnabled = false;
            }
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Are you sure you wish to delete selected/searched tasks?", "Deleting tasks...", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;

            if (!string.IsNullOrEmpty(list_searchbox.Text))
            {
                for (int i = 0; i < added_tasks_list.Items.Count; i++)
                {
                    ListTaskObject item = added_tasks_list.Items[i] as ListTaskObject;
                    if (item.Name == list_searchbox.Text)
                    {
                        added_tasks_list.Items.RemoveAt(i);
                        foreach (TaskObject created_task in created_tasks)
                        {
                            if (created_task.Name == list_searchbox.Text)
                            {
                                created_tasks.Remove(created_task);
                                break;
                            }
                        }
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < added_tasks_list.SelectedItems.Count; i++)
                {
                    ListTaskObject item = added_tasks_list.SelectedItems[i] as ListTaskObject;
                    for (int j = 0; j < created_tasks.Count; j++)
                    {
                        TaskObject created_task = created_tasks[j];
                        if (created_task.Name == item.Name)
                        {
                            created_tasks.RemoveAt(j);
                        }
                    }
                    added_tasks_list.Items.RemoveAt(i);
                }
            }

            if (added_tasks_list.Items.Count <= 0)
            {
                create_btn.IsEnabled = false;
            }
        }

    }
}