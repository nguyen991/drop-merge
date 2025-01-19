using Databrain.Logic.Attributes;

namespace Databrain.Logic.StateMachine
{
    [HideNode]
    public abstract class StateMachineNode : NodeData
    {
        public override void ExecuteNode(){}
        public virtual void OnStateNodeInit(StateMachineController stateMachineController){}
        public virtual void OnEnter(){}
        public virtual void OnUpdate(){}
        public virtual void OnExit(){}   
    }
}