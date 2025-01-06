namespace Game.Logic.PropertyCalc;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
public class PropertyCalculatorAttribute : Attribute
{
    /// <summary>
    /// Defines lowest property of calculator properties range
    /// </summary>
    private readonly eProperty m_min;
    /// <summary>
    /// Defines highest property of calculator properties range
    /// </summary>
    private readonly eProperty m_max;

    /// <summary>
    /// Gets the lowest property of calculator properties range
    /// </summary>
    public eProperty Min
    {
        get { return m_min; }
    }

    /// <summary>
    /// Gets the highest property of calculator properties range
    /// </summary>
    public eProperty Max
    {
        get { return m_max; }
    }

    /// <summary>
    /// Constructs a new calculator attribute for just one property
    /// </summary>
    /// <param name="prop">The property calculator is assigned to</param>
    public PropertyCalculatorAttribute(eProperty prop) : this(prop, prop)
    {
    }

    /// <summary>
    /// Constructs a new calculator attribute for range of properties
    /// </summary>
    /// <param name="min">The lowest property in range</param>
    /// <param name="max">The highest property in range</param>
    public PropertyCalculatorAttribute(eProperty min, eProperty max)
    {
        if (min > max)
            throw new ArgumentException($"min 값이 max 보다 큼 (min={(int)min} / max={(int)max})");
        if (min < 0 || max > eProperty.MaxProperty)
            throw new ArgumentOutOfRangeException("max", (int)max, $"값은 0 .. eProperty.MaxProperty({(int)eProperty.MaxProperty}) 범위 내에 있어야 함");
        m_min = min;
        m_max = max;
    }
}