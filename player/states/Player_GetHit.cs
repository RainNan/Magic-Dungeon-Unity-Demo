public class Player_GetHit : IState<Player>
{
    public void Enter(Player owner)
    {
        owner.anim.SetTrigger("GetHit");
        owner.PlayFlash();
    }

    public void Update(Player owner)
    {
        owner.Freezen();
    }

    public void FixedUpdate(Player owner)
    {
    }

    public void Exit(Player owner)
    {
        owner.UnFreezen();
    }
}
