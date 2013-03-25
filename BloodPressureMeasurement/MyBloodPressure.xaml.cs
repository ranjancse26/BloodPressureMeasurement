using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Net.NetworkInformation;
using Microsoft.Health.Mobile;
using System.Xml.Linq;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace BloodPressureMeasurement
{
    public partial class MyBloodPressure : PhoneApplicationPage
    {
        bool addingRecord = false;
    
        public MyBloodPressure()
        {
            InitializeComponent();
            
            LayoutRoot.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("Background.png", UriKind.Relative))
            };

            DataContext = this;
            
            txtWhen.Text = DateTime.Now.Date.ToShortDateString();

            var time = DateTime.Now.TimeOfDay;
            timeControl.Value = new TimeSpan(time.Hours, time.Minutes, time.Seconds);

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                SetErrorMesasge("No Network Available");
                App.Quit();
            }

            Loaded += new RoutedEventHandler(MyBloodPressure_Loaded);
        }

        void MyBloodPressure_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(App.SettingsFilename);
            App.HealthVaultService.BeginAuthenticationCheck(AuthenticationCompleted, DoShellAuthentication);
            SetProgressBarVisibility(true);           
        }

        void DoShellAuthentication(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            App.HealthVaultService.SaveSettings(App.SettingsFilename);

            string url;

            if (addingRecord)
            {
                url = App.HealthVaultService.GetUserAuthorizationUrl();
            }
            else
            {
                url = App.HealthVaultService.GetApplicationCreationUrl();
            }

            App.HealthVaultShellUrl = url;

            // If we are  using hosted browser via the hosted browser page
            Uri pageUri = new Uri("/HostedBrowser.xaml", UriKind.RelativeOrAbsolute);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });
        }

        void AuthenticationCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e != null && e.ErrorText != null)
            {
                SetRecordName(e.ErrorText);
                return;
            }

            if (App.HealthVaultService.CurrentRecord == null)
            {
                App.HealthVaultService.CurrentRecord = App.HealthVaultService.Records[0];
            }

            App.HealthVaultService.SaveSettings(App.SettingsFilename);
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                // We are only interested in the last item
                HealthVaultMethods.GetThings(BloodPressureModel.TypeId, null, null, null, GetThingsCompleted);
                SetProgressBarVisibility(true);
            }
        }

        void PutThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);
            if (e.ErrorText != null)
            {
                SetErrorMesasge(e.ErrorText);
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    HealthVaultMethods.GetThings(BloodPressureModel.TypeId, null, null, null, GetThingsCompleted);
         
                    var messagePrompt = new MessagePrompt
                    {
                        Title = "Info",
                        Message = "Blood Pressure information saved successfully."
                    };
                    messagePrompt.Show();
                });
            }
        }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                // using LINQ to get the latest reading of blood pressure state
                XElement latestReading = (from thingNode in responseNode.Descendants("thing")
                                          orderby Convert.ToDateTime(thingNode.Element("eff-date").Value) descending
                                          select thingNode).FirstOrDefault<XElement>();

                if (latestReading != null)
                {
                    BloodPressureModel bloodPressureState = new BloodPressureModel();
                    bloodPressureState.Parse(latestReading);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        txtLastUpdated.Text = string.Format("When - {0}, Systolic - {1}, Diastolic - {2}",
                                                            bloodPressureState.When.ToString("MMM dd, yyyy"),
                                                            bloodPressureState.Systolic,
                                                            bloodPressureState.Diastolic);
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        txtLastUpdated.Text = "No readings! Time to track blood pressure.";
                    });
                }
            }
        }

        void SetRecordName(string recordName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_RecordName.Text = string.Format("Patient: {0}", recordName);
            });
        }

        void SetProgressBarVisibility(bool visible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        void SetErrorMesasge(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(message);
            });
        }       
               
        void AboutApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            AboutPrompt about = new AboutPrompt();
            about.Title = "About";
            about.VersionNumber = "Version 1.0";
            about.Body = new TextBlock { Text = "Designed and Developed by Ranjan.D", TextWrapping = TextWrapping.Wrap };
            about.Show();
        }

        private static Boolean IsNumeric(string stringToTest)
        {
            int result;
            return int.TryParse(stringToTest, out result);
        }

        private static Boolean IsValidDate(string stringToTest)
        {
            DateTime result;
            return DateTime.TryParse(stringToTest, out result);
        }

        private bool Validate()
        {
            if (txtWhen.Text.Equals(""))
            {
                MessageBox.Show("Date value cannot be empty");
                return false;
            }
            else
            if (IsValidDate(txtWhen.Text.Trim()) == false)
            {
                MessageBox.Show("Invalid blood pressure date!");
                return false;
            }

            if (txtSystolic.Text.Equals(""))
            {
                MessageBox.Show("Systolic value cannot be empty");
                return false;
            }
            else
            if (IsNumeric(txtSystolic.Text) == false)
            {
                MessageBox.Show("Systolic value should be numeric only!");
                return false;
            }

            if (txtdiastolic.Text.Equals(""))
            {
                MessageBox.Show("Diastolic value cannot be empty");
                return false;
            }
            else
            if (IsNumeric(txtdiastolic.Text) == false)
            {
                MessageBox.Show("Diastolic value should be numeric only!");
                return false;
            }

            if (txtPulse.Text.Equals(""))
            {
                MessageBox.Show("Pulse value cannot be empty");
                return false;
            }
            else
            if (IsNumeric(txtPulse.Text) == false)
            {
                MessageBox.Show("Pulse value should be numeric only!");
                return false;
            }

            return true;
        }

        void SaveApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (Validate() == false)
                return;

            string formattedTime = "";

            if (timeControl.Value.HasValue)
            {
                var timeSpan = TimeSpan.Parse(timeControl.Value.ToString());
                var hours = (Int32)timeSpan.Hours;
                var minutes = (Int32)timeSpan.Minutes;
                var amPmDesignator = "AM";
                if (hours == 0)
                    hours = 12;
                else if (hours == 12)
                    amPmDesignator = "PM";
                else if (hours > 12)
                {
                    hours -= 12;
                    amPmDesignator = "PM";
                }

                formattedTime = String.Format("{0}:{1:00} {2}", hours, minutes, amPmDesignator);
            }
            var dateTime = txtWhen.Text + " " + formattedTime;

            BloodPressureModel model = new BloodPressureModel();
            model.Systolic = int.Parse(txtSystolic.Text);
            model.Diastolic = int.Parse(txtdiastolic.Text);
            model.Pulse = int.Parse(txtPulse.Text);
            model.When = DateTime.Parse(dateTime);
            if (!string.IsNullOrEmpty(txtNote.Text))
            {
                model.Note = txtNote.Text.Trim();
            }
            HealthVaultMethods.PutThings(model, PutThingsCompleted);
            SetProgressBarVisibility(true);
        }

        private void DiastolicGraphBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/DiastolicGraph.xaml", UriKind.RelativeOrAbsolute);
            Navigate(pageUri);  
        }

        private void SystolicGraphBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/SystolicGraph.xaml", UriKind.RelativeOrAbsolute);
            Navigate(pageUri);           
        }

        void Navigate(Uri pageUri)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });
        }
    }
}