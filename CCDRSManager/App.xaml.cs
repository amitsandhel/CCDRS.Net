/*
    Copyright 2023 University of Toronto
    This file is part of CCDRS.
    CCDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    CCDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using CCDRSManager.Data;
using System.Windows;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly CCDRSContext _context = new CCDRSContext();
        public CCDRSManagerModelRepository CCDRSManagerModelRepository { get; }

        public static App Us { get; private set; } = null!;
        public App()
        {
            CCDRSManagerModelRepository = new CCDRSManagerModelRepository(_context);
            Us = this;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
