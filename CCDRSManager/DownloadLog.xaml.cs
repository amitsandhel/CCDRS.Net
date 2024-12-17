/*
    Copyright 2024 University of Toronto
    This file is part of IDRS.
    IDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    IDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Windows;

namespace CCDRSManager;

/// <summary>
/// Interaction logic for DownloadLog.xaml
/// </summary>
public partial class DownloadLog : Window
{
    public DownloadLog()
    {
        InitializeComponent();
    }

    private void DownloadActivityLog(object sender, RoutedEventArgs e)
    {
        if(DataContext is CCDRSManagerViewModel vm)
        {
            vm.DownloadActivityLog();
        }
    }
}
