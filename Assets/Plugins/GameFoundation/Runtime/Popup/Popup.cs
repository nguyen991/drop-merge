using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameFoundation.Popup
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class Popup<T> : MonoBehaviour
        where T : System.Enum
    {
        public T popupType;

        [Header("Animation")]
        public Animator animator;
        public string openTrigger = "open";
        public string closeTrigger = "close";

        private Canvas canvas;
        private GraphicRaycaster raycaster;
        private PopupOpenMsg<T> popupData;
        private UniTaskCompletionSource onClosedTask = null;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            raycaster = GetComponent<GraphicRaycaster>();
            animator = animator ?? GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            // disable the popup by default
            canvas.enabled = false;
            raycaster.enabled = false;

            // subscribe to the popup events
            MessageBroker
                .Default.Receive<PopupOpenMsg<T>>()
                .Subscribe(msg =>
                {
                    if (msg.popupType.Equals(popupType))
                    {
                        Show(msg);
                    }
                })
                .AddTo(this);
            MessageBroker
                .Default.Receive<PopupHideMsg<T>>()
                .Subscribe(msg =>
                {
                    if (msg.popupType.Equals(popupType))
                    {
                        Hide();
                    }
                })
                .AddTo(this);
        }

        public void Show()
        {
            Show(new PopupOpenMsg<T> { popupType = popupType });
        }

        public void Show(PopupOpenMsg<T> msg)
        {
            popupData = msg;

            // broadcast the event
            gameObject.SendMessage("OnPopupShow", msg, SendMessageOptions.DontRequireReceiver);

            // show the popup if it's not already shown
            if (!canvas.enabled)
            {
                transform.SetAsLastSibling();
                canvas.enabled = true;
                raycaster.enabled = true;
                if (animator != null)
                {
                    animator.SetTrigger(openTrigger);
                }
            }
        }

        public async void Hide()
        {
            // hide the popup if it's not already hidden
            if (!canvas.enabled)
                return;

            // disable raycaster
            raycaster.enabled = false;

            // play the hide animation
            if (animator != null)
            {
                onClosedTask = new UniTaskCompletionSource();
                animator.SetTrigger(closeTrigger);
                await onClosedTask.Task;
                onClosedTask = null;
            }

            // disactivate canvas
            canvas.enabled = false;

            // broadcast the event
            gameObject.SendMessage(
                "OnPopupHide",
                popupData,
                SendMessageOptions.DontRequireReceiver
            );

            // check if the popup has set a result
            await UniTask.NextFrame();
            if (
                popupData != null
                && popupData.result != null
                && popupData.result.Task.Status == UniTaskStatus.Pending
            )
            {
                popupData.result.TrySetResult(null);
            }
        }

        public void OnClosed()
        {
            onClosedTask?.TrySetResult();
        }

#if UNITY_EDITOR
        // [Title("Debug"), Button(Name = "Show")]
        // public void DebugShow()
        // {
        //     Show(new PopupOpenMsg() { popupType = popupType });
        // }

        // [Button(Name = "Hide")]
        // public void DebugHide()
        // {
        //     Hide().Forget();
        // }
#endif
    }
}
