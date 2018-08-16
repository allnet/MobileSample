using UnityEngine;

namespace AllNetXR
{
    //  Derive from this class to change behavior game modes
    public abstract class UIViewControllerSequential : MonoBehaviour
    {
        public bool IsReady;
        public bool CanDismiss = true;
        public GameObject View;
        protected bool DebugMode;
        protected LoopSequencer sequencer;
        protected float EnterTime;

        protected virtual void Awake()
        {

        }
    }

}