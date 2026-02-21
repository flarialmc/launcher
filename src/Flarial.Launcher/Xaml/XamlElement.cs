using Windows.UI.Xaml;

namespace Flarial.Launcher.Xaml;

abstract class XamlElement<T>(T element) where T : UIElement
{
    protected readonly T @this = element;

    public static T operator ~(XamlElement<T> element) => element.@this;
}