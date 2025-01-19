using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameFoundation.Extensions;
using GameFoundation.FSM;
using GameFoundation.Mobile;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DropMerge.Popup
{
    public class PopupContinue : MonoBehaviour
    {
        public int countDownTime = 5;
        public TextMeshProUGUI countDownText;
        public Image countDownImage;
        public Button continueButton;
        public Button closeButton;

        [Header("Audio")]
        public AudioClip countDownSFX;

        public void OnPopupShow()
        {
            countDownText.text = countDownTime.ToString();
            countDownImage.fillAmount = 1;
            Tween
                .UIFillAmount(countDownImage, 1, 0, countDownTime, startDelay: 0.5f)
                .OnUpdate(
                    countDownImage,
                    (target, tween) =>
                    {
                        countDownText.text = Mathf
                            .CeilToInt(target.fillAmount * countDownTime)
                            .ToString();
                    }
                )
                .OnComplete(() => closeButton.onClick.Invoke());

            countDownText.transform.parent.localScale = Vector3.one;
            Tween.PunchScale(
                countDownText.transform.parent,
                Vector3.one * 0.15f,
                0.5f,
                startDelay: 0.5f,
                frequency: 1,
                cycles: countDownTime
            );

            countDownSFX.PlaySFX(1f, true);
        }

        public void OnPopupHide()
        {
            Tween.StopAll(countDownImage);
            Tween.StopAll(countDownText.transform.parent);
            countDownSFX.StopSFX();
        }

        [Inject]
        private void Inject(StateMachine stateMachine)
        {
            continueButton.onClick.AddListener(async () =>
            {
                PauseTween(true);

                // show ads
                var result = await AdController.Instance.ShowRewardAsync();
                if (!result)
                {
                    PauseTween(false);
                    return;
                }

                stateMachine.OnAction(GameStates.Action_Continue);
            });
            closeButton.onClick.AddListener(() =>
            {
                PauseTween(true);
                stateMachine.OnAction(GameStates.Action_EndGame);
            });
        }

        private void PauseTween(bool value)
        {
            Tween.SetPausedAll(value, countDownImage);
            Tween.SetPausedAll(value, countDownText.transform.parent);
            if (value)
            {
                countDownSFX.StopSFX();
            }
            else
            {
                countDownSFX.PlaySFX(1f, true);
            }
        }
    }
}
