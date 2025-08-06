using UnityEngine;

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    public class ExampleSingleton : Singleton<ExampleSingleton>
    {
        public int TestValue = 0;

        protected override void SingletonAwake()
        {
            base.SingletonAwake();
        }

        protected override void SingletonOnEnable()
        {
            base.SingletonOnEnable();
        }

        protected override void SingletonOnDisable()
        {
            base.SingletonOnDisable();
        }

        protected override void SingletonOnDestroy()
        {
            base.SingletonOnDestroy();
        }

        protected override void SingletonSharedUpdate()
        {
            base.SingletonSharedUpdate();
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
