using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace AvRichTextBox;

public partial class RichTextBox
{
   public static readonly StyledProperty<FlowDocument> FlowDocumentProperty =
   AvaloniaProperty.Register<RichTextBox, FlowDocument>(nameof(FlowDocument), defaultValue: new FlowDocument(), defaultBindingMode: BindingMode.TwoWay);

   public FlowDocument FlowDocument
   {
      get => GetValue(FlowDocumentProperty);
      set => SetValue(FlowDocumentProperty, value);
   }

   public static readonly StyledProperty<bool> ShowDebuggerPanelInDebugModeProperty = AvaloniaProperty.Register<RichTextBox, bool>(nameof(ShowDebuggerPanelInDebugMode), false);
   public bool ShowDebuggerPanelInDebugMode
   {

      get => GetValue(ShowDebuggerPanelInDebugModeProperty);
      set { SetValue(ShowDebuggerPanelInDebugModeProperty, value); ToggleDebuggerPanel(value); }
   }

   public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<RichTextBox, bool>(nameof(IsReadOnly), false);
   public bool IsReadOnly
   {
      get => GetValue(IsReadOnlyProperty);
      set { SetValue(IsReadOnlyProperty, value);  }
   }

   public static readonly StyledProperty<Color> CaretColorProperty = AvaloniaProperty.Register<RichTextBox, Color>(nameof(CaretColor), Colors.Black);
   public Color CaretColor
   {
      get => GetValue(CaretColorProperty);
      set { SetValue(CaretColorProperty, value); }
   }

   public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty = AvaloniaProperty.Register<RichTextBox, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility), ScrollBarVisibility.Auto);
   public ScrollBarVisibility VerticalScrollBarVisibility
   {
      get => GetValue(VerticalScrollBarVisibilityProperty);
      set { SetValue(VerticalScrollBarVisibilityProperty, value); }
   }
}
