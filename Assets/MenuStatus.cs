using System;
public static class MenuStatus
{
    public static bool IsActive { get; private set; }

    public static void ChangeStatus()
    {
        if(IsActive) IsActive = false;
        else IsActive = true;
        OnStatusChange();
    }

    private static Action OnStatusChange;

    public static void AddOnStatusChangeListener(Action action)
    {
        OnStatusChange -= action;
        OnStatusChange += action;
    }

}
