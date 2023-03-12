﻿using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Protocols
{
    public class FixedIntervalTrainingProtocol : IProtocol_5CSRTT
    {
        public long reinforced_response_period { get; set; } = 30 * 1000;
        public long nonreinforced_response_period { get; set; } = 10 * 1000;
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


        public const string PROTOCOL_EVENT__WRONG_POKE_START = "WRONG_POKE_START";
        public const string PROTOCOL_EVENT__REINFORCED_POKE_START = "REINFORCED_POKE_START";
        public const string PROTOCOL_EVENT__NOT_REINFORCED_POKE_START = "NOT_REINFORCED_POKE_START";
        public const string PROTOCOL_EVENT__WRONG_POKE_END = "WRONG_POKE_END";
        public const string PROTOCOL_EVENT__REINFORCED_POKE_END = "REINFORCED_POKE_END";
        public const string PROTOCOL_EVENT__NOT_REINFORCED_POKE_END = "NOT_REINFORCED_POKE_END";


        public const string PROTOCOL_EVENT__CORRECT_POKE_START = "CORRECT_POKE_START";


        public int GetId() => AppConstants.PROTOCOL_TYPE__FIXED_INTERVAL;

        public string GetName() => "Fixed interval";

        public string GetDescription()
        {
            return @"1. Начало исследования

2. Включение источника света House Light на время [Non‑Reinforced Response Period] + [Reinforced Response Period] секунд. По окончанию периода свет выключается.

2.1. В течении периода времени [Non-Reinforced Response Period] регистрируются тычки с кодом события {NOT_REINFORCED_POKE_START}.

2.2. В течении подпериода времени [Reinforced Response Period] регистрируются тычки с кодом события {REINFORCED_POKE_START}

2.3. Если за время [Reinforced Response Period] произошел хотябы один тычок, то  выпадает пелета, выключается свет и начинается период ожидания.

3. Ожидание времени [ITI Period] секунд. Если [ITI Random Spread] не ноль, время ожидания это случайное число в промежутке [ITI Period] – [ITI Random Spread] , [ITI Period] + [ITI Random Spread]

3.1. Если во время ожидания происходит тычок регистрируется событие {WRONG_POKE_START}

4. Повторить исследование со второго пункта. Исследования повторяются [Number Of Repeats] раз";
        }

        public Dictionary<string, ParameterDefinitionModel> GetParametersDefentitions()
        {
            var definitionList = new List<ParameterDefinitionModel>();

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "reinforced_response_period",
                Name = "Reinforced Response Period",
                Units = "s",
                ValueType = AppConstants.PARAMETER_VALUE_TYPE__NUMBER,
                Default = "30"
            });

            definitionList.Add(new ParameterDefinitionModel
            {
                Code = "nonreinforced_response_period",
                Name = "Non-Reinforced Response Period",
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
                Default = "30"
            });

            var result = definitionList.ToDictionary(x => x.Code, x => x);

            return result;
        }

        public Dictionary<string, string> GetProtocolDefaultParameterValues()
        {
            var parameterDefinition = GetParametersDefentitions();
            var result = parameterDefinition.ToDictionary(x => x.Key, x => x.Value.Default);
            return result;
        }

        public Dictionary<string, string> GetProtocolParameterValues()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add("reinforced_response_period", DataRepository.MillisecondsToFloatString(reinforced_response_period));
            result.Add("nonreinforced_response_period", DataRepository.MillisecondsToFloatString(nonreinforced_response_period));
            result.Add("iti_period", DataRepository.MillisecondsToFloatString(iti_period));
            result.Add("iti_random_spread", DataRepository.MillisecondsToFloatString(iti_random_spread));
            result.Add("number_of_repeats", number_of_repeats.ToString());

            return result;
        }

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

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "reinforced_response_period", out parseFloatValue))
            {
                reinforced_response_period = (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("reinforced_response_period", "Failed to parse value");
            }

            if (DataRepository.TryGetFloatValueFromDictionary(parameters, "nonreinforced_response_period", out parseFloatValue))
            {
                nonreinforced_response_period = (int)Math.Round(parseFloatValue * 1000);
            }
            else
            {
                validationErrors.Add("nonreinforced_response_period", "Failed to parse value");
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


        private FixedIntervalPeriodMode _currentPeriodMode;
        private FixedIntervalPeriodMode _pokeStartMode;
        private bool _forcePeriodModeSwitch = false;
        private bool _dropPellets = false;

        public void StartProtocolThread(SerialPortService_5CSRTT service)
        {
            _deviceService = service;
            _deviceService.Message_5CSRTTEvent += Message_5CSRTTHandler;

            _random = new Random();

            _thread = new Thread(Run);
            _thread.Start();
        }

        public void Run()
        {
            Log.Information("Started protocol");

            IsRunning = true;
            _currentPeriodMode = FixedIntervalPeriodMode.NONE;
            _pokeStartMode = FixedIntervalPeriodMode.NONE;
            _forcePeriodModeSwitch = false;
            _dropPellets = false;

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
                        case FixedIntervalPeriodMode.NONE:
                            RegisterEvent?.Invoke(AppConstants.PROTOCOL_EVENT__TRIAL_START, $"{trialCounter}");
                            RegisterEvent?.Invoke("NON_REINFORCED_PERIOD_START", "");

                            nextPeriodEnd = clock.ElapsedMilliseconds + nonreinforced_response_period;

                            _deviceService.Send_SetState_PanelCueLights(true);

                            UpdateStatusEvent?.Invoke($"Running. Trial #{trialCounter} / in non-reinforced response / waiting [{nonreinforced_response_period} ms]");

                            _currentPeriodMode = FixedIntervalPeriodMode.NON_REINFORCED_RESPONSE;

                            _dropPellets = false;
                            break;
                        case FixedIntervalPeriodMode.NON_REINFORCED_RESPONSE:
                            RegisterEvent?.Invoke("REINFORCED_PERIOD_START", "");

                            nextPeriodEnd = clock.ElapsedMilliseconds + reinforced_response_period;

                            UpdateStatusEvent?.Invoke($"Running. Trial #{trialCounter} / in reinforced response / waiting [{reinforced_response_period} ms]");

                            _currentPeriodMode = FixedIntervalPeriodMode.REINFORCED_RESPONSE;
                            break;
                        case FixedIntervalPeriodMode.REINFORCED_RESPONSE:
                            long timeToWait = 0;
                            if (iti_random_spread > 0)
                            {
                                long minValue = (iti_period - iti_random_spread) / 100;
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

                            if (_dropPellets)
                            {
                                _deviceService.Send_SetState_Pelets();
                            }
                            _deviceService.Send_SetState_PanelCueLights(false);

                            UpdateStatusEvent?.Invoke($"Running. Trial #{trialCounter} / waiting ITI time [{timeToWait} ms]");

                            _currentPeriodMode = FixedIntervalPeriodMode.WAIT_MODE;
                            break;
                        case FixedIntervalPeriodMode.WAIT_MODE:
                            RegisterEvent?.Invoke(AppConstants.PROTOCOL_EVENT__TRIAL_END, $"{trialCounter}");
                            trialCounter++;

                            _currentPeriodMode = FixedIntervalPeriodMode.NONE;

                            if (trialCounter > number_of_repeats)
                            {
                                FinishProtocol(true);
                            }

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
                            case FixedIntervalPeriodMode.NON_REINFORCED_RESPONSE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__NOT_REINFORCED_POKE_START, "");
                                break;
                            case FixedIntervalPeriodMode.REINFORCED_RESPONSE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__REINFORCED_POKE_START, "");

                                _forcePeriodModeSwitch = true;
                                _dropPellets = true;
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
                            case FixedIntervalPeriodMode.NON_REINFORCED_RESPONSE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__NOT_REINFORCED_POKE_END, "");
                                break;
                            case FixedIntervalPeriodMode.REINFORCED_RESPONSE:
                                RegisterEvent?.Invoke(PROTOCOL_EVENT__REINFORCED_POKE_END, "");
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

        public void FinishProtocol(bool isOk)
        {
            if (IsRunning)
            {
                RegisterEvent?.Invoke("STOP_PROTOCOL", isOk ? "0" : "1");
            }

            IsRunning = false;

            // send events to app

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

        public void StopProtocolThread()
        {
            FinishProtocol(false);
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
    }

    enum FixedIntervalPeriodMode
    {
        NONE,
        NON_REINFORCED_RESPONSE,
        REINFORCED_RESPONSE,
        WAIT_MODE
    }
}
