using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace BloodPressureMeasurement
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            textBlock1.Text = @"This application requires a connection to the Microsoft"+
                              @" HealthVault Service to store and retrieve the blood pressure information. You will need" +
                              @" to sign up for a free HealthVault account and authorize this" +
                              @" application to access blood pressure readings from your HealthVault" +
                              @" record. Press 'I Agree' button to begin.";
            button1.Click += new RoutedEventHandler(authenticateHealthVault);
        }

        void authenticateHealthVault(object sender, RoutedEventArgs e)
        {
            Uri pageUri = new Uri("/MyBloodPressure.xaml", UriKind.RelativeOrAbsolute);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });
        }
    }
}