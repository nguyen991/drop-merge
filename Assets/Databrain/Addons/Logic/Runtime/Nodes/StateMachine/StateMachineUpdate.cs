using System.Collections;
using System.Collections.Generic;
using Databrain.Logic.StateMachine;
using UnityEngine;

namespace Databrain.Logic.StateMachine
{
    public class StateMachineUpdate : MonoBehaviour
    {
        public static StateMachineUpdate Instance{ get; private set; }
        
        private List<StateMachineController> stateMachineControllers = new List<StateMachineController>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }   
        }

        public void RegisterToUpdate(StateMachineController _controller)
        {
            stateMachineControllers.Add(_controller);
        }

        public void UnregisterFromUpdate()
        {
            
        }

        void Update()
        {
            for (int i = stateMachineControllers.Count-1; i >= 0; i --)
            {
                // stateMachineControllers[i].OnUpdate
            }
        }
    }
}