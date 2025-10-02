using UnityEngine;

namespace Spacats.Utils
{
    public class EmptyMonoBehClass : MonoBehaviour
    {
        public int Value;

        public void SomeMethod(int input)
        {
            Value = input;
        }
    }
}
