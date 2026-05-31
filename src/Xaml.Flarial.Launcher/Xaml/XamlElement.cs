namespace Flarial.Launcher.Xaml;

abstract class XamlElement<T>(T element) where T : Windows.UI.Xaml.UIElement
{
    readonly T _element = element;
    public static T operator ~(XamlElement<T> element) => element._element;
}