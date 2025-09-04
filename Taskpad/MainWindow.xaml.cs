using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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
using Taskpad.Windows;
using Taskpad.Windows.CopyWindows;

namespace Taskpad
{
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Immutable;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    using System.ComponentModel;
    using System.Threading.Tasks;

    public class ListViewTask : INotifyPropertyChanged
    {
        private string name;
        private string dueDate;
        private string priority;
        private bool completed;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string DueDate
        {
            get => dueDate;
            set
            {
                if (dueDate != value)
                {
                    dueDate = value;
                    OnPropertyChanged(nameof(DueDate));
                }
            }
        }

        public string Priority
        {
            get => priority;
            set
            {
                if (priority != value)
                {
                    priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        public bool Completed
        {
            get => completed;
            set
            {
                if (completed != value)
                {
                    completed = value;
                    OnPropertyChanged(nameof(Completed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public partial class MainWindow : Window
    {
        //private static List<TaskObject> original_task_states = new List<TaskObject>();
        private static List<TaskObject> added_tasks = new List<TaskObject>();
        private static Dictionary<string, bool> previous_states = new Dictionary<string, bool>();
        public static ProjectObject ?current_project = null;
        private static int changes_made = 0;

        // CUSTOM FUNCTIONS

        // SNAPSHOTTING FUNCTIONS
        public List<TaskObject> GetCurrentTasks()
        {
            return added_tasks.Select(t => new TaskObject
            {
                Name = t.Name,
                DueDate = t.DueDate,
                Priority = t.Priority,
                Completed = t.Completed
            }).ToList();
        }

        public Snapshot GetCurrentTasksAsSnapshot()
        {
            return new Snapshot { SnapshottedTaskList = GetCurrentTasks() };
        }

        public void LoadSnapshot(Snapshot snapshot, bool was_reverted = false)
        {
            if (!was_reverted)
                SnapshotService.AddRedoPoint(added_tasks); // adds a redo point before undoing

            //Console.WriteLine($"Loading snapshot of size: {snapshot.SnapshottedTaskList.Count}");

            added_tasks.Clear();
            previous_states.Clear();
            LoadTasksOfProject(task_string: snapshot.SnapshottedTaskList);
            
            if (!was_reverted)
            {
                changes_made = 1;
            }
            else
                changes_made = Math.Clamp(--changes_made, 0, int.MaxValue);
            UpdateChangesDisplay();
        }
        // -----------------------------------

        public void EditTask(TaskObject edited_task, string? og_name = null)
        {
            if (edited_task == null)
                return;

            string edited_name = edited_task.Name;

            TaskObject? taskInProject = current_project.TaskList.FirstOrDefault(t => t.Name == og_name);
            if (taskInProject != null)
            {
                taskInProject.Name = edited_name;
                taskInProject.DueDate = edited_task.DueDate;
                taskInProject.Priority = edited_task.Priority;
                taskInProject.Completed = edited_task.Completed;
            }

            SnapshotService.CreateSnapshot(added_tasks);

            var taskInUI = tasks_list.Items.OfType<ListViewTask>()
                             .FirstOrDefault(t => t.Name == og_name);
            if (taskInUI != null)
                tasks_list.Items.Remove(taskInUI);

            var taskInMemory = added_tasks.FirstOrDefault(t => t.Name == og_name);
            if (taskInMemory != null)
                added_tasks.Remove(taskInMemory);

            TaskObject updatedTask = new TaskObject
            {
                Name = edited_name,
                DueDate = edited_task.DueDate,
                Priority = edited_task.Priority,
                Completed = edited_task.Completed
            };

            added_tasks.Add(updatedTask);
            previous_states[updatedTask.Name] = updatedTask.Completed;

            tasks_list.Items.Add(new ListViewTask
            {
                Name = updatedTask.Name,
                DueDate = updatedTask.DueDate,
                Priority = updatedTask.Priority.ToString(),
                Completed = updatedTask.Completed
            });

            changes_made++;
            UpdateChangesDisplay();
        }

        private void LoadTasksOfProject(ProjectObject? projectObject = null, string? project_name = null, List<TaskObject> ?task_string = null)
        {
            try
            {
                ProjectObject? project = null;
                if (task_string == null)
                {
                    project = projectObject ?? DatabaseHandler.FetchProject(project_name);
                    if (project == null)
                        return;
                }

                tasks_list.Items.Clear();

                List<TaskObject>? to_enumerate = task_string ?? project.TaskList;
                if (to_enumerate == null) { Console.WriteLine("Couldnt load enumerate table"); return; }

                foreach (TaskObject task in to_enumerate)
                {
                    added_tasks.Add(task);
                    string task_name = task.Name;
                    bool completed_value = task.Completed;
                    previous_states.Add(task_name, completed_value);
                    tasks_list.Items.Add(new ListViewTask
                    {
                        Name = task.Name,
                        DueDate = task.DueDate,
                        Priority = task.Priority.ToString(),
                        Completed = task.Completed
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetCompletedTasksCount()
        {
            int completed = 0;
            if (current_project == null || current_project.TaskList == null)
                return completed;


            foreach (TaskObject task in current_project.TaskList)
            {
                if (task.Completed)
                {
                    completed++;
                }
            }
            return completed;
        }
        private void InitialiseTaskProgressBar()
        {
            if (current_project == null)
            {
                progress_grid.Visibility = Visibility.Collapsed;
                return;
            }

            tasks_progress_bar.Maximum = current_project.TaskList.Count;
            int completed = GetCompletedTasksCount();

            tasks_progress_bar.Value = completed;
            task_progress_display.Text = $"{Math.Clamp(added_tasks.Count - completed, 0, added_tasks.Count)} tasks pending, {completed} tasks completed";
        }

        private void UpdateTaskProgressBar(int increment_value)
        {
            switch (increment_value)
            {
                case -1:
                    tasks_progress_bar.Value--;
                    

                    break;
                case 1:
                    tasks_progress_bar.Value++;
                    break;
            }
            int completed = GetCompletedTasksCount();
            task_progress_display.Text = $"{Math.Clamp(added_tasks.Count - completed, 0, added_tasks.Count)} tasks pending, {completed} tasks completed";
        }

        private void Reset()
        {
            previous_states.Clear();
            added_tasks.Clear();
            tasks_list.Items.Clear();
            tasks_progress_bar.Value = 0;
            task_progress_display.Text = "0 tasks pending, 0 tasks completed";
            current_project = null;
            changes_made = 0;
            warning_display.Visibility = Visibility.Hidden;
            progress_grid.Visibility = Visibility.Collapsed;
        }

        private bool? GetPreviousTaskState(string task_name)
        {
            if (current_project == null)
                return null;

            bool previous_state = previous_states[task_name];

            return previous_state;
        }

        private void UpdateChangesDisplay()
        {
            if (changes_made <= 0)
            {
                warning_display.Visibility = Visibility.Hidden;
            }
            else
            {
                warning_display.Visibility = Visibility.Visible;
                warning_display.Text = $"{changes_made} unsaved changes.";
            }
        }
        public void RefreshTaskList(List<TaskObject> new_list)
        {

            ProjectObject p = current_project;
            Reset();
            current_project = p;
            foreach (TaskObject task in new_list)
            {
                added_tasks.Add(task);
                string task_name = task.Name;
                bool completed_value = task.Completed;
                previous_states.Add(task_name, completed_value);
                tasks_list.Items.Add(new ListViewTask
                {
                    Name = task.Name,
                    DueDate = task.DueDate,
                    Priority = task.Priority.ToString(),
                    Completed = task.Completed
                });
            }
        }

        // ----------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            SnapshotService.Init(this);

            AppPrefs.Set(DatabaseHandler.GetPreferencesString());
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            this.Closing += MainWindow_Closing;
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                Save();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                tasks_list.SelectAll();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H)
            {
                HelpWindow hw = new HelpWindow();
                hw.ShowDialog();
            }
            else if ((Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl)) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && e.Key == Key.A)
            {
                NewTask();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppPrefs.prefs["open_previously_open_project_on_startup"] == false) return;
            current_project = DatabaseHandler.GetPreviouslyOpenedProject();
            if (current_project == null)
            {
                if (AppPrefs.prefs["display_previously_open_project_message"]) MessageBox.Show("No previously opened project found!");
                new_task_btn.IsEnabled = false;
                DatabaseHandler.SetPreviouslyOpenedProject(string.Empty);
                progress_grid.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (AppPrefs.prefs["display_previously_open_project_message"]) MessageBox.Show($"Loaded previous project: {current_project.Name}");
                if (current_project.TaskList.Count > 0)
                {
                    LoadTasksOfProject(current_project);
                    progress_grid.Visibility = Visibility.Visible;
                    InitialiseTaskProgressBar();
                }
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (changes_made > 0 && (AppPrefs.prefs["unsaved_changes_warning"] == true && MessageBox.Show($"Are you sure you wish to exit?\nYou have {changes_made} unsaved changes which will be lost!", $"Exiting with {changes_made} unsaved changes.", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes))
            {
                e.Cancel = true;
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (current_project != null && current_project.Name != null)
            {
                DatabaseHandler.SetPreviouslyOpenedProject(current_project.Name);
            }
        }

        // AUX

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            SnapshotService.CreateSnapshot(added_tasks);

            CheckBox ?checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                foreach (TaskObject task in added_tasks)
                {
#pragma warning disable
                    if (task.Name == checkBox.Tag)
                    {
                        task.Completed = (checkBox.IsChecked == true);
                        int taskbar_progress = (checkBox.IsChecked == true) ? 1 : -1;
                        
                        bool ?previous_state = GetPreviousTaskState(task.Name);
                        if (previous_state != null && previous_state != task.Completed)
                        {
                            changes_made++;
                        }
                        else if (previous_state != null && previous_state == task.Completed)
                        {
                            changes_made = Math.Clamp(--changes_made, 0, int.MaxValue);
                        }

                        UpdateChangesDisplay();
                        UpdateTaskProgressBar(taskbar_progress);
                        return;
                    }
                }
            }
#pragma warning enable
        }

        //---------------------------------------------------------------------------

        // MENU BUTTONS

        private void open_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (changes_made > 0 && (AppPrefs.prefs["unsaved_changes_warning"] == true && MessageBox.Show("Are you sure you wish to open a new project? Once selected all unsaved changes from the current project will be lost!", "Unsaved changes...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes))
                return;
            List<ProjectObject> ?fetched = DatabaseHandler.FetchProjects();
            if (fetched != null && fetched.Count > 0 )
            {
                Reset();
                ProjectSelectorWindow psw = new ProjectSelectorWindow();
                if (psw.ShowDialog() == true)
                {
                    LoadTasksOfProject(current_project);
                    DatabaseHandler.SetPreviouslyOpenedProject(current_project.Name);
                    progress_grid.Visibility = Visibility.Visible;
                    UpdateTaskProgressBar(GetCompletedTasksCount());
                } 
            }
        }

        private void Save()
        {
            bool succceded = DatabaseHandler.UpdateProjectTaskList(current_project);
            if (succceded)
            {
                SnapshotService.ClearHistory();
                changes_made = 0;
                warning_display.Visibility = Visibility.Hidden;
                foreach (TaskObject task in added_tasks)
                {
                    previous_states[task.Name] = task.Completed;
                }
            }
            else
            {
                MessageBox.Show("Something went wrong while attempting to save changes!");
            }
        }

        private void save_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (changes_made <= 0)
            {
                changes_made = 0;
                MessageBox.Show("Already up to date.", "No changes to save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (current_project != null && MessageBox.Show($"Do you want to save all {changes_made} changes?\nThis will also clear your undo history.", $"Saving {changes_made} changes...", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Save();
            }
            else if (current_project == null)
            {
                MessageBox.Show("Please open a project first!", "No project selected!");
            }
        }
        private void new_project_mbtn_Click(object sender, RoutedEventArgs e)
        {
            ProjectAdderWindow paw = new ProjectAdderWindow();
            paw.ShowDialog();
        }

        private void saveas_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (current_project != null && changes_made <= 0)
            {
                SaveHandler.SaveAs(SaveType.log, current_project);
            }
            else if (current_project == null)
            {
                MessageBox.Show("Please open a project first!", "No project to save...", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (changes_made > 0)
            {
                MessageBox.Show("Please save your changes first!", "Unsaved changes detected...", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void import_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (changes_made > 0 && (AppPrefs.prefs["unsaved_changes_warning"] == true && MessageBox.Show("Are you sure you wish to import a new project? Previous changes will be lost!", "Unsaved changes while importing...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes))
                return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a file to import";
            ofd.DefaultExt = "tpd";  // no dot here
            ofd.Filter = "Taskpad Project Files (*.tpd)|*.tpd|All Files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                ProjectObject? fetched = SaveHandler.Import(ofd.FileName);
                if (fetched != null)
                {
                    MessageBox.Show($"Imported {fetched.Name}", "Successfully imported project!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Reset();
                    current_project = fetched;
                    LoadTasksOfProject(current_project);
                    progress_grid.Visibility = Visibility.Visible;
                    UpdateChangesDisplay();
                }
            }
        }

        private void export_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (current_project == null)
            {
                MessageBox.Show("Please open a project to export first!");
                return;
            }

            if (MessageBox.Show($"Are you sure you wish to export the following project: {current_project.Name}?", "Exporting project...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SaveHandler.Export(current_project);
            }
        }

        private void exit_task_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppPrefs.prefs["unsaved_changes_warning"] == false || MessageBox.Show("Are you sure you wish to exit Taskpad? Any unsaved changes will be lost!", "Exiting taskpad...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                this.Close();
        }

        private void undo_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            SnapshotService.Undo();
        }

        private void redo_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            SnapshotService.Redo();
        }

        private void selectall_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you wish to select every task in the list? ({added_tasks.Count} tasks)", $"Selecting all ({added_tasks.Count}) tasks in list...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            tasks_list.SelectAll();
        }

        private void edittask_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (tasks_list.SelectedItem == null)
            {
                MessageBox.Show("Please select a task first before editing!", "No task selected for editing...", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Are you sure you wish to edit the selected task?", $"Editing task...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            TaskObject selected_task = added_tasks[tasks_list.SelectedIndex];
            CopyPageDisplayWindow display_window = new CopyPageDisplayWindow(this, new string[] {"is_editing_flag"});
            CreateTasksWindows create_window = new CreateTasksWindows(current_project.Name, display_window, "edit", task_namebox.Text, true, selected_task, this);
            display_window.SetContent(create_window);
            display_window.ShowDialog();
            
        }

        private void deletetask_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if (tasks_list.SelectedItem == null)
            {
                MessageBox.Show("No task selected! Please select a task first before attempting to delete!", "No task selected to delete", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TaskObject selected = added_tasks[tasks_list.SelectedIndex];

            if (AppPrefs.prefs["delete_task_warning"] == false || MessageBox.Show($"Are you sure you wish to delete the following task: {selected.Name}? All previous changes will be saved!\nThis action cannot be reversed!", $"Deleting {selected.Name}...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Save();
                TaskObject? foundInProject = current_project.TaskList.FirstOrDefault(t => t.Name == selected.Name);
                if (foundInProject != null) current_project.TaskList.Remove(foundInProject);
                TaskObject foundInAdded = added_tasks.FirstOrDefault(t => t.Name == selected.Name);
                if (foundInAdded != null) added_tasks.Remove(foundInAdded);
                
                for (int i = tasks_list.Items.Count - 1; i >= 0; i--)
                {
                    ListViewTask tsk = tasks_list.Items[i] as ListViewTask;

                    if (tsk.Name == selected.Name)
                    {
                        tasks_list.Items.RemoveAt(i);
                        break;
                    }
                }
                previous_states.Remove(selected.Name);

                DatabaseHandler.UpdateProjectTaskList(current_project);
            }
            InitialiseTaskProgressBar();
        }
        private void deleteproject_mbtn_Click(object sender, RoutedEventArgs e)
        {
            List<ProjectObject>? fetched = DatabaseHandler.FetchProjects();
            if (fetched != null && fetched.Count > 0)
            {
                ProjectSelectorWindow psw = new ProjectSelectorWindow(true);
                if (psw.ShowDialog() == true)
                {
                    LoadTasksOfProject(current_project);
                    DatabaseHandler.SetPreviouslyOpenedProject(current_project.Name);
                    progress_grid.Visibility = Visibility.Visible;
                    UpdateTaskProgressBar(GetCompletedTasksCount());
                }
            }
        }
        private void clear_opened_project_mbtn_Click(object sender, RoutedEventArgs e)
        {
            if ((AppPrefs.prefs["delete_confirmation_warning"] == false || MessageBox.Show("Are you sure you wish to close the current project?", "Closing current project...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes) && (changes_made <= 0 || (AppPrefs.prefs["unsaved_changes_warning"] == true && MessageBox.Show("Your unsaved changes will be lost! Continue?", "Unsaved changes will be lost", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)))
            {
                Reset();
            }
        }
        private void preferences_edit_mbtn_Click(object sender, RoutedEventArgs e)
        {
            PreferencesWindow pf = new PreferencesWindow();
            pf.ShowDialog();
        }
        private void help_mbtn_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.ShowDialog();
        }
        // MENU BUTTONS

        // TASK CREATION PANEL  
        private void task_namebox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool name_already_exists = Utility.DoesNameAlreadyExist(task_namebox.Text);
            bool disable_btn = false;
            task_warningbox.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(task_namebox.Text))
            {
                disable_btn = true;
            }
            new_task_btn.IsEnabled = !disable_btn;
        }

        private void NewTask()
        {
            if (current_project != null)
            {
                CopyPageDisplayWindow display_window = new CopyPageDisplayWindow(this, new string[] { "dont_delete_if_empty" });
                CreateTasksWindows create_window = new CreateTasksWindows(current_project.Name, display_window, "update", task_namebox.Text);
                display_window.SetContent(create_window);
                display_window.ShowDialog();
            }
        }
        private void new_task_btn_Click(object sender, RoutedEventArgs e)
        {
            NewTask();
        }
        // ------------------------------------------ //
    }
}
