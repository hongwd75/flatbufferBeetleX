namespace Game.Logic.PropertyCalc;

public interface IPropertyIndexer
{
    int this[int index] { get; set; }
    int this[eProperty index] { get; set; }   
}