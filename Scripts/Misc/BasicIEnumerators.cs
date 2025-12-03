using System.Collections;
using UnityEngine;

namespace Spacats.Utils
{
    public static class BasicIEnumerators
    {
        public static IEnumerator WaitNextFrame(System.Action onNextFrame, int framesToSkip = 1)
        {
            for (int i = 0; i < framesToSkip; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            onNextFrame?.Invoke();
        }
    }
}
