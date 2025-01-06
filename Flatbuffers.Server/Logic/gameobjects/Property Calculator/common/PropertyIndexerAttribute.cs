namespace Game.Logic.PropertyCalc;
[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
public class PropertyIndexerAttribute : Attribute
{
    private readonly eBuffBonusType m_min;
    private readonly eBuffBonusType m_max;

    public eBuffBonusType Min
    {
        get { return m_min; }
    }

    public eBuffBonusType Max
    {
        get { return m_max; }
    }

    public PropertyIndexerAttribute(eBuffBonusType prop) : this(prop, prop)
    {
    }

    public PropertyIndexerAttribute(eBuffBonusType min, eBuffBonusType max)
    {
        if (min > max)
            throw new ArgumentException($"min 값이 max 보다 큼 (min={(int)min} / max={(int)max})");
        if (min < 0 || max > eBuffBonusType.MaxBonusType)
            throw new ArgumentOutOfRangeException("max", (int)max, $"값은 0 .. eBuffBonusType.MaxBonusType({(int)eBuffBonusType.MaxBonusType}) 범위 내에 있어야 함");
        m_min = min;
        m_max = max;
    }
}