
public interface CC_IState
{
    public void OnStart();
    public void OnEnter();
    public void OnUpdate();
    public void OnFixedUpdate();
    public void OnExit();
    public bool CanEnter(CC_IState currentState);
    public bool CanExit();
}
