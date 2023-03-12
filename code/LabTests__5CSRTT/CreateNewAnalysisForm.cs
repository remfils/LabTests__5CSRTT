using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Protocols;
using LabTests__5CSRTT.Service;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabTests__5CSRTT
{
    public partial class CreateNewAnalysisForm : Form
    {
        public List<string> AnalysesNames { get; }

        public delegate bool AnalysisCreatedDelegate(CreateNewAnalysisForm sender, AnalysisCreateModel model);
        public event AnalysisCreatedDelegate AnalysisCreatedEvent;

        public CreateNewAnalysisForm()
        {

        }

        public CreateNewAnalysisForm(int index, AppSettingsModel config, AnalysisCreatedDelegate createdEventHandler)
        {
            InitializeComponent();

            AnalysisCreatedEvent += createdEventHandler;

            NameTextBox.Text = config.default_analysis_name
                .Replace("{index}", index.ToString());

            ProtocolIdComboBox.DisplayMember = "Value";
            ProtocolIdComboBox.ValueMember = "Key";

            var protocols = new List<IProtocol_5CSRTT> { 
                new HabitationMagazineTrainingProtocol__house_light(),
                new FixedIntervalTrainingProtocol(),
                new PeakIntervalTrainingProtocol(),
                new HabitationMagazineTrainingProtocol__cue_light(),
            };

            foreach (var protocol in protocols)
            {
                ProtocolIdComboBox.Items.Add(new KeyValuePair<int, string>(protocol.GetId(), protocol.GetName()));
            }

            ProtocolIdComboBox.SelectedIndex = 0;
        }

        public void ShowError(string error)
        {
            ErrorLabel.Text = error;
        }

        #region button_event_handlers

        private void CreateButton_Click(object sender, EventArgs e)
        {
            var analysis = new AnalysisCreateModel
            {
                Name = NameTextBox.Text,
            };

            var selectedProtocolItem= (KeyValuePair<int, string>)ProtocolIdComboBox.SelectedItem;
            analysis.ProtocolId = selectedProtocolItem.Key;

            var isOK = AnalysisCreatedEvent?.Invoke(this, analysis) ?? false;
            if (isOK)
            {
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion button_event_handlers
    }
}
