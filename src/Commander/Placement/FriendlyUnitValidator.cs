namespace Commander.Placement;

internal static class FriendlyUnitValidator
{
    public static bool IsOperational(Unit? unit, FactionHQ localHq)
    {
        if (unit == null
            || unit.Networkdisabled
            || unit is Aircraft
            || unit is Ship
            || unit.IsSlung()
            || unit.GetComponentInChildren<CargoDeploymentSystem>(true) != null)
        {
            return false;
        }

        if (unit.NetworkHQ == localHq || unit.MapHQ == localHq)
        {
            return true;
        }

        return unit is Container container
            && UnitRegistry.TryGetPersistentUnit(container.NetworkownerID, out PersistentUnit owner)
            && owner.GetHQ() == localHq;
    }
}
