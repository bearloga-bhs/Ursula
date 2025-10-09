using Godot;
using System;

public partial class CowAnim : BaseAnimation
{   

    public override async void PlayUseAnimation()
    {
        currentState = State.Use;
        PlayEatingAnimation();
        //PlayHeadSpinAnimation();
    }


}
