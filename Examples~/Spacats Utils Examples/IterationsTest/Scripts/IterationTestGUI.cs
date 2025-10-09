using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spacats.Utils
{
    public class IterationTestGUI : GUIButtons
    {
        public int Iterations = 1_000_000;
        
        [HideInInspector] public List<EmptyClass> List;
        [HideInInspector] public List<EmptyMonoBehClass> ListMono;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Recreate();
        }

        public void Recreate()
        {
            TimeTracker.Start("IterationTestGUI Recreate");
            ClearChildren(gameObject.transform);
            if (List!=null) List.Clear();
            if (ListMono!=null) ListMono.Clear();
            Create();
            
            TimeTracker.Finish("IterationTestGUI Recreate");
            Debug.Log("Recreated: " + Iterations);
        }

        private void Create()
        {
            List = new List<EmptyClass>();
            ListMono = new List<EmptyMonoBehClass>();
            for (int i = 0; i < Iterations; i++)
            {
                EmptyClass newEmpty = new EmptyClass();
                List.Add(newEmpty);

                GameObject newGO = new GameObject();
                EmptyMonoBehClass newMonoBeh = newGO.AddComponent<EmptyMonoBehClass>();
                ListMono.Add(newMonoBeh);
                newMonoBeh.gameObject.transform.parent = gameObject.transform;
            }

        }

        protected override string GetButtonLabel(int index)
        {
            switch (index)
            {
                default: return base.GetButtonLabel(index);
                case 0:
                    if (!GUILogViewer.Instance.LoggingEnabled) return "Logging Disabled";
                    return GUILogViewer.Instance.IsOpened ? "Hide \nLog" : "Show \nLog";
                case 1: return "Recreate";
                case 2: return "Cycle For";
                case 3: return "Cycle For \nClass";
                case 4: return "Cycle For \nClass Mono";
                
                case 5: return "Foreach \nClass";
                case 6: return "Foreach \nClass Mono";
                
                case 7: return "Foreach \nClass + Method";
                case 8: return "Foreach \nClass Mono +\n Method";
                
                case 9: return "For \n MonoBeh +\n Method";
                case 10: return "For \n MonoBeh +\n Method2";
                case 11: return "BEST \nFor_KeepLength\n MonoBeh";
            }
        }

        protected override void OnButtonClick(int index)
        {
            switch (index)
            {
                default: base.OnButtonClick(index); break;
                case 0: SwitchShowHideLog(); break;
                case 1: Recreate(); break;
                
                case 2: LaunchCycleFor(); break;
                case 3: LaunchCycleForClass(); break;
                case 4: LaunchCycleForClassMono(); break;
                
                case 5: LaunchForeachClass(); break;
                case 6: LaunchForeachMonoBeh(); break;
                
                case 7: LaunchForeachClassMethod(); break;
                case 8: LaunchForeachMonoBehMethod(); break;
                
                case 9: LaunchCycleForClassMonoMethod(); break;
                case 10: LaunchCycleForClassMonoMethod2(); break;
                
                case 11: BEST_PRACTICE_LaunchCycleForClassMonoMethod(); break;
            }
        }
        
        private void SwitchShowHideLog()
        {
            if (GUILogViewer.Instance.IsOpened) GUILogViewer.Instance.CloseLog();
            else GUILogViewer.Instance.OpenLog();
        }
        
        public void ClearChildren(Transform target)
        {
            if (target == null)
            {
                Debug.LogError("ClearChildren of NULL target!");
                return;
            }

            for (int i = target.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(target.GetChild(i).gameObject);
            }
        }

        private void LaunchCycleFor()
        {
            TimeTracker.Start("LaunchCycleFor " + Iterations);

            int counter = 0;
            for (int i = 0; i < Iterations; i++)
            {
                counter += i & 1;
            }

            TimeTracker.Finish("LaunchCycleFor " + Iterations);
        }
        
        private void LaunchCycleForClass()
        {
            TimeTracker.Start("LaunchCycleForClass " + Iterations);

            int counter = 0;
            for (int i = 0; i < List.Count; i++)
            {
                counter += i & 1;
            }

            TimeTracker.Finish("LaunchCycleForClass " + Iterations);
        }
        
        private void LaunchCycleForClassMono()
        {
            TimeTracker.Start("LaunchCycleForClassMono " + Iterations);

            int counter = 0;
            for (int i = 0; i < ListMono.Count; i++)
            {
                counter += i & 1;
            }

            TimeTracker.Finish("LaunchCycleForClassMono " + Iterations);
        }
        
        private void LaunchForeachClass()
        {
            TimeTracker.Start("LaunchForeachClass " + Iterations);
            int counter = 0;
            int tmp = 0;
            foreach (EmptyClass entry in List)
            {
                tmp++;
                counter += tmp & 1;
            }

            TimeTracker.Finish("LaunchForeachClass " + Iterations);
        }
        
        private void LaunchForeachMonoBeh()
        {
            TimeTracker.Start("LaunchForeachMonoBeh " + Iterations);
            int counter = 0;
            int tmp = 0;
            foreach (EmptyMonoBehClass entry in ListMono)
            {
                tmp++;
                counter += tmp & 1;
            }

            TimeTracker.Finish("LaunchForeachMonoBeh " + Iterations);
        }
        
        private void LaunchForeachClassMethod()
        {
            TimeTracker.Start("LaunchForeachClassMethod " + Iterations);
            int counter = 0;
            int tmp = 0;
            foreach (EmptyClass entry in List)
            {
                tmp++;
                counter += tmp & 1;
                entry.SomeMethod(counter);
            }

            TimeTracker.Finish("LaunchForeachClassMethod " + Iterations);
        }
        
        private void LaunchForeachMonoBehMethod()
        {
            TimeTracker.Start("LaunchForeachMonoBehMethod " + Iterations);
            int counter = 0;
            int tmp = 0;
            foreach (EmptyMonoBehClass entry in ListMono)
            {
                tmp++;
                counter += tmp & 1;
                entry.SomeMethod(counter);
            }

            TimeTracker.Finish("LaunchForeachMonoBehMethod " + Iterations);
        }
        
        private void LaunchCycleForClassMonoMethod()
        {
            TimeTracker.Start("LaunchCycleForClassMonoMethod " + Iterations);

            int counter = 0;
            for (int i = 0; i < ListMono.Count; i++)
            {
                counter += i & 1;
                ListMono[i].SomeMethod(counter);
            }

            TimeTracker.Finish("LaunchCycleForClassMonoMethod " + Iterations);
        }
        
        private void LaunchCycleForClassMonoMethod2()
        {
            TimeTracker.Start("LaunchCycleForClassMonoMethod2 " + Iterations);

            int counter = 0;
            for (int i = 0; i < ListMono.Count; i++)
            {
                counter += i & 1;
                ListMono[i].Value = counter;//faster then calling a SomeMethod about 0.1ms per 100k calls
            }

            TimeTracker.Finish("LaunchCycleForClassMonoMethod2 " + Iterations);
        }
        
        private void BEST_PRACTICE_LaunchCycleForClassMonoMethod()
        {
            TimeTracker.Start("BEST_PRACTICE_LaunchCycleForClassMonoMethod " + Iterations);

            int count = ListMono.Count;
            int counter = 0;
            for (int i = 0; i < count; i++)
            {
                counter += i & 1;
                ListMono[i].SomeMethod(counter);
            }

            TimeTracker.Finish("BEST_PRACTICE_LaunchCycleForClassMonoMethod " + Iterations);
        }
    }
}
