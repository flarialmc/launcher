using Windows.UI.Xaml;

namespace Flarial.Launcher.Xaml;

abstract class XamlElement<T> where T : UIElement
{
    internal readonly T @this;

    internal XamlElement(T element) => @this = element; 
}