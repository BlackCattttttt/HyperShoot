using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;

public class EvenGlobalManager : Singleton<EvenGlobalManager>
{
    public Relay OnStartLoadScene = new Relay();
    public Relay OnFinishLoadScene = new Relay();
    //public Relay OnUpdateSetting = new Relay();

    public Relay OnLoadLevel = new Relay();
    public Relay OnStartPlay = new Relay();
    public Relay OnEndPlay = new Relay();

    // public Relay<bool> OnActiveTarget = new Relay<bool>();
    //public Relay OnEndReached = new Relay();

    public Relay OnKillEnemy = new Relay();
    public Relay OnContinue = new Relay();

    //public Relay<int> OnStartEffectBonus = new Relay<int>();
    //public Relay OnEndEffectBonus = new Relay();
    //public Relay<Arrow> OnArrowDisappear = new Relay<Arrow>();
    public Relay OnResume = new Relay();
}
