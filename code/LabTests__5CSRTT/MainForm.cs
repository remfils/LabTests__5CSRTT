using LabTests__5CSRTT.Middleware;
using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Protocols;
using LabTests__5CSRTT.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace LabTests__5CSRTT
{
    public partial class MainForm : Form
    {
        private AppSettingsModel _settings;

        private AnalysisFileService _analysisFileService;
        private SeriesDataFileService _seriesDataFileService;
        private ClockService _clockService;
        private SerialPortService_5CSRTT _deviceService;


        private List<string> _loadedAnalyses = new List<string>();
        private IProtocol_5CSRTT _currentProtocol;
        private AnalysisModel _currentAnalysis;
        private SeriesModel _currentSeries;
        private System.Timers.Timer _updateEventTableFromRegisteredEventsTimer;
        private int _registeredEventCount;
        private ConcurrentDictionary<int, EventRecordModel> _registeredEvents;

        public MainForm()
        {
            if (!DesignMode)
            {
                InitializeConfig();
            }

            InitializeComponent();

            InitializeLog();
            InitializeServices();


            if (!DesignMode)
            {
                var nameColumn = LoadedAnalysesListView.Columns[1];
                nameColumn.Width = -2;

                var dataColumn = SeriesEventListView.Columns[3];
                dataColumn.Width = -2;

                UpdateAnalysisListView();
                UpdateAnalysisTabsVisibility();
                UpdatePortComboboxFromPorts();
                SetEnabledStateForProtocolStartButton(true);
                UpdateConnectionButtonState(true);

                InputParametersDataGridView.ColumnHeadersDefaultCellStyle.BackColor = AppConstants.VAR_HL_COLOR;

                Log.Information("Started application");
                SetStatusMessage("Started app");
            }
        }

        private void LoadedAnalysesListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            ListView item = sender as ListView;
            if (item.SelectedIndices.Contains(e.ItemIndex))
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);
            }

            //e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font,
            //                      new SolidBrush(combo.ForeColor),
            //                      new Point(e.Bounds.X, e.Bounds.Y)
            //                     );

            //if (e. >= 0)
            //{
            //    ListViewItem combo = sender as ListViewItem;
            //    if (e.Index == combo.SelectedIndex)
            //    {
            //        e.Graphics.FillRectangle(new SolidBrush(Color.Gray), e.Bounds);
            //        if (lastDrawn != null)
            //            lastDrawn.Graphics.FillRectangle(new SolidBrush(combo.BackColor),
            //                                         lastDrawn.Bounds
            //                                        );
            //        lastDrawn = e;
            //    }
            //    else
            //        e.Graphics.FillRectangle(new SolidBrush(combo.BackColor),
            //                                 e.Bounds
            //                                );

            //    e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font,
            //                          new SolidBrush(combo.ForeColor),
            //                          new Point(e.Bounds.X, e.Bounds.Y)
            //                         );
            //}
        }

        public void SetAnalysis(AnalysisModel analysis)
        {
            _currentAnalysis = analysis;
            _currentProtocol = DataRepository.CreateProtocolById(analysis.ProtocolId);

            if (_currentProtocol == null)
            {
                Log.Error($"Protocol not defined for ID [{analysis.ProtocolId}]");
                return;
            }

            if (_currentAnalysis.ProtocolParameters == null)
            {
                _currentAnalysis.ProtocolParameters = _currentProtocol.GetProtocolDefaultParameterValues();
            }

            if (analysis.Series != null)
            {
                var itemWithMaxId = analysis.Series.OrderByDescending(x => x.Id).First();
                analysis.LastSeriesId = itemWithMaxId.Id;
            }
            else
            {
                analysis.Series = new List<SeriesModel>();
            }

            UpdateAnalysisTabsVisibility();

            SetStatusMessage($"Loaded analysis [{analysis.Name}] with [{analysis.Series.Count}] series");

            AnalysisNameLabel.Text = analysis.Name;
            AnalysisCreatedLabel.Text = analysis.CreatedDate.ToString();
            AnalysisLastModifiedLabel.Text = analysis.ModificationDate.ToString();

            AnalysisProtocolNameLabel.Text = _currentProtocol.GetName();

            ProtocolDescriptionTextBox.Text = _currentProtocol.GetDescription();

            // highliht parameters
            // TODO: something wrong with first highlight

            int characterStartHlIndex = 0;
            int characterEventStartHlIndex = 0;

            // NOTE: first selection is a hack
            ProtocolDescriptionTextBox.Select(0, 0);
            ProtocolDescriptionTextBox.SelectionBackColor = AppConstants.VAR_HL_COLOR;

            for (int charIndex = 0; charIndex < ProtocolDescriptionTextBox.Text.Length; charIndex++)
            {
                char character = ProtocolDescriptionTextBox.Text[charIndex];
                if (character == '[')
                {
                    characterStartHlIndex = charIndex;
                }
                else if (character == ']')
                {
                    ProtocolDescriptionTextBox.Select(characterStartHlIndex, charIndex - characterStartHlIndex + 1);
                    ProtocolDescriptionTextBox.SelectionBackColor = AppConstants.VAR_HL_COLOR;
                }
                else if (character == '{')
                {
                    characterEventStartHlIndex = charIndex;
                }
                else if (character == '}')
                {
                    ProtocolDescriptionTextBox.Select(characterEventStartHlIndex, charIndex - characterEventStartHlIndex + 1);
                    ProtocolDescriptionTextBox.SelectionBackColor = AppConstants.EVENT_HL_COLOR;
                }
            }

            FillInputParametersGridViewFromParameterDictionary(analysis.ProtocolParameters);

            SeriesEventListView.Items.Clear();
            FillSeriesCombobox(analysis.Series);
        }

        public void SetEnabledStateForProtocolStartButton(bool state)
        {
            ProtocolStartButton.Enabled = state;
            ProtocolStopButton.Enabled = !state;
        }

        public void SetEnabledStateForInputUI(bool state)
        {
            InputParametersDataGridView.Enabled = state;
            CreateNewAnalysysButton.Enabled = state;
            LoadedAnalysesListView.Enabled = state;

            PortCombobox.Enabled = state;
            AnalysisSeriesComboBox.Enabled = state;
            RefreshPortsButton.Enabled = state;

            // TODO: block stat menu strip

            UpdateConnectionButtonState(state);
        }

        private void UpdateConnectionButtonState(bool state)
        {
            ConnectButton.Enabled = state && !_deviceService.IsConnected;
            DisconnectButton.Enabled = state && _deviceService.IsConnected;
        }

        public void LoadAnalysisFile(string analysisFileName)
        {
            var filename = Path.Join(_settings.data_directory, analysisFileName + ".json");
            var analysis = JsonConvert.DeserializeObject<AnalysisModel>(File.ReadAllText(filename));
            if (analysis != null)
            {
                SetAnalysis(analysis);
            }
            else
            {
                Log.Error($"Error reading anaylysis file [{filename}]");
            }
        }

        public void UpdateAnalysisListView()
        {
            _loadedAnalyses = _analysisFileService.ReadAnalysesOrderedFileNames();

            LoadedAnalysesListView.Items.Clear();
            int rowCounter = 1;
            foreach (var analysisFileName in _loadedAnalyses)
            {
                var vals = new string[] {
                    rowCounter.ToString(),
                    analysisFileName
                };
                var item = new ListViewItem(vals);
                LoadedAnalysesListView.Items.Add(item);
                rowCounter++;
            }
        }

        public void UpdateAnalysisTabsVisibility()
        {
            if (_currentAnalysis != null)
            {
                if (AnalysisTabControl.TabPages.IndexOf(TabPage_Description) == -1)
                {
                    AnalysisTabControl.TabPages.Insert(0, TabPage_Description);
                }

                if (AnalysisTabControl.TabPages.IndexOf(TabPage_Info) == -1)
                {
                    AnalysisTabControl.TabPages.Insert(0, TabPage_Info);
                }

                if (AnalysisTabControl.TabPages.IndexOf(TabPage_Training) == -1)
                {
                    AnalysisTabControl.TabPages.Insert(0, TabPage_Training);
                }

                AnalysisTabControl.SelectedTab = AnalysisTabControl.TabPages[0];
            }
            else
            {
                AnalysisTabControl.TabPages.Remove(TabPage_Training);
                AnalysisTabControl.TabPages.Remove(TabPage_Description);
                AnalysisTabControl.TabPages.Remove(TabPage_Info);
            }
        }

        public void FillInputParametersGridViewFromParameterDictionary(Dictionary<string, string> protocolParameters)
        {
            InputParametersDataGridView.Rows.Clear();

            int rowCounter = 1;
            var parameterDefinitions = _currentProtocol.GetParametersDefentitions();
            foreach (var protocolParameter in protocolParameters)
            {
                var def = parameterDefinitions.GetValueOrDefault(protocolParameter.Key, null);

                if (def != null)
                {
                    var listViewData = new string[] {
                        rowCounter.ToString(),
                        def.Name,
                        protocolParameter.Value,
                        def.Units,
                        def.Code
                    };

                    InputParametersDataGridView.Rows.Add(listViewData);

                    rowCounter++;
                }
                else
                {
                    Log.Warning($"Parameter definition not found for key [{protocolParameter.Key}]");
                }
            }
        }

        public void FillSeriesCombobox(List<SeriesModel> series)
        {
            AnalysisSeriesComboBox.Items.Clear();

            AnalysisSeriesComboBox.DisplayMember = "Value";
            AnalysisSeriesComboBox.ValueMember = "Key";

            AnalysisSeriesComboBox.Items.Add(new KeyValuePair<SeriesModel?, string>(null, ""));

            if (series != null)
            {
                foreach (SeriesModel seriesModel in series.OrderByDescending(x => x.StartDate))
                {
                    string displayValue = "#" + seriesModel.Id.ToString() + " from " + seriesModel.StartDate.ToString();
                    AnalysisSeriesComboBox.Items.Add(new KeyValuePair<SeriesModel, string>(seriesModel, displayValue));
                }
            }
        }

        public void SetStatusMessage(string status)
        {
            Status_MessageStatusLabel.Text = "> " + status;
        }

        public void AppendSeriesListViewItems(IDictionary<int, EventRecordModel> registeredEvents)
        {
            foreach (var keyPair in registeredEvents)
            {
                var evt = keyPair.Value;
                var data = new string[] {
                    evt.EventType,
                    evt.SecondsTrial.ToString("0.00", CultureInfo.InvariantCulture),
                    evt.SecondsTotal.ToString("0.00", CultureInfo.InvariantCulture),
                    evt.MillisecondsTotal.ToString(),
                    evt.Parameters
                };
                var item = new ListViewItem(data);
                SeriesEventListView.Items.Add(item);
            }
            var dataColumn = SeriesEventListView.Columns[3];
            dataColumn.Width = -2;
        }

        private void UpdatePortComboboxFromPorts()
        {
            var ports = DataRepository.GetSerialPorts();
            PortCombobox.Items.Clear();
            foreach (var port in ports)
            {
                PortCombobox.Items.Add(port);
            }
        }

        #region init

        private void InitializeConfig()
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.SetBasePath(System.IO.Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile(path: "config.json", optional: true, reloadOnChange: false);

            var config = configBuilder.Build();

            _settings = new AppSettingsModel(config);
        }

        private void InitializeLog()
        {
            var loggerConfig = new LoggerConfiguration();

            var logEventSink = new StringEventLoggerSink(LogMessageEventHandler);
            loggerConfig.WriteTo.Sink(logEventSink);

            var logFilename = _settings.log_file_name;
            var currentDateISO = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            currentDateISO = currentDateISO.Substring(0, currentDateISO.IndexOf(".")).Replace(":", "_");
            logFilename = logFilename.Replace("{date}", currentDateISO);
            logFilename = System.IO.Path.Join(_settings.log_directory, logFilename);

            loggerConfig.WriteTo.File(logFilename);

            switch (_settings.log_level.ToLowerInvariant())
            {
                case "information":
                    loggerConfig.MinimumLevel.Information();
                    break;
                case "warning":
                    loggerConfig.MinimumLevel.Warning();
                    break;
                case "error":
                    loggerConfig.MinimumLevel.Error();
                    break;
                case "debug":
                    loggerConfig.MinimumLevel.Debug();
                    break;
                default:
                    loggerConfig.MinimumLevel.Information();
                    break;
            }

            Log.Logger = loggerConfig.CreateLogger();

            Log.Debug("Loaded configuration:\r\n" + JsonConvert.SerializeObject(_settings));
        }

        private void InitializeServices()
        {
            _analysisFileService = new AnalysisFileService(_settings);
            _seriesDataFileService = new SeriesDataFileService(_settings);
            _clockService = new ClockService(_settings);
            _deviceService = new SerialPortService_5CSRTT();
        }

        #endregion init


        #region form_event_handlers

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_currentProtocol?.IsRunning ?? false)
            {
                var window = MessageBox.Show(
                    "Close the window?",
                    "You have a protocol running. Stop protocol and close?",
                    MessageBoxButtons.YesNo);

                if (window == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Log.CloseAndFlush();

            _currentProtocol?.StopProtocolThread();

            _analysisFileService?.Dispose();
            _seriesDataFileService?.Dispose();
            _deviceService?.Dispose();
        }

        #endregion form_event_handlers

        #region element_event_handlers

        private void CreateNewAnalysysButton_Click(object sender, EventArgs e)
        {
            var dialogForm = new CreateNewAnalysisForm(_loadedAnalyses.Count + 1, _settings, AnalysisCreatedEventHanlder);
            dialogForm.StartPosition = FormStartPosition.CenterParent;
            dialogForm.ShowDialog(this);
            dialogForm.Dispose();
        }

        private void LoadedAnalysesListView_Click(object sender, EventArgs e)
        {
            if (LoadedAnalysesListView.SelectedIndices.Count > 0)
            {
                var selectedIndex = LoadedAnalysesListView.SelectedIndices[0];
                var analysisFilename = LoadedAnalysesListView.Items[selectedIndex]?.SubItems[1]?.Text ?? null;

                if (analysisFilename != null)
                {
                    LoadAnalysisFile(analysisFilename);
                }
            }
        }

        private void LoadedAnalysesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void InputParametersDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            InputParametersDataGridView.BeginEdit(true);
        }

        private void RefreshPortsButton_Click(object sender, EventArgs e)
        {
            UpdatePortComboboxFromPorts();
        }

        private void AnalysisSeriesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (KeyValuePair<SeriesModel, string>)AnalysisSeriesComboBox.SelectedItem;
            _currentSeries = item.Key;

            SeriesEventListView.Items.Clear();

            if (_currentSeries != null && _currentSeries.EndDate.HasValue)
            {
                FillInputParametersGridViewFromParameterDictionary(_currentSeries.RunParameters);

                Dictionary<int, EventRecordModel> series = _seriesDataFileService.LoadAsDictionary(_currentSeries.SeriesFileName);

                AppendSeriesListViewItems(series);
            }
            else
            {
                FillInputParametersGridViewFromParameterDictionary(_currentAnalysis.ProtocolParameters);
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {

        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string? portName = null;
            if (PortCombobox.SelectedIndex != -1)
            {
                portName = PortCombobox.SelectedItem.ToString();
            }

            if (portName != null)
            {
                _deviceService.ConnectAndStartReading(portName);

                if (!_deviceService.IsConnected)
                {
                    MessageBox.Show("Failed to connect to selected port", "Error", MessageBoxButtons.OK);
                }

                Status_ConnectionStatusLabel.Text = "Connected";
            }
            else
            {
                MessageBox.Show("No port name specified", "Error", MessageBoxButtons.OK);
            }

            UpdateConnectionButtonState(true);
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            _deviceService.Disconnect();
            UpdateConnectionButtonState(true);
            Status_ConnectionStatusLabel.Text = "Not connected";
        }

        private void DeviceControlFormLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_deviceService.IsConnected)
            {
                var form = new DeviceControl_5CSRTForm(_deviceService);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(this);
                form.Dispose();
            }
            else
            {
                MessageBox.Show(this, "Please, connect device via port", "Error", MessageBoxButtons.OK);
            }
        }

        private void InputParametersDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (InputParametersDataGridView.EditingControl == null)
                {
                    if (InputParametersDataGridView.CurrentCell.ColumnIndex == 2)
                    {
                        InputParametersDataGridView.BeginEdit(true);
                        e.Handled = true;
                    }
                }
                else
                {
                    InputParametersDataGridView.EndEdit();
                }
            }
        }

        private void exportDataToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_currentSeries == null)
            {
                MessageBox.Show("No data selected to export", "Error", MessageBoxButtons.OK);
                return;
            }

            SaveFileDialog saveSeriesDialog = new SaveFileDialog();
            saveSeriesDialog.Filter = "csv file|*.csv";
            saveSeriesDialog.Title = "Export event log";
            saveSeriesDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveSeriesDialog.FileName != "")
            {
                try
                {
                    switch (saveSeriesDialog.FilterIndex)
                    {
                        case 1: // csv
                            string filename = _seriesDataFileService.GetFileName(_currentSeries.SeriesFileName);
                            File.Copy(filename, saveSeriesDialog.FileName, true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error occured: {ex.Message}");
                    SetStatusMessage("Error ocured when saving file. See logs");
                }
            }
        }

        private void simpleReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentSeries == null)
            {
                MessageBox.Show("No data selected to export", "Error", MessageBoxButtons.OK);
                return;
            }

            SaveFileDialog saveSeriesDialog = new SaveFileDialog();
            saveSeriesDialog.Filter = "csv file|*.csv";
            saveSeriesDialog.Title = "Save Simple Report";
            saveSeriesDialog.ShowDialog();

            if (saveSeriesDialog.FileName != "")
            {
                try
                {
                    switch (saveSeriesDialog.FilterIndex)
                    {
                        case 1: // csv
                            var simpleReportService = new SimpleStatReportService(_settings);

                            var data = _seriesDataFileService.LoadAsDictionary(_currentSeries.SeriesFileName);

                            simpleReportService.CalculateAndSaveReportToFile(_currentProtocol.GetId(), data, saveSeriesDialog.FileName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error occured: {ex.Message}");
                    SetStatusMessage("Error ocured when saving file. See logs");
                }
            }
        }

        #endregion element_event_handlers

        #region thread_safe_wrappers

        private void TrheadSafeSetEnabledStateForProtocolStartButton(bool state)
        {
            if (ProtocolStartButton.InvokeRequired)
            {
                ProtocolStartButton.Invoke(new UpdateUIStateDelegate(this.SetEnabledStateForProtocolStartButton), new object[] { state });
            }
            else
            {
                SetEnabledStateForProtocolStartButton(state);
            }
        }

        public void ThreadSafeSetEnabledStateForInputUI(bool state)
        {
            if (InputParametersDataGridView.InvokeRequired)
            {
                InputParametersDataGridView.Invoke(new UpdateUIStateDelegate(this.SetEnabledStateForInputUI), new object[] { state });
            }
            else
            {
                SetEnabledStateForInputUI(state);
            }
        }

        public void ThreadSafeAppendSeriesListViewItems(IDictionary<int, EventRecordModel> registeredEvents)
        {
            if (SeriesEventListView.InvokeRequired)
            {
                SeriesEventListView.Invoke(new AppendSeriesListViewItemsDelegate(this.AppendSeriesListViewItems), new object[] { registeredEvents });
            }
            else
            {
                AppendSeriesListViewItems(registeredEvents);
            }
        }

        #endregion thread_safe_wrappers

        #region app_event_handlers

        private void LogMessageEventHandler(string message)
        {
            // TODO: clear text area with rotation?
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.Invoke(new AppendLogMessageToTextBoxDelegate(this.LogMessageEventHandler), new object[] { message });
            }
            else
            {
                LogTextBox.AppendText(message);
            }

        }

        private void StatusUpdateEventHandler(string status)
        {
            if (LogTextBox.InvokeRequired)
            {
                Status_MessageStatusLabel.Invoke(new AppendLogMessageToTextBoxDelegate(this.SetStatusMessage), new object[] { status });
            }
            else
            {
                SetStatusMessage(status);
            }
        }

        private bool AnalysisCreatedEventHanlder(CreateNewAnalysisForm sender, AnalysisCreateModel analysisCreate)
        {
            var result = true;

            result = analysisCreate.Name.Count() > 0;
            if (result)
            {
                result = !_loadedAnalyses.Any(name => analysisCreate.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (result)
                {
                    var analysis = new AnalysisModel
                    {
                        Name = analysisCreate.Name,
                        ProtocolId = analysisCreate.ProtocolId,
                        CreatedDate = DateTime.Now,
                        ModificationDate = DateTime.Now,
                    };

                    result = _analysisFileService.SaveAnalysis(analysis, false);

                    if (result)
                    {
                        UpdateAnalysisListView();

                        SetAnalysis(analysis);

                        Log.Information("Created analysis");
                    }
                    else
                    {
                        Log.Warning($"File error");
                        sender.ShowError("Failed to write data to file");
                    }
                }
                else
                {
                    Log.Warning($"Name must be unique [{analysisCreate.Name}]");
                    sender.ShowError("File name must be unique");
                }
            }
            else
            {
                Log.Warning($"Empty result name");
                sender.ShowError("File name can't be empty");
            }

            return result;
        }

        private void UpdateEventTableFromRegisteredEventTimerEventHandler(object? sender, ElapsedEventArgs e)
        {
            ThreadSafeAppendSeriesListViewItems(_registeredEvents);
            _registeredEvents.Clear();
        }

        #endregion app_event_handlers

        #region protocol_event_handlers

        private void CurrentProtocolFinishedEventHandler()
        {
            _currentSeries.EndDate = DateTime.Now;

            _seriesDataFileService.StopFileWrite();
            _clockService.StopClock();

            _updateEventTableFromRegisteredEventsTimer.Stop();
            _updateEventTableFromRegisteredEventsTimer.Dispose();

            ThreadSafeAppendSeriesListViewItems(_registeredEvents);
            _registeredEvents.Clear();

            bool isOk = _analysisFileService.SaveAnalysis(_currentAnalysis, true);

            if (isOk)
            {
                TrheadSafeSetEnabledStateForProtocolStartButton(true);

                ThreadSafeSetEnabledStateForInputUI(true);
            }
        }

        private long _lastTrialMiliseconds = 0;
        private void CurrentProtocolEventHandler(string eventCode, string eventParams)
        {
            Log.Debug($"event handler {eventCode}");

            var eventRecord = new EventRecordModel
            {
                EventType = eventCode,
                Parameters = eventParams
            };
            _clockService.RegisterChange(out eventRecord.MillisecondsTotal);

            eventRecord.SecondsTotal = DataRepository.MillisecondsToSeconds(eventRecord.MillisecondsTotal);

            if (Object.ReferenceEquals(eventCode, AppConstants.PROTOCOL_EVENT__TRIAL_START))
            {
                if (_lastTrialMiliseconds == 0)
                {
                    _lastTrialMiliseconds = 1;
                    eventRecord.SecondsTrial = DataRepository.MillisecondsToSeconds(eventRecord.MillisecondsTotal); ;
                }
                else
                {
                    _lastTrialMiliseconds = eventRecord.MillisecondsTotal;
                }
            }
            else
            {
                eventRecord.SecondsTrial = DataRepository.MillisecondsToSeconds(eventRecord.MillisecondsTotal - _lastTrialMiliseconds);
            }

            _seriesDataFileService.WriteEventRecord(eventRecord);
            Log.Debug($"File write {eventCode}");

            if (!_registeredEvents.TryAdd(_registeredEventCount++, eventRecord))
            {
                Log.Error("Something is wrong why adding record to dictionary");
            }
            else
            {
                Log.Debug($"Added event {eventCode}");
            }
        }

        #endregion protocol_event_handlers


        private void ProtocolStartButton_Click(object sender, EventArgs e)
        {
            // init params
            _lastTrialMiliseconds = 0;

            // parse parameters for protocol, validate them and set them
            var parameterDictionary = new Dictionary<string, string>();

            foreach (DataGridViewRow row in InputParametersDataGridView.Rows)
            {
                var parameterCode = row.Cells["ParameterCode"]?.Value.ToString() ?? "";
                var parameterValue = row.Cells["ParameterValue"]?.Value.ToString() ?? "";

                if (parameterCode.Count() != 0)
                {
                    parameterDictionary[parameterCode] = parameterValue;
                }
            }

            Dictionary<string, string> validationErrors = new Dictionary<string, string>();
            _currentProtocol.SetParametersFromDictionary(parameterDictionary, validationErrors);

            if (validationErrors.Count == 0)
            {
                _registeredEventCount = 0;
                _registeredEvents = new ConcurrentDictionary<int, EventRecordModel>();

                _updateEventTableFromRegisteredEventsTimer = new System.Timers.Timer();
                _updateEventTableFromRegisteredEventsTimer.Elapsed += UpdateEventTableFromRegisteredEventTimerEventHandler;
                _updateEventTableFromRegisteredEventsTimer.Interval = 500;
                _updateEventTableFromRegisteredEventsTimer.Start();

                // create series object and add to analysis
                _currentAnalysis.ProtocolParameters = _currentProtocol.GetProtocolParameterValues();
                FillInputParametersGridViewFromParameterDictionary(_currentAnalysis.ProtocolParameters);

                _currentSeries = new SeriesModel
                {
                    StartDate = DateTime.Now,
                    RunParameters = _currentAnalysis.ProtocolParameters.ToDictionary(x => x.Key, x => x.Value)
                };

                if (_currentAnalysis.Series == null)
                {
                    _currentAnalysis.Series = new List<SeriesModel>();
                }

                ++_currentAnalysis.LastSeriesId;
                _currentSeries.Id = _currentAnalysis.LastSeriesId;
                _currentSeries.SetFileNameFromAnalysisName(_currentAnalysis.Name);
                _currentAnalysis.Series.Add(_currentSeries);

                bool isOk = _analysisFileService.SaveAnalysis(_currentAnalysis, true);

                if (isOk)
                {
                    // update series combox
                    FillSeriesCombobox(_currentAnalysis.Series);
                    AnalysisSeriesComboBox.SelectedIndex = 1;

                    // block all user UI
                    TrheadSafeSetEnabledStateForProtocolStartButton(false);

                    ThreadSafeSetEnabledStateForInputUI(false);

                    _seriesDataFileService.StartFileWrite(_currentSeries.SeriesFileName);

                    _clockService.StartClock();

                    // set status

                    _currentProtocol.ClearEventHandlers();

                    _currentProtocol.UpdateStatusEvent += StatusUpdateEventHandler;
                    _currentProtocol.RegisterEvent += CurrentProtocolEventHandler;
                    _currentProtocol.ProtocolFinishedEvent += CurrentProtocolFinishedEventHandler;
                    _currentProtocol.StartProtocolThread(_deviceService);

                    // run paradigm object with finish callback in separate thread?
                }
            }
            else
            {
                foreach (DataGridViewRow row in InputParametersDataGridView.Rows)
                {
                    var parameterCode = row.Cells["ParameterCode"]?.Value.ToString() ?? "";

                    if (parameterCode.Count() != 0 && validationErrors.ContainsKey(parameterCode))
                    {
                        row.Cells["ParameterValue"].ErrorText = validationErrors[parameterCode];
                    }
                }
            }
        }

        private void ProtocolStopButton_Click(object sender, EventArgs e)
        {
            _currentProtocol.StopProtocolThread();
        }
    }
}
