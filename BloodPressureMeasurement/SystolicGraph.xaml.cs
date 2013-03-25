using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml.Linq;
using Microsoft.Health.Mobile;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BloodPressureMeasurement
{
    public partial class SystolicGraph : PhoneApplicationPage
    {
        ObservableCollection<GraphItem> _data = new ObservableCollection<GraphItem>();
        
        public SystolicGraph()
        {
            InitializeComponent();
            LayoutRoot.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("Background.png", UriKind.Relative))
            };
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                progressOverlay.Show();
            });

            if (App.HealthVaultService.CurrentRecord != null)
            {
                // Get all readings
                HealthVaultMethods.GetThings(BloodPressureModel.TypeId, null, null, null, GetThingsCompleted);               
            }
        }

        public ObservableCollection<GraphItem> Data { get { return _data; } }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                // using LINQ to get the latest reading of blood pressure state
                var readings = (from thingNode in responseNode.Descendants("thing")
                                orderby Convert.ToDateTime(thingNode.Element("eff-date").Value) ascending
                                select thingNode).ToList();

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    BloodPressureModel bloodPressureState = new BloodPressureModel();
                    foreach (var item in readings)
                    {
                        bloodPressureState.Parse(item);
                        _data.Add(new GraphItem
                                     {
                                         Name = bloodPressureState.When.ToShortDateString(),
                                         Value = bloodPressureState.Systolic.ToString()
                                     });
                    }

                    progressOverlay.Hide();
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(e.ErrorText);
                });
            }
        }   
    }
}