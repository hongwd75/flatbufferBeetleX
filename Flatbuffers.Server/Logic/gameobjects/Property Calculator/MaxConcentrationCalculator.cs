namespace Game.Logic.PropertyCalc;

[PropertyCalculator(eProperty.MaxConcentration)]
public class MaxConcentrationCalculator : PropertyCalculator
{
    public MaxConcentrationCalculator() {}

    public override int CalcValue(GameLiving living, eProperty property) 
    {
        if (living is GamePlayer) 
        {
            GamePlayer player = living as GamePlayer;
            if (player.CharacterClass.ManaStat == eStat.UNDEFINED) 
                return 1000000;

            int concBase = (int)((player.Level * 4) * 2.2);
            int stat = player.GetModified((eProperty)player.CharacterClass.ManaStat);
            int factor = (stat > 50) ? (stat - 50) / 2 : (stat - 50);
            int conc = (concBase + concBase * factor / 100) / 2;
            conc = (int)(player.Effectiveness * (double)conc);

            if (conc < 0)
            {
                conc = 0;
            }
            return conc;
        } 
        else 
        {
            return 1000000;
        }
    }
}