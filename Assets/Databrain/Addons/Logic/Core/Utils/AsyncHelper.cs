using System.Threading.Tasks;
using UnityEngine;

namespace Databrain.Logic.Utils
{
    public class AsyncHelper
    {
        public async Task WaitForSeconds(float _seconds, bool _unscaled = false)
        {
            if (!_unscaled)
            {
                float start = Time.time;
                while (Time.time < start + _seconds )
                {
                    var _currentFrame = Time.frameCount;
                    while (_currentFrame >= Time.frameCount)
                        await Task.Yield();
                }
            }
            else
            {
                float start = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup < start + _seconds )
                {
                    var _currentFrame = Time.frameCount;
                    while (_currentFrame >= Time.frameCount)
                        await Task.Yield();
                }   
            }

        }

        public async Task WaitForFrame()
        {
            var _currentFrame = Time.frameCount;
            while (_currentFrame >= Time.frameCount)
                await Task.Yield();
        }
        

        public AsyncHelper(){}
    }
}