using System.Data;
using System.Runtime.InteropServices;
using Windows.UI.Xaml;


abstract class XamlElement<T> : XamlElement where T : UIElement
{
    protected T Element { get; }

    protected XamlDispatcher Dispatcher {get;}

    internal XamlElement(T element) : base(element)
    {
        Element = element;
        Dispatcher = new();
    }
}

abstract class XamlElement
{
    readonly UIElement _element;

    internal XamlElement(UIElement element)
    {
        _element = element;
    }

    public static explicit operator UIElement(XamlElement element) => element._element;
}