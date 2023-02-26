using UnityEngine;

public class DisableCanvasInState : StateMachineBehaviour
{
    private GameObject _canvas;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        GetCanvas(animator).SetActive(false);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        GetCanvas(animator).SetActive(true);
    }

    private GameObject GetCanvas(Animator animator)
    {
        if (_canvas == null)
        {
            _canvas = animator.GetComponentInChildren<Canvas>().gameObject;
        }

        return _canvas;
    }
}
