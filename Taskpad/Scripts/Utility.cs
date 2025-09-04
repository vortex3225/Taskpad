using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskpad.Objects;
using SQLitePCL;
using Microsoft.Data.Sqlite;
using System.Windows;

namespace Taskpad.Scripts
{
	public static class Utility
	{
		public const string TASK_LIST_SPLIT = "???---???"; // Used to differentiate between different tasks in a string.
		public const string TASK_PARAM_SPLIT = ":++"; // Used to differentiate between different task parameters such as due date and priority.


		public static bool DoesNameAlreadyExist(string name_to_check)
		{
			try
			{
				using (SqliteConnection cnn = new SqliteConnection(DatabaseHandler.LoadConnectionString()))
				{
					cnn.Open();
					string sql = "SELECT * FROM Projects WHERE name=@Name LIMIT 1";
					using var cmd = new SqliteCommand(sql, cnn);
					cmd.Parameters.AddWithValue("@Name", name_to_check);
					object ?fetched = cmd.ExecuteScalar();
					if (fetched != null)
					{
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return false;
		}

        public static string ConvertTaskListToString(List<TaskObject> task_list)
        {
            return string.Join(TASK_LIST_SPLIT, task_list.Select(task =>
                $"{task.Name}{TASK_PARAM_SPLIT}{task.Priority}{TASK_PARAM_SPLIT}{task.DueDate}{TASK_PARAM_SPLIT}{task.Completed}"
            ));
        }

        public static List<TaskObject> GetTasksFromString(string task_string)
		{
			List<TaskObject> fetched_list = new List<TaskObject>();

			foreach (string task_object in task_string.Split(TASK_LIST_SPLIT))
			{
				string[] task_params = task_object.Split(TASK_PARAM_SPLIT);
				
				string name = task_params[0];
				TaskPriority priority = Enum.Parse<TaskPriority>(task_params[1]);
				string due_date = task_params[2];
				string completed = task_params[3];
				bool val = false;

				TaskObject task = new TaskObject()
				{
					Name = name,
					Priority = priority,
					DueDate = due_date,
					Completed = bool.TryParse(completed, out val) ? val : false,
				};
				fetched_list.Add(task);
			}

			return fetched_list;
		}

	}
}