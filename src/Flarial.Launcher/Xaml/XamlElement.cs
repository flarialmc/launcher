using Windows.UI.Xaml;

namespace Flarial.Launcher.Xaml;

abstract class XamlElement<T> where T : UIElement
{
    internal readonly T _this;

    internal XamlElement(T element) => _this = element; 
}