using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Spacats.Utils
{
    public class ExampleController2 : Controller
    {
        [Tooltip("Updated during update")]
        public int RandomValue = 0;
        protected override void COnEnable()
        {
            base.COnEnable();
        }

        protected override void COnDisable()
        {
            base.COnDisable();
        }

        public override void CSharedUpdate(bool isGuiCall = false)
        {
            base.CSharedUpdate();
            RandomValue = Random.Range(0,100);
        }
    }
}
