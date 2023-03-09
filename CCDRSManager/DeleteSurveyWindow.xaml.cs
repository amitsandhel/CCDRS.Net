using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for DeleteSurveyWindow.xaml
    /// </summary>
    public partial class DeleteSurveyWindow : Window
    {
        public DeleteSurveyWindow()
        {
            InitializeComponent();
        }

        [GeneratedRegex("[^0-9]+")]
        private static partial Regex CheckNumber();

        /// <summary>
        /// Method to ensure that the values entered in the survey textbox are only integer numbers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = CheckNumber().IsMatch(e.Text);
        }

        /// <summary>
        /// Delete the Survey data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteSurvey(object sender, RoutedEventArgs e)
        {
            if (DataContext is CCDRSManagerViewModel vm)
            {
                await vm.DeleteSurveyDataAsync();
            }
        }
    }
}
