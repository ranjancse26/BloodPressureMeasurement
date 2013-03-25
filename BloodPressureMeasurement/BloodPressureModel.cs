using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Xml.Linq;
using System.Linq;

namespace BloodPressureMeasurement
{
    /// <summary>
    ///  Data Model for BloodPressure.
    ///  This model should enable - 
    ///     1. Loading an bloodpressure state from existing HealthVault XML
    ///     2. Creating an bloodpressure state object to be store in HealthVault
    ///     3. Expose the important properties of bloodpressure state available in HealthVault SDK
    /// </summary>
    public class BloodPressureModel : HealthRecordItemModel
    {
        public readonly static string TypeId = "ca3c57f4-f4c1-4e15-be67-0a3caf5414ed";
        private string thingXml =
            @"<info><thing>
                <type-id>ca3c57f4-f4c1-4e15-be67-0a3caf5414ed</type-id>
                <thing-state>Active</thing-state>
                <flags>0</flags>
                <data-xml>
                <blood-pressure>
                <when>
                    <date>
                        <y>{0}</y>
                        <m>{1}</m>
                        <d>{2}</d>
                    </date>
                    <time>
                        <h>{3}</h>
                        <m>{4}</m>
                        <s>{5}</s>
                        <f>{6}</f>
                    </time>             
                </when>
                <systolic>{7}</systolic>
                <diastolic>{8}</diastolic>
                <pulse>{9}</pulse>
                <irregular-heartbeat>{10}</irregular-heartbeat>
                </blood-pressure>
                <common>
                    <source>Blood Pressure Measurement Tracker for Windows Phone</source>
                    <note>{11}</note>
                  </common>
                </data-xml>
                </thing></info>";

        public void Parse(XElement thingXml)
        {
            XElement bloodPressureState = thingXml.Descendants("data-xml").Descendants("blood-pressure").First();

            this.When = Convert.ToDateTime(thingXml.Element("eff-date").Value);

            if (thingXml.Descendants("common") != null &&
                    (thingXml.Descendants("common").Descendants("note").Count() != 0))
            {
                this.Note = thingXml.Descendants("common").Descendants("note").First().Value;
            }

            if (bloodPressureState.Element("systolic") != null)
            {
                try
                {
                    this.Systolic = int.Parse(bloodPressureState.Element("systolic").Value);
                }
                catch (Exception) { }
            }
            if (bloodPressureState.Element("diastolic") != null)
            {
                try
                {
                    this.Diastolic = int.Parse(bloodPressureState.Element("diastolic").Value);
                }
                catch (Exception) { }
            }
            if (bloodPressureState.Element("pulse") != null)
            {
                try
                {
                    this.Pulse = int.Parse(bloodPressureState.Element("pulse").Value);
                }
                catch (Exception) { }
            }
            if (bloodPressureState.Element("irregular-heartbeat") != null)
            {
                try
                {
                    this.IrregularHeartbeat = bool.Parse(bloodPressureState.Element("irregular-heartbeat").Value);
                }
                catch (Exception) { }
            }
            else
                this.IrregularHeartbeat = false;
        }

        /// <summary>
        /// Get the Xml representing this type
        /// </summary>
        /// <returns></returns>
        public override string GetXml()
        {
            int irregularHeartbeat = 0;
            if (IrregularHeartbeat)
                irregularHeartbeat = 1;

            return string.Format(
                thingXml,
                When.Year,
                When.Month,
                When.Day,
                When.Hour,
                When.Minute,
                When.Second,
                When.Millisecond,
                (this.Systolic).ToString(),
                (this.Diastolic).ToString(),
                (this.Pulse).ToString(),
                irregularHeartbeat,
                this.Note);
        }

        public String FormattedWhen
        {
            get{
                return String.Format("{0:dd/MM}",When);
            }
        }
        public DateTime When { get; set; }       
        public string Note { get; set; }
        public int Systolic { get; set; }
        public int Diastolic { get; set; }
        public int Pulse { get; set; }
        public bool IrregularHeartbeat { get; set; }
    }  
}
