using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Transformation;

namespace Flarial.Launcher.Controls.SegmentedBar;

public partial class SegmentedBar : UserControl
{
    // Properties
    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<SegmentedBar, IEnumerable>(nameof(Items));

    public IEnumerable Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SegmentedBar, int>(nameof(SelectedIndex), defaultValue: 0);

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SegmentedBar, object?>(nameof(SelectedItem));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public SegmentedBar()
    {
        InitializeComponent();
        //DataContext = this;
        SizeChanged += (s, e) => UpdateVisuals();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // CRITICAL FIX: Ensure visuals update when control is physically attached to the screen
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateVisuals();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsProperty)
        {
            // Handle ObservableCollection updates
            if (change.OldValue is INotifyCollectionChanged oldList)
                oldList.CollectionChanged -= OnCollectionChanged;
                
            if (change.NewValue is INotifyCollectionChanged newList)
                newList.CollectionChanged += OnCollectionChanged;

            UpdateVisuals();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            var list = Items.Cast<object>().ToList();
            if (SelectedIndex >= 0 && SelectedIndex < list.Count)
            {
                SelectedItem = list[SelectedIndex];
            }
            UpdateVisuals();
        }
        else if (change.Property == SelectedItemProperty)
        {
            var newItem = change.NewValue;
            var list = Items?.Cast<object>().ToList();
            if (list == null) return;
            if (newItem == null) return;
            var newIndex = list.IndexOf(newItem);
            if (newIndex != -1 && newIndex != SelectedIndex)
                SelectedIndex = newIndex;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVisuals();
    }

    private void OnRadioButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            SelectedItem = rb.DataContext;
        }
    }
    
    private void UpdateVisuals()
    {
        var list = Items.Cast<object>().ToList();
        if (list.Count == 0) return;

        var indicator = this.FindControl<Border>("SelectionIndicator");
        var container = this.FindControl<ItemsControl>("ItemsContainer");
        
        if (indicator == null || container == null) return;
        
        if (container.Bounds.Width <= 0) return;

        var itemWidth = container.Bounds.Width / list.Count;
        var targetX = itemWidth * SelectedIndex;
        indicator.Width = itemWidth - 10;
        
        indicator.RenderTransform = TransformOperations.Parse($"translate({targetX - 25}px, 0px)");
    }
}