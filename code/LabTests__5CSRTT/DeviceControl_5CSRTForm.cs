using LabTests__5CSRTT.Service;
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
    public partial class DeviceControl_5CSRTForm : Form
    {
        public bool isCueLight_1_On = false;
        public bool isCueLight_2_On = false;
        public bool isCueLight_3_On = false;
        public bool isCueLight_4_On = false;
        public bool isCueLight_5_On = false;
        public bool isPanelCueLightOn = false;
        public bool isHouseLightInCubileOn = false;
        public bool isPelletReceptacleLightOn = false;
        public bool isHouseLightInCubicleOn = false;
        public bool is_IR_LightInCubicleOn = false;
        public bool isFanCubicleOn = false;

        private SerialPortService_5CSRTT SerialPortService_5CSRTT { get; }

        public DeviceControl_5CSRTForm(SerialPortService_5CSRTT serialPortService_5CSRTT)
        {
            InitializeComponent();
            SerialPortService_5CSRTT = serialPortService_5CSRTT;
        }


        private void ToggleCueLight_X_State(int cueLightIdx, ref bool state)
        {
            var isColorLight = CueLightRedInput.Value != 0
                || CueLightBlueInput.Value != 0
                || CueLightGreenInput.Value != 0;

            byte red = (byte)CueLightRedInput.Value;
            byte blue = (byte)CueLightBlueInput.Value;
            byte green = (byte)CueLightGreenInput.Value;

            if (isColorLight)
            {
                SerialPortService_5CSRTT.Send_SetState_HoleCueLight_Color(cueLightIdx, state, red, green, blue);
            }
            else
            {
                SerialPortService_5CSRTT.Send_SetState_HoleCueLight(cueLightIdx, state);
            }

            state = !state;
        }


        private void ToggleCueLight_1_Button_Click(object sender, EventArgs e)
        {
            ToggleCueLight_X_State(1, ref isCueLight_1_On);
        }

        private void ToggleCueLight_2_Button_Click(object sender, EventArgs e)
        {
            ToggleCueLight_X_State(2, ref isCueLight_2_On);
        }

        private void ToggleCueLight_3_Button_Click(object sender, EventArgs e)
        {
            ToggleCueLight_X_State(3, ref isCueLight_3_On);
        }

        private void ToggleCueLight_4_Button_Click(object sender, EventArgs e)
        {
            ToggleCueLight_X_State(4, ref isCueLight_4_On);
        }

        private void ToggleCueLight_5_Button_Click(object sender, EventArgs e)
        {
            ToggleCueLight_X_State(5, ref isCueLight_5_On);
        }

        private void TogglePanelCueLight_Button_Click(object sender, EventArgs e)
        {
            var isColorLight = OtherLightsRedInput.Value != 0
                || OtherLightsGreenInput.Value != 0
                || OtherLightsBlueInput.Value != 0;

            byte red = (byte)OtherLightsRedInput.Value;
            byte blue = (byte)OtherLightsBlueInput.Value;
            byte green = (byte)OtherLightsGreenInput.Value;

            if (isColorLight)
            {
                SerialPortService_5CSRTT.Send_SetState_PanelCueLights_Color(isPanelCueLightOn, red, green, blue);
            }
            else
            {
                SerialPortService_5CSRTT.Send_SetState_PanelCueLights(isPanelCueLightOn);
            }

            isPanelCueLightOn = !isPanelCueLightOn;
        }

        private void ActivatePelletDrop_Button_Click(object sernder, EventArgs e)
        {
            SerialPortService_5CSRTT.Send_SetState_Pelets();
        }

        private void TogglePelletReceptacleLight_Button_Click(object sender, EventArgs e)
        {
            var isColorLight = OtherLightsRedInput.Value != 0
                || OtherLightsGreenInput.Value != 0
                || OtherLightsBlueInput.Value != 0;

            byte red = (byte)OtherLightsRedInput.Value;
            byte blue = (byte)OtherLightsBlueInput.Value;
            byte green = (byte)OtherLightsGreenInput.Value;

            if (isColorLight)
            {
                SerialPortService_5CSRTT.Send_SetState_PelletReceptacleLight_Color(isPelletReceptacleLightOn, red, green, blue);
            }
            else
            {
                SerialPortService_5CSRTT.Send_SetState_PelletReceptacleLight(isPelletReceptacleLightOn);
            }

            isPelletReceptacleLightOn = !isPelletReceptacleLightOn;
        }

        private void ToggleHouseLightInCubicle_Button_Click(object sender, EventArgs e)
        {
            var isColorLight = OtherLightsRedInput.Value != 0
                || OtherLightsGreenInput.Value != 0
                || OtherLightsBlueInput.Value != 0;

            byte red = (byte)OtherLightsRedInput.Value;
            byte blue = (byte)OtherLightsBlueInput.Value;
            byte green = (byte)OtherLightsGreenInput.Value;

            if (isColorLight)
            {
                SerialPortService_5CSRTT.Send_SetState_HouseLightInCubicle_Color(isHouseLightInCubicleOn, red, green, blue);
            }
            else
            {
                SerialPortService_5CSRTT.Send_SetState_HouseLightInCubicle(isHouseLightInCubicleOn);
            }

            isHouseLightInCubicleOn = !isHouseLightInCubicleOn;
        }

        private void IR_LightInCubicleButton_Click(object sender, EventArgs e)
        {
            SerialPortService_5CSRTT.Send_SetState_IR_LightInCubicle(is_IR_LightInCubicleOn);
            is_IR_LightInCubicleOn = !is_IR_LightInCubicleOn;
        }

        private void FanCubicleButton_Click(object sender, EventArgs e)
        {
            SerialPortService_5CSRTT.Send_SetState_FanInCubicle(isFanCubicleOn);
            isFanCubicleOn = !isFanCubicleOn;
        }
    }
}
