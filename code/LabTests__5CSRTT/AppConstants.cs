using LabTests__5CSRTT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT
{
    public delegate void AppendLogMessageToTextBoxDelegate(string text);

    public delegate void UpdateStatusDelegate(string status);

    public delegate void UpdateUIStateDelegate(bool state);

    public delegate void RegisterEventDelegate(string eventCode, string eventParams);

    public delegate void ProtocolFinishedCallbackDelegate();

    public delegate void AppendSeriesListViewItemsDelegate(IDictionary<int, EventRecordModel> registeredEvents);

    public delegate void RegisterParsedMessageDelegate(Message_5CSRTT message);

    public class AppConstants
    {

        public const int PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__CUE_LIGHT = 1;
        public const int PROTOCOL_TYPE__FIXED_INTERVAL = 2;
        public const int PROTOCOL_TYPE__PEAK_INTERVAL = 3;
        public const int PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__HOUSE_LIGHT = 4;



        public const int PARAMETER_VALUE_TYPE__NUMBER = 1;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // device messages
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public const byte DEVICE_MESSAGE_CATEGORY__HOLE_CUE_LIGHTS = 0x01;
        public const byte DEVICE_MESSAGE_CATEGORY__SHOCKER = 0x03;
        public const byte DEVICE_MESSAGE_CATEGORY__CURRENT_MA = 0x04;
        public const byte DEVICE_MESSAGE_CATEGORY__PANEL_CUE_LIGHT = 0x06;
        public const byte DEVICE_MESSAGE_CATEGORY__PELLETS = 0x08;
        public const byte DEVICE_MESSAGE_CATEGORY__PELLET_RECEPTACLE_LIGHT = 0x09;
        public const byte DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_HOLE = 0x20;
        public const byte DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_PELLET = 0x21;
        public const byte DEVICE_MESSAGE_CATEGORY__HOUSE_LIGHT_IN_CUBICLE = 0x11;
        public const byte DEVICE_MESSAGE_CATEGORY__IR_LIGHT_IN_CUBICLE = 0x12;
        public const byte DEVICE_MESSAGE_CATEGORY__FAN_IN_CUBICLE = 0x13;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // app events
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public const string PROTOCOL_EVENT__TRIAL_START = "TRIAL_START";
        public const string PROTOCOL_EVENT__TRIAL_END = "TRIAL_END";


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // color scheme
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static Color VAR_HL_COLOR = Color.FromArgb(0xB4C7DC);
        public static Color EVENT_HL_COLOR = Color.FromArgb(212, 234, 107);


    }
}
