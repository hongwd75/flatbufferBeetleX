using log4net;

namespace Game.Logic.PropertyCalc;

public class PropertyCalculator : IPropertyCalculator
{
    protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public PropertyCalculator()
    {
    }

    /// <summary>
    /// calculates the final property value
    /// </summary>
    /// <param name="living"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual int CalcValue(GameLiving living, eProperty property) 
    {
        return 0;
    }

    public virtual int CalcValueBase(GameLiving living, eProperty property) 
    {
        return 0;
    }

    /// <summary>
    /// Calculates the property value for this living's buff bonuses only.
    /// </summary>
    /// <param name="living"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual int CalcValueFromBuffs(GameLiving living, eProperty property)
    {
        return 0;
    }

    /// <summary>
    /// Calculates the property value for this living's item bonuses only.
    /// </summary>
    /// <param name="living"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public virtual int CalcValueFromItems(GameLiving living, eProperty property)
    {
        return 0;
    }
}