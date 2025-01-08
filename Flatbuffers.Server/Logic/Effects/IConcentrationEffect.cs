namespace Game.Logic.Effects;

public interface IConcentrationEffect
{
    string Name { get; }
    string OwnerName { get; }
    ushort Icon { get; }
    byte Concentration { get; }
    void Cancel(bool playerCanceled);
}