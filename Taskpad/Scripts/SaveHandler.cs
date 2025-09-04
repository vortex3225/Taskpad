using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Taskpad.Objects;

namespace Taskpad.Scripts
{
    public enum SaveType
    {
        txt,
        log
    }
    public static class SaveHandler
    {
        /*
         TASKPAD EXPORT FORMAT

        NAMING -> {project-name}_export
        EXTENSION -> .tpd

         */

        private static int[] column_widths = { 10, 13, 13, 14 };
        private static string[] columns = { "Name", "Due Date", "Priority", "Completed?" };
        private static string header_text = string.Empty;

        private static string FormatTaskData(TaskObject task_data)
        {
            string[] data = { task_data.Name, task_data.DueDate, task_data.Priority.ToString(), task_data.Completed.ToString() };
            int max_len = column_widths[0];

            for (int i = 0; i < data.Length; i++)
            {
                int maxLen = Math.Max(columns[i].Length, data[i].Length);
                if (maxLen > column_widths[i])
                    column_widths[i] = maxLen;
                max_len = maxLen;
            }

            if (string.IsNullOrEmpty(header_text))
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    header_text += $"| {columns[i].PadRight(column_widths[i])} ";
                }
                header_text += "|" + '\n';
            }

            string return_data = "";
            for (int i = 0; i < data.Length; i++)
            {
                return_data += $"| {data[i].PadRight(column_widths[i])} ";
            }
            return_data += "|";

            return return_data;
        }

        private static string GetProjectContentsAsString(ProjectObject project)
        {
            string task_list_string = string.Empty;

            int[] widths = { 15, 15, 15, 15 };
            // initialises the task_list_string
            for (int i = 0; i < project.TaskList.Count; i++)
            {
                TaskObject task = project.TaskList[i];
                
                task_list_string += FormatTaskData(task) + '\n';
            }

            string project_contents = $"""
                ************************** TASKPAD **************************
                *
                *
                Project name: {project.Name}
                Created at: {project.CreationDate}
                *
                *
                *
                ** TASKS **
                {header_text}
                {task_list_string}
                """;
            return project_contents;
        }

        public static void SaveAs(SaveType file_type, ProjectObject project)
        {
            string cwd = AppContext.BaseDirectory;
            string new_file_path = Path.Combine(cwd, $"{project.Name.ToUpper()}.{file_type.ToString()}");

            try
            {
                if (!File.Exists(new_file_path) || MessageBox.Show($"File at path: {new_file_path} already exists! Would you like to overwrite it?", "Overwrite existing file...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    File.WriteAllText(new_file_path, GetProjectContentsAsString(project));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't save project: {ex.Message}");
            }
        }

        public static void Export(ProjectObject project)
        {
            // exports the list as a json for importing
            string save_path = Path.Combine(AppContext.BaseDirectory, $"{project.Name}.tpd"); // saves with the tpd extension

            string json_string = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                if (!File.Exists(save_path) || MessageBox.Show($"{save_path} already exist! Would you like to overwrite it?", "Overwrite existing exported file...", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    File.WriteAllText(save_path, json_string);
                }
                else
                {
                    return;
                }
                MessageBox.Show($"Successfully exported {project.Name} to {AppContext.BaseDirectory}!", "Sucessfully exported project", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't export project: {ex.Message}");
            }
        }

        public static ProjectObject ?Import(string import_path_file)
        {
            string fetched_json = string.Empty;
            try
            {
                if (File.Exists(import_path_file))
                {
                    fetched_json = File.ReadAllText(import_path_file);
                }
                else
                {
                    throw new Exception($"{import_path_file} does not exist!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot import: {import_path_file}");
            }

            if (fetched_json != string.Empty)
            {
                try
                {
                    return JsonSerializer.Deserialize<ProjectObject>(fetched_json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot deserialize data: {ex.Message}");
                }
            }
            return null;
        }

    }
}
