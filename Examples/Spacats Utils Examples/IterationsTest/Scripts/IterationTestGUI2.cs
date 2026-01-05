using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class IterationTestGUI2 : GUIButtons
    {
        [SerializeField] private IterationTestGUI _iteratioTest1Link;
        
        
        protected override string GetButtonLabel(int index)
        {
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0:  return "T Position";
                case 1:  return "T Change POS";
                case 2:  return "T GET POS ROT \nSCALE";
            }
        }
        
        protected override void OnButtonClick(int index)
        {
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: IterateTransformPosition(); break;
                case 1: IterateTransformModify(); break;
                case 2: IterateTransformALL(); break;
            }
        }

        private void IterateTransformPosition()
        {
            int count = _iteratioTest1Link.ListMono.Count;
            TimeTracker.Start("IterateTransformPosition " + count);
            
            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                counter += i & 1;
                Vector3 vec = _iteratioTest1Link.ListMono[i].transform.position;
            }
            
            TimeTracker.Finish("IterateTransformPosition " + count);
        }
        
        // private void IterateTransform()
        // {
        //     int count = _iteratioTest1Link.ListMono.Count;
        //     TimeTracker.Start("IterateTransform " + count);
        //     
        //     int counter = 0;
        //     for (int i = 0; i < count; i++)
        //     {
        //         counter += i & 1;
        //         Transform transform = _iteratioTest1Link.ListMono[i].transform;
        //         Vector3 vec = transform.position;
        //     }
        //     
        //     TimeTracker.Finish("IterateTransform " + count);
        // }
        private void IterateTransformModify()
        {
            int count = _iteratioTest1Link.ListMono.Count;
            TimeTracker.Start("IterateTransformModify " + count);
            
            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                counter += i & 1;
                Transform transform = _iteratioTest1Link.ListMono[i].transform;
                Vector3 vec = transform.position;
                vec *= 2f;
                transform.position = vec;
            }
            
            TimeTracker.Finish("IterateTransformModify " + count);
        }
        
        private void IterateTransformALL()
        {
            int count = _iteratioTest1Link.ListMono.Count;
            TimeTracker.Start("IterateTransformALL " + count);
            
            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                counter += i & 1;
                Transform transform = _iteratioTest1Link.ListMono[i].transform;
                Vector3 vec = transform.position;
                Vector3 vec2 = transform.localPosition;
                Vector3 scale = transform.lossyScale;
                Vector3 scale2 = transform.localScale;
                Quaternion quat = transform.rotation;
                Quaternion quat2 = transform.localRotation;
                Vector3 euler = transform.eulerAngles;
                Vector3 euler2 = transform.localEulerAngles;
            }
            
            TimeTracker.Finish("IterateTransformALL " + count);
        }
    }
}
