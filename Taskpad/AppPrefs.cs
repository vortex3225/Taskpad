using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskpad
{
    public static class AppPrefs
    {
        public static Dictionary<string, bool> prefs = new Dictionary<string, bool>
        {
            {"open_previously_open_project_on_startup", true },
            { "display_previously_open_project_message", true},
            { "delete_confirmation_warning", true},
            { "unsaved_changes_warning", true},
            {"delete_task_warning", true }
        };

        public static readonly string DEFAULT_PREFERENCES = GeneratePreferencesString();

        public static string GeneratePreferencesString()
        {
            string generated = string.Empty;
            int i = 0;
            foreach (KeyValuePair<string, bool> kvp in prefs)
            {
                string pref_string = $"{kvp.Key}:{kvp.Value.ToString()}";
                if (i < prefs.Count - 1)
                {
                    pref_string += @"\";
                }
                generated += pref_string;
                i++;
            }
            return generated;
        }
        public static void Set(string ?pref_string)
        {
            if (string.IsNullOrEmpty(pref_string)) { 
                Set(DEFAULT_PREFERENCES);
                return;
            }

            string[] splitted = pref_string.Split(@"\");
            foreach (string setting in splitted)
            {
                string[] split_setting = setting.Split(":");
                string setting_name = split_setting[0];
                string setting_value = split_setting[1];
                prefs[setting_name] = Convert.ToBoolean(setting_value);
            }
        }
    }
}