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
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
            display_previously_open_project_message.IsChecked = AppPrefs.prefs["display_previously_open_project_message"];
            open_previously_open_project_on_startup.IsChecked = AppPrefs.prefs["open_previously_open_project_on_startup"];
            delete_confirmation_warning.IsChecked = AppPrefs.prefs["delete_confirmation_warning"];
            unsaved_changes_warning.IsChecked = AppPrefs.prefs["unsaved_changes_warning"];
        }

        private void save_menu_btn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to overwrite preferences?", "Saving preferences", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                DatabaseHandler.SavePreferences();
                string ?pref_string = DatabaseHandler.GetPreferencesString();
                if (pref_string != null)
                    AppPrefs.Set(pref_string);
                this.Close();
            }
        }

        private void display_previously_open_project_message_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            AppPrefs.prefs[c.Name] = (c.IsChecked == true);
        }

        private void open_previously_open_project_on_startup_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            AppPrefs.prefs[c.Name] = (c.IsChecked == true);
        }

        private void delete_confirmation_warning_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            AppPrefs.prefs[c.Name] = (c.IsChecked == true);
        }

        private void unsaved_changes_warning_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            AppPrefs.prefs[c.Name] = (c.IsChecked == true);
        }
        private void delete_task_warning_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            AppPrefs.prefs[c.Name] = (c.IsChecked == true);
        }

        private void reset_menu_btn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to reset your preferences?", "Reseting preferences...", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AppPrefs.Set(AppPrefs.DEFAULT_PREFERENCES);
                DatabaseHandler.SavePreferences();
                this.Close();
            }
        }
    }
}