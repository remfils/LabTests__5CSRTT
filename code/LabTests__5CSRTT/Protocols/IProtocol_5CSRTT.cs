using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Protocols
{
    public interface IProtocol_5CSRTT
    {
        bool IsRunning { get; }

        event RegisterEventDelegate RegisterEvent;

        event ProtocolFinishedCallbackDelegate ProtocolFinishedEvent;

        event UpdateStatusDelegate UpdateStatusEvent;

        int GetId();

        string GetName();

        string GetDescription();

        void SetParametersFromDictionary(Dictionary<string, string> parameterDictionary, Dictionary<string, string> validationErrors);

        Dictionary<string, string> GetProtocolParameterValues();

        Dictionary<string, string> GetProtocolDefaultParameterValues();

        Dictionary<string, ParameterDefinitionModel> GetParametersDefentitions();

        void ClearEventHandlers();

        void StartProtocolThread(SerialPortService_5CSRTT service);

        void StopProtocolThread();
    }
}
