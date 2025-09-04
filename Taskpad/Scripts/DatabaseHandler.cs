using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskpad.Objects;
using SQLitePCL;
using Microsoft.Data.Sqlite;
using System.Windows;

namespace Taskpad.Scripts
{
    public static class DatabaseHandler
    {
        public static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }

        // MAIN DATABASE OPERATIONS

        /*
        Handles all of the task and project related database operations.
        */
        public static void SaveProject(string project_name, List<TaskObject> tasks)
        {
            try
            {
                if (Utility.DoesNameAlreadyExist(project_name))
                {
                    throw new Exception($"Name already exists for project ({project_name})! Please use a different name!");
                }

                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sql = $"INSERT INTO Projects (name, tasks_list, creation_date) VALUES (@Name, @Tasks, @Creation)";

                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@Name", project_name);
                    cmd.Parameters.AddWithValue("@Tasks", Utility.ConvertTaskListToString(tasks));
                    cmd.Parameters.AddWithValue("@Creation", DateTime.Now.ToString("dd.MM.yyyy @ HH/:mm"));

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DeleteProject(string ?project_name = null, ProjectObject ?project = null)
        {
            try
            {
                project_name = project_name ?? project.Name;
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sql = "DELETE FROM Projects WHERE name=@Name";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@Name", project_name);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't delete project with the name: {project_name} due to exception: {ex.Message}");
            }
        }

        public static bool UpdateProjectTaskList(ProjectObject project)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    string project_name = project.Name;
                    cnn.Open();
                    string new_task_list_string = Utility.ConvertTaskListToString(project.TaskList);
                    string sql = "UPDATE Projects SET tasks_list=@NewTasks WHERE name=@Name";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@NewTasks", new_task_list_string);
                    cmd.Parameters.AddWithValue("@Name", project_name);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
            return false;
        }

        public static ProjectObject ?FetchProject(string project_name)
        {
            //Console.WriteLine($"Fetching: {project_name}");
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sql = "SELECT * FROM Projects WHERE name=@Name";

                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@Name", project_name);

                    using var reader = cmd.ExecuteReader();


                    string? name = null;
                    List<TaskObject>? tasks = new List<TaskObject>();
                    string? creation_date = null;
                    while (reader.Read())
                    {
                        name = reader.GetString(0);
                        tasks = !reader.IsDBNull(1) && !string.IsNullOrWhiteSpace(reader.GetString(1))
                            ? Utility.GetTasksFromString(reader.GetString(1))
                            : new List<TaskObject>();
                        creation_date = reader.GetString(2);
                    }
                    if (name != null && tasks != null && creation_date != null)
                    {
                        ProjectObject fetched_project = new ProjectObject
                        {
                            Name = name,
                            TaskList = tasks,
                            CreationDate = creation_date
                        };
                        return fetched_project;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to fetch project: {ex.Message}");
                return null;
            }
        }

        public static List<ProjectObject> ?FetchProjects()
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();
                    string sql = "SELECT * FROM Projects";
                    using var cmd = new SqliteCommand(sql, cnn);
                    using var reader = cmd.ExecuteReader();

                    List<ProjectObject> fetched_projects = new List<ProjectObject>();

                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        List<TaskObject> tasks = !reader.IsDBNull(1) && !string.IsNullOrWhiteSpace(reader.GetString(1))
                            ? Utility.GetTasksFromString(reader.GetString(1))
                            : new List<TaskObject>();
                        string creation_date = reader.GetString(2);

                        ProjectObject project = new ProjectObject
                        {
                            Name = name,
                            TaskList = tasks,
                            CreationDate = creation_date
                        };
                        fetched_projects.Add(project);
                    }
                    return fetched_projects;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        // -----------------------------------------------------------------



        // SETTTINGS HANDLING

        /*
         The following part deals only with the application settings editing!
        */

        public static void SavePreferences()
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();

                    string pref_string = AppPrefs.GeneratePreferencesString();
                    string sql = "UPDATE Settings SET preferences=@Prefs";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@Prefs", pref_string);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save preferences: {ex.Message}");
            }
        }

        public static string ?GetPreferencesString()
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();

                    string pref_string = AppPrefs.GeneratePreferencesString();
                    string sql = "SELECT preferences FROM Settings";
                    using var cmd = new SqliteCommand(sql, cnn);
                    object? fetched = cmd.ExecuteScalar();

                    if (fetched != null)
                    {
                        return fetched.ToString();
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't get preference string: {ex.Message}");
                return null;
            }
        }
        public static ProjectObject ?GetPreviouslyOpenedProject()
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();

                    string sql = "SELECT prev_project_name FROM Settings";
                    using var cmd = new SqliteCommand(sql, cnn);
                    object? Fetched = cmd.ExecuteScalar();

                    if (Fetched == null)
                    {
                        return null;
                    }
                    else
                    {
                        ProjectObject ?fetched = FetchProject(Fetched.ToString());
                        return fetched;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static void SetPreviouslyOpenedProject(string project_name)
        {
            try
            {
                using (SqliteConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Open();

                    string sql = "UPDATE Settings SET prev_project_name=@Name";
                    using var cmd = new SqliteCommand(sql, cnn);
                    cmd.Parameters.AddWithValue("@Name", project_name);
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set previously opened project: {ex.Message}");
            }
        }
        // -----------------------------------------------------------------

    }
}