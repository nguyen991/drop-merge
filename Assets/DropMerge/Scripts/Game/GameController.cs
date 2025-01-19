using System.Collections;
using System.Collections.Generic;
using GameFoundation.Audio;
using GameFoundation.FSM;
using UnityEngine;
using Zenject;

namespace DropMerge.Game
{
    public class GameController : MonoBehaviour
    {
        private StateMachine stateMachine;

        [Inject]
        private void Inject(StateMachine fsm, DiContainer container)
        {
            fsm.AddState(GameStates.Init, container.Instantiate<GSInit>());
            fsm.AddState(GameStates.Playing, container.Instantiate<GSPlaying>());
            fsm.AddState(GameStates.Lose, container.Instantiate<GSLose>());
            stateMachine = fsm;
        }

        private void Start()
        {
            AudioManager.Instance.PlayBackground();
            stateMachine.Start(GameStates.Init);
        }

        private void Update()
        {
            stateMachine.OnUpdate();
        }

        private void OnDestroy()
        {
            stateMachine.OnDestroy();
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (!focusStatus)
            {
                stateMachine.OnAction(GameStates.Action_SaveGame);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                stateMachine.OnAction(GameStates.Action_SaveGame);
            }
        }
    }
}
