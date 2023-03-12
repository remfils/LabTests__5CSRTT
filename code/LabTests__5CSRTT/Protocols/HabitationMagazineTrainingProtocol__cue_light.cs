using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LabTests__5CSRTT.Protocols.IProtocol_5CSRTT;

namespace LabTests__5CSRTT.Protocols
{
    public class HabitationMagazineTrainingProtocol__cue_light : IProtocol_5CSRTT
    {

        public int cue_light_number { get; set; } = 3;
        public long cue_light_period { get; set; } = 30 * 1000;
        public long iti_period { get; set; } = 15 * 1000;
        public long iti_random_spread { get; set; } = 0 * 1000;
        public int number_of_repeats { get; set; } = 20;

        public event RegisterEventDelegate RegisterEvent;
        public event ProtocolFinishedCallbackDelegate ProtocolFinishedEvent;
        public event UpdateStatusDelegate UpdateStatusEvent;

        public bool IsRunning { get; set; } = false;
        private SerialPortService_5CSRTT _deviceService;
        private Thread _thread;
        private Random _random;

        public int GetId() => AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__CUE_LIGHT;

        public string GetName() => "Habituation. Magazine Training (cue light)";

        public void SetParametersFromDictionary(Dictionary<string, string> parameters, Dictionary<string, string> validationErrors)
        {
            int parseIntValue = 0;
            float parseFloatValue = 0;

            if (DataRepository.TryGetIntValueFromDictionary(parameters, "cue_light_number", out parseIntValue))
            {
                cue_light_number = parseIntValue;
            }
            else
            {
                validationErrors.Add("cue_light_number", "Failed to parse value");
            }

            if (DataRepository.TryGetIntValueFromDictionary(parameters, "number_of_repeats", out parseIntValue))
            {
                number_of_repeats = parseIntValue;
            }
            else
            {
                validationErrors.Add("number_of_repeats", "Failed to parse value");
            }

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "cue_light_period", out parseFloatValue))
            {
                cue_light_period= (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("cue_light_period", "Failed to parse value");
            }

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "iti_period", out parseFloatValue))
            {
                iti_period = (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("iti_period", "Failed to parse value");
            }

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "iti_random_spread", out parseFloatValue))
            {
                iti_random_spread = (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("iti_random_spread", "Failed to parse value");
            }
        }

        public Dictionary<string, string> GetProtocolParameterValues()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add("cue_light_number", cue_light_number.ToString());
            result.Add("cue_light_period", DataRepository.MillisecondsToFloatString(cue_light_period));
            result.Add("iti_period", DataRepository.MillisecondsToFloatString(iti_period));
            result.Add("iti_random_spread", DataRepository.MillisecondsToFloatString(iti_random_spread));
            result.Add("number_of_repeats", number_of_repeats.ToString());

            return result;
        }

        public Dictionary<string, string> GetProtocolDefaultParameterValues()
        {
            var parameterDefinition = GetParametersDefentitions();

            var result = parameterDefinition.ToDictionary(x => x.Key, x => x.Value.Default);

            return result;
        }

        public Dictionary<string, ParameterDefinitionModel> GetParametersDefentitions()
        {
            var definitionList = new List<ParameterDefinitionModel>();

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "cue_light_number",
                Name = "Cue Light Number",
                Units = "",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "3"
            });

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "cue_light_period",
                Name = "Cue Light Period",
                Units = "s",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "30"
            });

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "iti_period",
                Name = "ITI Period",
                Units = "s",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "15"
            });

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "iti_random_spread",
                Name = "ITI Random Spread",
                Units = "s",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "0"
            });

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "number_of_repeats",
                Name = "Number Of Repeats",
                Units = "",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "20"
            });

            var result = definitionList.ToDictionary(x => x.Code, x => x);

            return result;
        }

        public string GetDescription()
        {
            return @"1. Начало исследования

2. Включение источника света под номером [Cue Light Number] на время [Cue Light Period] секунд.
    По окончанию периода [Cue Light Period] свет выключается.

3. Ожидается тычок носом

3.1. Если произошел тычок носом пока горит источник света, то

    * выключить источник света под номером [Cue Light Number]

    * выпадает пеллета

3.2. Если тычок носом не произошел

    * выключить источник света под номером [Cue Light Number]

    * выпадает пеллета

4. Ожидание времени [ITI Period] секунд. Если [ITI Random Spread] не ноль, время ожидания это случайное
    число в промежутке

        от [ITI Period] – [ITI Random Spread]
        до [ITI Period] + [ITI Random Spread]

    если [ITI Period] – [ITI Random Spread] меньше [Cue Light Period] то случайное число берется из
    промежутка

        от [Cue Light Period]
        до [ITI Period] + [ITI Random Spread]

5. Повторить исследование со второго пункта. Исследования повторяются [Number Of Repeats] раз";
        }

        public void ClearEventHandlers()
        {
            if (RegisterEvent != null)
            {
                foreach (Delegate del in RegisterEvent.GetInvocationList())
                {
                    RegisterEvent -= (RegisterEventDelegate)del;
                }
            }

            if (ProtocolFinishedEvent != null)
            {
                foreach (Delegate del in ProtocolFinishedEvent.GetInvocationList())
                {
                    ProtocolFinishedEvent -= (ProtocolFinishedCallbackDelegate)del;
                }

            }
            if (UpdateStatusEvent != null)
            {
                foreach (Delegate del in UpdateStatusEvent.GetInvocationList())
                {
                    UpdateStatusEvent -= (UpdateStatusDelegate)del;
                }
            }
        }

        private bool _isLightOn = false;
        private bool _isInWaitMode = false;
        private bool _isCorrectNosePoke = false;

        public void StartProtocolThread(SerialPortService_5CSRTT deviceService)
        {
            IsRunning = true;

            _isLightOn = false;
            _isInWaitMode = false;
            _isCorrectNosePoke = false;

            _deviceService = deviceService;
            _deviceService.Message_5CSRTTEvent += Message_5CSRTTHandler;

            _random = new Random();

            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Run()
        {
            var clock = Stopwatch.StartNew();
            long timeOnTurnOffLight = 0;
            long timeOnWaitEnd = 0;
            int trialCounter = 1;

            RegisterEvent?.Invoke("START_PROTOCOL", "");
            Log.Debug("Started protocol");

            while (IsRunning)
            {
                if (!_isInWaitMode)
                {
                    if (!_isLightOn)
                    {
                        _deviceService.Send_SetState_HoleCueLight(cue_light_number, true);

                        _isLightOn = true;
                        RegisterEvent?.Invoke("START_TRIAL", $"{trialCounter}");
                        RegisterEvent?.Invoke("CUE_LIGHT_ON", $"{cue_light_number}");

                        UpdateStatusEvent?.Invoke($"Running... Trial #{trialCounter} > waiting mouse input {cue_light_period} ms");

                        timeOnTurnOffLight = clock.ElapsedMilliseconds + cue_light_period;
                    }
                    else
                    {
                        if (_isCorrectNosePoke || clock.ElapsedMilliseconds >= timeOnTurnOffLight)
                        {
                            _deviceService.Send_SetState_HoleCueLight(cue_light_number, false);
                            _deviceService.Send_SetState_Pelets();

                            RegisterEvent?.Invoke("CUE_LIGHT_OFF", $"{cue_light_number}");

                            _isCorrectNosePoke = false;
                            _isLightOn = false;
                            _isInWaitMode = true;

                            long timeToWait = 0;
                            if (iti_random_spread > 0)
                            {
                                long minValue = (iti_period - iti_random_spread) / 100;
                                if (minValue < cue_light_period)
                                {
                                    minValue = cue_light_period;
                                }
                                long maxValue = (iti_period + iti_random_spread) / 100;

                                timeToWait = _random.NextInt64(minValue, maxValue / 100) * 100;
                                RegisterEvent?.Invoke("BEGIN_ITI_WAIT", $"{timeToWait};{iti_random_spread}");
                            }
                            else
                            {
                                timeToWait = iti_period;
                                RegisterEvent?.Invoke("BEGIN_ITI_WAIT", $"{iti_period};0");
                            }

                            timeOnWaitEnd = clock.ElapsedMilliseconds + timeToWait;

                            UpdateStatusEvent?.Invoke($"Running... Trial #{trialCounter} > waiting ITI time {timeToWait} ms");
                        }
                    }
                }
                else
                {
                    if (clock.ElapsedMilliseconds >= timeOnWaitEnd)
                    {
                        _isInWaitMode = false;

                        RegisterEvent?.Invoke("END_TRIAL", $"{trialCounter}");

                        trialCounter++;

                        if (trialCounter > number_of_repeats)
                        {
                            FinishProtocol(true);
                        }
                    }
                }
            }

            Log.Debug("Protocol end");

            ProtocolFinishedEvent?.Invoke();
        }

        private void Message_5CSRTTHandler(Message_5CSRTT message)
        {
            switch (message.category)
            {
                case AppConstants.DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_HOLE:
                    if (message.value_1 == 0x01) // nose poke start
                    {
                        if (message.address == cue_light_number)
                        {
                            if (_isLightOn)
                            {
                                RegisterEvent?.Invoke("CORRECT_POKE_START", $"{cue_light_number}");
                                Log.Information($"CORRECT HOLE: Start NOSE POKE in hole #{message.address:X2}");
                                _isCorrectNosePoke = true;
                            }
                            else
                            {
                                RegisterEvent?.Invoke("WRONG_POKE_START", $"{cue_light_number}");
                                Log.Information($"WRONG HOLE: Start NOSE POKE in hole #{message.address:X2}");
                            }
                        }
                        else
                        {
                            RegisterEvent?.Invoke("WRONG_POKE_START", $"{cue_light_number}");
                            Log.Information($"WRONG HOLE: Start NOSE POKE in hole #{message.address:X2}");
                        }
                    }
                    else if (message.value_1 == 0x00) // nose poke end
                    {
                        if (message.address == cue_light_number)
                        {
                            if (_isLightOn)
                            {
                                RegisterEvent?.Invoke("CORRECT_POKE_END", $"{cue_light_number}");
                                Log.Information($"CORRECT HOLE: END NOSE POKE in hole #{message.address:X2}");
                            }
                            else
                            {
                                RegisterEvent?.Invoke("WRONG_POKE_END", $"{cue_light_number}");
                                Log.Information($"WRONG HOLE: END NOSE POKE in hole #{message.address:X2}");
                            }
                        }
                        else
                        {
                            RegisterEvent?.Invoke("WRONG_POKE_END", $"{cue_light_number}");
                            Log.Information($"WRONG HOLE: END NOSE POKE in hole #{message.address:X2}");
                        }
                    }
                    break;
                case AppConstants.DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_PELLET:
                    if (message.value_1 == 0x02)
                    {
                        RegisterEvent?.Invoke("PELLET_CONSUMED", "");
                        Log.Information($"NOSE POKE PELLET status [consumed]");
                    }
                    else if (message.value_1 == 0x01)
                    {
                        RegisterEvent?.Invoke("PELLET_DROPED", "");
                        Log.Information($"NOSE POKE PELLET status [droped]");
                    }
                    break;
                default:
                    Log.Warning($"Message of unknown category: {message.category:X2}");
                    break;
            }
        }

        public void StopProtocolThread()
        {
            FinishProtocol(false);
        }

        public void FinishProtocol(bool isOk)
        {
            if (IsRunning)
            {
                RegisterEvent?.Invoke("STOP_PROTOCOL", isOk ? "0" : "1");
            }

            IsRunning = false;

            if (isOk)
            {
                UpdateStatusEvent?.Invoke($"Protocol finished");
            }
            else
            {
                UpdateStatusEvent?.Invoke($"Protocol terminated");
            }

            // clear events on run
            _deviceService.Message_5CSRTTEvent -= Message_5CSRTTHandler;

            // reset device state
            _deviceService.Send_SetState_HoleCueLight(cue_light_number, false);
        }
    }
}
