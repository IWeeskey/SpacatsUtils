using UnityEngine;

namespace Spacats.Utils
{
    [ExecuteInEditMode]
    public class ExampleSingleton : Singleton<ExampleSingleton>
    {
        public int TestValue = 0;

        protected override void SAwake()
        {
            base.SAwake();
        }

        protected override void SOnEnable()
        {
            base.SOnEnable();
        }

        protected override void SOnDisable()
        {
            base.SOnDisable();
        }

        protected override void SOnDestroy()
        {
            base.SOnDestroy();
        }

        protected override void SSharedUpdate(bool isGuiCall = false)
        {
            base.SSharedUpdate();
        }

        protected override void SOnApplicationQuit()
        {
            base.SOnApplicationQuit();
        }
    }
}
