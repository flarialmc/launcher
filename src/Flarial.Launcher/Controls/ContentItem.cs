namespace Flarial.Launcher.Controls;

abstract class ContentItem<T>
{
    internal abstract T Value { get; }
    protected abstract string String { get; }
    public override string ToString() => String;
}