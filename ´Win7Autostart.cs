/* Win7Autostart - enables autostart for USB-Devices in Windows 7-10
   Copyright (C) 2016 Dennis M. Heine

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, please visit <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Win7Autostart
{

    public Win7Autostart
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key,
            string defaultValue, StringBuilder value, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue,
            [In, Out] char[] value, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string section, IntPtr keyValue,
            int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode]
        private static extern bool WritePrivateProfileString(string section, string key,
	    string value, string filePath);

        public static int capacity = 512;
        ArrayList currConnected = new ArrayList();
        Process p;
        String lastConnected = "";

        private void updateDriveLetters()
        {
            currConnected = new ArrayList();
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);

            foreach (DriveInfo d in drives)
            {
                if (d.IsReady && d.DriveType == DriveType.Removable)
                    currConnected.Add(d.RootDirectory.FullName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            updateDriveLetters();
        }

        private bool checkRootExisting(String check)
        {
            foreach (String r in currConnected)
            {
                if (r == check)
                    return true;
            }
            return false;
        }


        private bool isLastConnectedStillActive()
        {
            bool found=false;
            foreach(String dir in currConnected)
            {
                if (dir == lastConnected)
                    found = true;
            }
            return found;
        }

        private void tick()
        {
            var drives = DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);

            foreach(DriveInfo d in drives)
            {
                if(d.IsReady &&d.DriveType==DriveType.Removable)
                    if (!checkRootExisting(d.RootDirectory.FullName))
                    {
                        lastConnected = d.RootDirectory.FullName;
                        p = new Process();

                        var value = new StringBuilder(capacity);
                        GetPrivateProfileString("Autorun", "open", "", value, value.Capacity, d.RootDirectory + "\\autorun.inf");

                        ProcessStartInfo si = new ProcessStartInfo(d.RootDirectory + "\\"+value.ToString(), "");
                        p.StartInfo = si;
                        try
                        {
                            p.Start();
                        }catch(Exception i){
                            System.Threading.Thread.Sleep(2000);
                            try { p.Start(); }
                            catch (Exception e1) { };
                            }
                    }
            }
            
            updateDriveLetters();
            if (!isLastConnectedStillActive())
                if (p != null && p.Handle.ToInt32() > 0 && !p.HasExited)
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception e) { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tick();
        }
    }
}
