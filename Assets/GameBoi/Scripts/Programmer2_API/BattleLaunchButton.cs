using UnityEngine;

public class BattleLaunchButton : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject hubRoot;
    [SerializeField] private GameObject battleRoot;

    [Header("Battle")]
    [SerializeField] private AutoBattleController autoBattleController;

    [Header("Inspector Test Data")]
    [SerializeField] private BattleLaunchData battleData = new BattleLaunchData();

    public void LaunchFromInspector()
    {
        Launch(battleData);
    }

    public void Launch(BattleLaunchData data)
    {
        if (hubRoot != null)
            hubRoot.SetActive(false);

        if (battleRoot != null)
            battleRoot.SetActive(true);

        if (autoBattleController != null)
            autoBattleController.SetupExternalBattle(data);
    }

    public void ReturnToHub()
    {
        if (autoBattleController != null)
            autoBattleController.StopBattleForHub();

        if (battleRoot != null)
            battleRoot.SetActive(false);

        if (hubRoot != null)
            hubRoot.SetActive(true);
    }
}
