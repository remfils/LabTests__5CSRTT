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
    public class HabitationMagazineTrainingProtocol__house_light : IProtocol_5CSRTT
    {
        public long iti_period { get; set; } = 15 * 1000;
        public long iti_random_spread { get; set; } = 0 * 1000;
        public int number_of_repeats { get; set; } = 20;
        public int house_light_period { get; set; } = 30 * 1000;

        public event RegisterEventDelegate RegisterEvent;
        public event ProtocolFinishedCallbackDelegate ProtocolFinishedEvent;
        public event UpdateStatusDelegate UpdateStatusEvent;

        public bool IsRunning { get; set; } = false;
        private SerialPortService_5CSRTT _deviceService;
        private Thread _thread;
        private Random _random;

        public int GetId() => AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__HOUSE_LIGHT;

        public string GetName() => "Habituation. Magazine Training";


        public const string PROTOCOL_EVENT__CORRECT_POKE_START = "CORRECT_POKE_START";
        public const string PROTOCOL_EVENT__CORRECT_POKE_END= "CORRECT_POKE_END";
        public const string PROTOCOL_EVENT__WRONG_POKE_START = "WRONG_POKE_START";
        public const string PROTOCOL_EVENT__WRONG_POKE_END = "WRONG_POKE_END";


        public void SetParametersFromDictionary(Dictionary<string, string> parameters, Dictionary<string, string> validationErrors)
        {
            int parseIntValue = 0;
            float parseFloatValue = 0;

            if (DataRepository.TryGetIntValueFromDictionary(parameters, "number_of_repeats", out parseIntValue))
            {
                number_of_repeats = parseIntValue;
            }
            else
            {
                validationErrors.Add("number_of_repeats", "Failed to parse value");
            }

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "house_light_period", out parseFloatValue))
            {
                house_light_period = (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("house_light_period", "Failed to parse value");
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

            result.Add("house_light_period", DataRepository.MillisecondsToFloatString(house_light_period));
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
                Code = "house_light_period",
                Name = "House Light Period",
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
            return @"1. Начало исследования.

2. Включение источника света [House Light Number] на время [House Light Period] секунд.
    По окончанию периода [House Light Period] свет выключается.

2.1. Регистрируется событие {TRIAL_START}.

3. Ожидается тычок носом

3.1. Если произошел тычок носом пока горит источник света, то

    * выключить источник света

    * выпадает пеллета

    * регистрируется событие {CORRECT_POKE_START}

3.2. Если тычок носом не произошел, по окончанию времени [House Light Duration]

    * выключить источник света

    * выпадает пеллета

4. Ожидание времени [ITI Period] секунд. Если [ITI Random Spread] не ноль, время ожидания это случайное
    число в промежутке

        от [ITI Period] – [ITI Random Spread]
        до [ITI Period] + [ITI Random Spread]

    если [ITI Period] – [ITI Random Spread] меньше [House Light Duration] то случайное число берется из
    промежутка

        от [Cue Light Duration]
        до [ITI Period] + [ITI Random Spread]

4.1. Регистрируется событие {ITI_WAIT}

4.2. Если за время ожидание происходит тычок носом - регистрируется событие {WRONG_POKE_START}

5. Регистрируется событие {TRIAL_END}. Повторить исследование со второго пункта. Исследования повторяются [Number Of Repeats] раз";
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

        private HabitationMagazinePeriodMode _currentPeriodMode;
        private HabitationMagazinePeriodMode _pokeStartMode;
        private bool _forcePeriodModeSwitch = false;

        public void StartProtocolThread(SerialPortService_5CSRTT deviceService)
        {
            _deviceService = deviceService;
            _deviceService.Message_5CSRTTEvent += Message_5CSRTTHandler;

            _random = new Random();

            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Run()
        {
            Log.Information("Started protocol");

            IsRunning = true;
            _currentPeriodMode = HabitationMagazinePeriodMode.NONE;
            _forcePeriodModeSwitch = false;

            long nextPeriodEnd = 0;
            int trialCounter = 1;

            var clock = Stopwatch.StartNew();
            RegisterEvent?.Invoke("START_PROTOCOL", "");

            while (IsRunning)
            {
                if (clock.ElapsedMilliseconds >= nextPeriodEnd || _forcePeriodModeSwitch)
                {
                    _forcePeriodModeSwitch = false;

                    switch (_currentPeriodMode)
                    {
                        case HabitationMagazinePeriodMode.NONE:
                            nextPeriodEnd = clock.ElapsedMilliseconds + house_light_period;

                            RegisterEvent?.Invoke(AppConstants.PROTOCOL_EVENT__TRIAL_START, $"{trialCounter}");
                            RegisterEvent?.Invoke("HOUSE_LIGHT_ON", "");

                            _deviceService.Send_SetState_PanelCueLights(true);

                            UpdateStatusEvent?.Invoke($"Running... Trial #{trialCounter} > waiting mouse input [{house_light_period} ms]");

                            _currentPeriodMode = HabitationMagazinePeriodMode.HOUSE_LIGHT_ON_MODE;
                            break;
                        case HabitationMagazinePeriodMode.HOUSE_LIGHT_ON_MODE:
                            RegisterEvent?.Invoke("HOUSE_LIGHT_OFF", $"");

                            long timeToWait = 0;
                            if (iti_random_spread > 0)
                            {
                                long minValue = (iti_period - iti_random_spread) / 100;
                                if (minValue < house_light_period)
                                {
                                    minValue = house_light_period;
                                }
                                long maxValue = (iti_period + iti_random_spread) / 100;

                                timeToWait = _random.NextInt64(minValue, maxValue) * 100;
                                RegisterEvent?.Invoke("ITI_WAIT", $"{timeToWait};{iti_random_spread}");
                            }
                            else
                            {
                                timeToWait = iti_period;
                                RegisterEvent?.Invoke("ITI_WAIT", $"{iti_period};0");
                            }

                            nextPeriodEnd = clock.ElapsedMilliseconds + timeToWait;

                            _deviceService.Send_SetState_PanelCueLights(false);
                            _deviceService.Send_SetState_Pelets();

                            UpdateStatusEvent?.Invoke($"Running... Trial #{trialCounter} > waiting ITI time {timeToWait} ms");

                            _currentPeriodMode = HabitationMagazinePeriodMode.WAIT_MODE;

                            break;
                        case HabitationMagazinePeriodMode.WAIT_MODE:
                            RegisterEvent?.Invoke(AppConstants.PROTOCOL_EVENT__TRIAL_END, $"{trialCounter}");

                            trialCounter++;

                            if (trialCounter > number_of_repeats)
                            {
                                FinishProtocol(true);
                            }

                            _currentPeriodMode = HabitationMagazinePeriodMode.NONE;
                            break;
                    }
                }
            }

            Log.Information("Protocol end");

            ProtocolFinishedEvent?.Invoke();
        }

        private void Message_5CSRTTHandler(Message_5CSRTT message)
        {
            switch (message.category)
            {
                case AppConstants.DEVICE_MESSAGE_CATEGORY__NOSE_POKE_ON_PELLET:

                    if (message.value_1 == 0x01)
                        {
                        _pokeStartMode = _currentPeriodMode;

                        switch (_currentPeriodMode)
                        {
                            case HabitationMagazinePeriodMode.HOUSE_LIGHT_ON_MODE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__CORRECT_POKE_START, "");
                                _forcePeriodModeSwitch = true;
                                break;
                            default:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__WRONG_POKE_START, "");
                                break;
                        }
                    }
                    else if (message.value_1 == 0x02)
                    {
                        switch (_pokeStartMode)
                        {
                            case HabitationMagazinePeriodMode.HOUSE_LIGHT_ON_MODE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__CORRECT_POKE_END, "");
                                break;
                            default:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__WRONG_POKE_END, "");
                                break;
                        }
                    }

                    break;
                default:
                    Log.Debug($"Message of unknown category: {message.category:X2}");
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

            if (_deviceService != null)
            {
                // clear events on run
                _deviceService.Message_5CSRTTEvent -= Message_5CSRTTHandler;

                // reset device state
                _deviceService.Send_SetState_PanelCueLights(false);
            }
        }
    }

    enum HabitationMagazinePeriodMode
    {
        NONE,
        HOUSE_LIGHT_ON_MODE,
        WAIT_MODE
    }
}
