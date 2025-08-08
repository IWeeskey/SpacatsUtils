using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Spacats.Utils
{
    public class ExampleController : Controller
    {
        [Tooltip("Updated during update")]
        public int RandomValue = 0;
        protected override void ControllerOnEnable()
        {
            base.ControllerOnEnable();
        }

        protected override void ControllerOnDisable()
        {
            base.ControllerOnDisable();
        }

        public override void ControllerSharedUpdate()
        {
            base.ControllerSharedUpdate();
            RandomValue = Random.Range(0,100);
        }
    }
}
