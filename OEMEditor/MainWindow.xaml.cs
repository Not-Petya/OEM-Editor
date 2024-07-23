using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace OEMEditor
{
    public partial class MainWindow : Window
    {
        private static class OEMKey
        {
            private static RegistryKey key;
            public static RegistryKey GetKey()
            {
                if (key == null)
                {
                    string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation";
                    RegistryKey baseKey = Registry.LocalMachine;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    }
                    key = baseKey.CreateSubKey(keyPath);
                }
                return key;
            }

            public static string GetStringFromKey(string name)
            {
                if (key == null)
                    key = GetKey();
                object val = key.GetValue(name);
                return val as string ?? "";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            GetCurrentInformation();
        }

        private void btnITMDefault_Click(object sender, RoutedEventArgs e)
        {
            txtManufacturer.Text = "ITM GmbH";
            txtModel.Text = "ITM";
            txtSupportHours.Text = "Mo. - Fr.: 8:00-12:00 || 13:00-17:00";
            txtSupportPhone.Text = "+49 831 5752760";
            txtSupportUrl.Text = "https://itm-technologies.de/";

            string destinationPath = "C:\\ITM\\itm.bmp";
            try
            {
                if (!Directory.Exists("C:\\ITM"))
                {
                    Directory.CreateDirectory("C:\\ITM");
                }

                using (Stream resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/img/itm.bmp")).Stream)
                {
                    using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }

                // Aktualisieren Sie den Registry-Wert für das Logo
                SetLogoRegistryValue(destinationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error copying file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetUpdatedInformation();
        }

        private void SetLogoRegistryValue(string imagePath)
        {
            try
            {
                RegistryKey key = OEMKey.GetKey();
                key.SetValue("Logo", imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating registry: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetCurrentInformation()
        {
            txtManufacturer.Text = OEMKey.GetStringFromKey("Manufacturer");
            txtModel.Text = OEMKey.GetStringFromKey("Model");
            txtSupportHours.Text = OEMKey.GetStringFromKey("SupportHours");
            txtSupportPhone.Text = OEMKey.GetStringFromKey("SupportPhone");
            txtSupportUrl.Text = OEMKey.GetStringFromKey("SupportURL");
        }

        private void SetUpdatedInformation()
        {
            string manufacturer = txtManufacturer.Text;
            string model = txtModel.Text;
            string supportHours = txtSupportHours.Text;
            string supportPhone = txtSupportPhone.Text;
            string supportUrl = txtSupportUrl.Text;

            RegistryKey key = OEMKey.GetKey();

            if ((int)key.GetValue("HelpCustomized", 0) == 1)
                key.SetValue("HelpCustomized", 0, RegistryValueKind.DWord);

            key.SetValue("Manufacturer", manufacturer);
            key.SetValue("Model", model);
            key.SetValue("SupportHours", supportHours);
            key.SetValue("SupportPhone", supportPhone);
            key.SetValue("SupportURL", supportUrl);

            string cplPath = Path.Combine(Environment.SystemDirectory, "control.exe");
            System.Diagnostics.Process.Start(cplPath, "/name Microsoft.System");
        }
    }
}
