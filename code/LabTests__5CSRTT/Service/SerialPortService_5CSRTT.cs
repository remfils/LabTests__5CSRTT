using LabTests__5CSRTT.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class SerialPortService_5CSRTT : IDisposable
    {
        public bool IsReading = false;
        public bool IsConnected = false;

        private string _portName;
        private SerialPort _port;
        private Thread _readThread;

        public event RegisterParsedMessageDelegate? Message_5CSRTTEvent;


        public SerialPortService_5CSRTT()
        {

        }

        public void ConnectAndStartReading(string portName)
        {
            _portName = portName;

            try
            {
                _port = new SerialPort(_portName, 115200, Parity.None, 8, StopBits.One);

                //_port.Handshake = Handshake.RequestToSend;
                //_port.DtrEnable = true;
                //_port.RtsEnable = true;
                //_port.WriteTimeout = 1000;

                _port.Open();
                IsConnected = true;

                StartReadingFromDeviceInThread();

                Log.Information($"Connected to port [{_portName}] and started reading messages");
            }
            catch (Exception e)
            {
                Log.Error("Error opening port: " + e.Message);
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
            IsReading = false;

            if (_port != null && _port.IsOpen)
            {
                _port.Close();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }


        #region serial_read

        public void StartReadingFromDeviceInThread()
        {
            if (!IsConnected)
            {
                Log.Warning("No connection started");
                return;
            }

            Log.Information($"Started reading from port [{_portName}]");

            IsReading = true;
            _readThread = new Thread(ReadSeriealPortBytes);
            _readThread.Start();
        }

        public void Dummy_Trigger__DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_PELLET()
        {
            var msg = new List<byte>
            {
                0xaa, 0xbb, 0x01, AppConstants.DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_PELLET, 0x00, 0x00, 0x00, 0x00, 0xcc, 0xdd
            };
            RegisterMessage(msg);
        }

        public void RegisterMessage(List<byte> byteList)
        {
            Log.Debug("Read message bytes: " + byteList.ToString());
            var message = new Message_5CSRTT(byteList);
            Message_5CSRTTEvent?.Invoke(message);
        }

        public void ReadSeriealPortBytes()
        {
            var byteList = new List<byte>();
            while (IsReading)
            {
                try
                {
                    var c = (byte)_port.ReadByte();

                    // NOTE: ignore everyting before start symbol: 0xaa
                    if (!byteList.Any() && c != 0xaa)
                    {
                        continue;
                    }

                    byteList.Add(c);

                    if (IsDeviceMessageCompleted(byteList))
                    {
                        RegisterMessage(byteList);
                        byteList.Clear();
                    }
                }
                catch (Exception e)
                {
                    IsReading = false;
                    Log.Error("Error while reading from port. Exception: " + e.Message);
                }

            }
        }

        private bool IsDeviceMessageCompleted(List<byte> byteList)
        {
            if (byteList.Count < 2)
            {
                return false;
            }
            var len = byteList.Count;
            var result = byteList[len - 2] == 0xcc && byteList[len - 1] == 0xdd;
            return result;
        }

        #endregion serial_read


        #region serial_write

        public async void WriteToSerialPortAsync(Message_5CSRTT message)
        {
            byte[] sendBytes = { 0xaa, 0xbb, message.aisle, message.category, message.address, message.value_1, message.value_2, message.value_3, 0xcc, 0xdd };

            WriteToSerialPort(sendBytes);
        }

        public void WriteToSerialPort(Message_5CSRTT message)
        {
            byte[] sendBytes = { 0xaa, 0xbb, message.aisle, message.category, message.address, message.value_1, message.value_2, message.value_3, 0xcc, 0xdd };
            WriteToSerialPort(sendBytes);
        }

        public void WriteToSerialPort(byte[] sendBytes)
        {
            if (!IsConnected)
            {
                Log.Warning("No connection started");
                return;
            }

            try
            {
                _port.Write(sendBytes, 0, sendBytes.Length);
            }
            catch (Exception e)
            {
                Log.Error("Erro writing to port: " + e.Message);
            }
        }
        #endregion serial_write


        #region send_messages

        public void Send_SetState_HoleCueLight(int index, bool isOn)
        {
            byte color = 0xff;
            Send_SetState_HoleCueLight_Color(index, isOn, color, color, color);
        }

        internal void Send_SetState_HoleCueLight_Color(int index, bool isOn, byte col1, byte col2, byte col3)
        {
            byte value_1 = (byte)(isOn ? col1 : 0x00);
            byte value_2 = (byte)(isOn ? col2 : 0x00);
            byte value_3 = (byte)(isOn ? col3 : 0x00);

            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__HOLE_CUE_LIGHTS,
                address = Convert.ToByte(index),
                value_1 = value_1,
                value_2 = value_2,
                value_3 = value_3,
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_HoleCueLight_Color: [index:{index}], [isOn:{isOn}], [col1:{col1}], [col2:{col2}], [col3:{col3}]");
        }

        public void Send_SetState_Shoker(bool isOn)
        {
            byte state = (byte)(isOn ? 0x01 : 0x00);
            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__SHOCKER,
                address = 0x00,
                value_1 = state
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_Shoker [isOn:{isOn}]");
        }

        public void Send_SetCurrent_in_mA(float current_in_mA)
        {
            int currentAsInt = (int)Math.Round(current_in_mA * 10, 0);
            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__CURRENT_MA,
                address = 0x00,
                value_1 = Convert.ToByte(currentAsInt),
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetCurrent_in_mA [current_in_mA:{current_in_mA}]");
        }


        public void Send_SetState_PanelCueLights(bool isOn)
        {
            byte color = 0xff;
            Send_SetState_PanelCueLights_Color(isOn, color, color, color);
        }

        public void Send_SetState_PanelCueLights_Color(bool isOn, byte col1, byte col2, byte col3)
        {
            byte value_1 = (isOn ? col1 : (byte)0x00);
            byte value_2 = (isOn ? col2 : (byte)0x00);
            byte value_3 = (isOn ? col3 : (byte)0x00);

            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__PANEL_CUE_LIGHT,
                address = 0x00,
                value_1 = value_1,
                value_2 = value_2,
                value_3 = value_3
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_PanelCueLights_Color [isOn:{isOn}], [col1:{col1}], [col2:{col2}], [col3:{col3}]");
        }

        public void Send_SetState_Pelets()
        {
            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__PELLETS,
                address = 0x00,
                value_1 = 0x01
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_Pelets");
        }

        public void Send_SetState_PelletReceptacleLight(bool isOn)
        {
            byte color = 0xff;
            Send_SetState_PelletReceptacleLight_Color(isOn, color, color, color);
        }

        public void Send_SetState_PelletReceptacleLight_Color(bool isOn, byte col1, byte col2, byte col3)
        {
            byte value_1 = (isOn ? col1 : (byte)0x00);
            byte value_2 = (isOn ? col2 : (byte)0x00);
            byte value_3 = (isOn ? col3 : (byte)0x00);

            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__PELLET_RECEPTACLE_LIGHT,
                address = 0x00,
                value_1 = value_1,
                value_2 = value_2,
                value_3 = value_3
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_PelletReceptacleLight_Color [isOn:{isOn}], [col1:{col1}], [col2:{col2}], [col3:{col3}]");
        }

        public void Send_SetState_HouseLightInCubicle(bool isOn)
        {
            byte color = 0xff;
            Send_SetState_HouseLightInCubicle_Color(isOn, color, color, color);
        }

        public void Send_SetState_HouseLightInCubicle_Color(bool isOn, byte col1, byte col2, byte col3)
        {
            byte value_1 = (isOn ? col1 : (byte)0x00);
            byte value_2 = (isOn ? col2 : (byte)0x00);
            byte value_3 = (isOn ? col3 : (byte)0x00);

            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__HOUSE_LIGHT_IN_CUBICLE,
                address = 0x00,
                value_1 = value_1,
                value_2 = value_2,
                value_3 = value_3,
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_HouseLightInCubile [isOn:{isOn}], [col1:{col1}], [col2:{col2}], [col3:{col3}]");
        }

        public void Send_SetState_IR_LightInCubicle(bool isOn)
        {
            byte state = (byte)(isOn ? 0x01 : 0x00);
            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__IR_LIGHT_IN_CUBICLE,
                address = 0x00,
                value_1 = state
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_IR_LightInCubicle [isOn:{isOn}]");
        }

        public void Send_SetState_FanInCubicle(bool isOn)
        {
            byte state = (byte)(isOn ? 0x01 : 0x00);
            var message = new Message_5CSRTT
            {
                aisle = 0x01,
                category = AppConstants.DEVICE_MESSAGE_CATEGORY__FAN_IN_CUBICLE,
                address = 0x00,
                value_1 = state
            };

            WriteToSerialPort(message);

            Log.Debug($"Send_SetState_FanInCubicle [isOn:{isOn}]");
        }

        #endregion send_messages

    }
}
