using Game.Logic.Currencys;

namespace Game.Logic.Utils;

public class Money
{
    public static int GetMithril(long money)
    {
        return (int)(money / 100L / 100L / 1000L / 1000L % 1000L);
    }

    public static int GetPlatinum(long money)
    {
        return (int)(money / 100L / 100L / 1000L % 1000L);
    }

    public static int GetGold(long money)
    {
        return (int)(money / 100L / 100L % 1000L);
    }

    public static int GetSilver(long money)
    {
        return (int)(money / 100L % 100L);
    }

    public static int GetCopper(long money)
    {
        return (int)(money % 100L);
    }

    public static long GetMoney(int mithril, int platinum, int gold, int silver, int copper)
    {
        return ((((long)mithril * 1000L + (long)platinum) * 1000L + (long)gold) * 100L + (long)silver) * 100L + (long)copper;
    }

    public static string GetString(long money)
    {
        return Currency.Copper.Mint(money).ToText();
    }

    public static string GetShortString(long money)
    {
        return Currency.Copper.Mint(money).ToAbbreviatedText();
    }
}