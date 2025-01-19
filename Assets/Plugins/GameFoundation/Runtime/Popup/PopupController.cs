using Cysharp.Threading.Tasks;
using UniRx;

namespace GameFoundation.Popup
{
    public class PopupOpenMsg<T>
        where T : System.Enum
    {
        public T popupType;
        public object data;
        public UniTaskCompletionSource<object> result;
    }

    public class PopupHideMsg<T>
        where T : System.Enum
    {
        public T popupType;
    }

    public class PopupController
    {
        public static void Show<T>(T popupType, object data = null)
            where T : System.Enum
        {
            MessageBroker.Default.Publish(
                new PopupOpenMsg<T> { popupType = popupType, data = data }
            );
        }

        public static UniTask<object> ShowAsync<T>(
            T popupType,
            object data = null,
            UniTaskCompletionSource<object> result = null
        )
            where T : System.Enum
        {
            result ??= new UniTaskCompletionSource<object>();
            MessageBroker.Default.Publish(
                new PopupOpenMsg<T>
                {
                    popupType = popupType,
                    data = data,
                    result = result
                }
            );
            return result.Task;
        }

        public static void Hide<T>(T popupType)
            where T : System.Enum
        {
            MessageBroker.Default.Publish(new PopupHideMsg<T> { popupType = popupType });
        }
    }
}
