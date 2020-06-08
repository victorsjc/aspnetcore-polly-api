using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Stateless;
using Web.Api.Core.Shared;
using System.Runtime.Serialization;

namespace Web.Api.Core.Domain.Entities
{
	public class FID : BaseEntity
	{
        public enum FIDTriggers
        {
        	Publish,
        	Cancel,
        	Request_Changes,
            Suspend,
            Terminate,
            Approve,
            Approve_Restriction,
            Reject
        }
        public enum EvaluateState
        {
        	[EnumMember(Value = "Rascunho")] 
            Draft,
            [EnumMember(Value = "Finalizada")] 
            Deleted,
            [EnumMember(Value = "Aguardando Revisao")] 
            Awaiting_Review,
            [EnumMember(Value = "Aguardando Ajuste")] 
            Awaiting_Changes,
            [EnumMember(Value = "Revisado")]
            Reviewed,
            [EnumMember(Value = "Aguardando Avaliação")]
            Awaiting_Evaluation,
            [EnumMember(Value = "Avaliado")]
            Evaluated,
            [EnumMember(Value = "ATA-Aprovada")]
            Approved,
            [EnumMember(Value = "ATA-Reprovada")]
            Rejected,
            [EnumMember(Value = "ATA-Pendente")]
            Waiting_For_Approval,
            [EnumMember(Value = "Cancelada")]
            Canceled
        }
        public EvaluateState Status => _stateMachine.State;
        private readonly StateMachine<EvaluateState, FIDTriggers> _stateMachine;
        public string Name { get; }

        public FID(string name)
        {
        	_stateMachine = new StateMachine<EvaluateState, FIDTriggers>(EvaluateState.Draft);
            Name = name;

            ConfigureStateMachine();
        }

        [JsonConstructor]
        public FID(string state, string name)
        {
            var fidState = (EvaluateState) Enum.Parse(typeof(EvaluateState), state);
            _stateMachine = new StateMachine<EvaluateState, FIDTriggers>(fidState);
            Name = name;

            ConfigureStateMachine();
        }

        private void ConfigureStateMachine()
        {
            _stateMachine.Configure(EvaluateState.Draft)
                .Permit(FIDTriggers.Terminate, EvaluateState.Deleted)
                .Permit(FIDTriggers.Publish, EvaluateState.Awaiting_Review);

            _stateMachine.Configure(EvaluateState.Awaiting_Review)
                .Permit(FIDTriggers.Cancel, EvaluateState.Canceled)
                .Permit(FIDTriggers.Request_Changes, EvaluateState.Awaiting_Changes)
                .Permit(FIDTriggers.Approve, EvaluateState.Reviewed);

            _stateMachine.Configure(EvaluateState.Awaiting_Changes)
                .Permit(FIDTriggers.Publish, EvaluateState.Awaiting_Review);

            _stateMachine.Configure(EvaluateState.Reviewed)
                .Permit(FIDTriggers.Approve, EvaluateState.Awaiting_Evaluation)
                .Permit(FIDTriggers.Cancel, EvaluateState.Canceled);

            _stateMachine.Configure(EvaluateState.Awaiting_Evaluation)
                .Permit(FIDTriggers.Approve, EvaluateState.Approved)
                .Permit(FIDTriggers.Approve_Restriction, EvaluateState.Waiting_For_Approval)
                .Permit(FIDTriggers.Reject, EvaluateState.Rejected)
                .Permit(FIDTriggers.Cancel, EvaluateState.Canceled);
        }

        public void Terminate(){
        	_stateMachine.Fire(FIDTriggers.Terminate);
        }

        public void Publish()
        {
        	_stateMachine.Fire(FIDTriggers.Publish);
        }

        public void Approve()
        {
        	_stateMachine.Fire(FIDTriggers.Approve);
        }

        public void Cancel()
        {
        	_stateMachine.Fire(FIDTriggers.Cancel);
        }

        public void Reject()
        {
        	_stateMachine.Fire(FIDTriggers.Reject);
        }

        public void RequestChanges()
        {
        	_stateMachine.Fire(FIDTriggers.Request_Changes);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static FID FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<FID>(jsonString);
        }
	}
}