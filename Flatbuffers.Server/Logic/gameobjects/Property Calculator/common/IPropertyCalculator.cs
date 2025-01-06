namespace Game.Logic.PropertyCalc
{
    public interface IPropertyCalculator
    {
        /// <summary>
        /// Calculates the final property value.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        int CalcValue(GameLiving living, eProperty property);
        int CalcValueBase(GameLiving living, eProperty property);

        /// <summary>
        /// Calculates the modified value from buff bonuses only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        int CalcValueFromBuffs(GameLiving living, eProperty property);

        /// <summary>
        /// Calculates the modified value from item bonuses only.
        /// </summary>
        /// <param name="living"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        int CalcValueFromItems(GameLiving living, eProperty property);
    }
}