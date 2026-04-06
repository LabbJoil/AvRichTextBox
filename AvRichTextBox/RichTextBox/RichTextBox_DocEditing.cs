using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AvRichTextBox;

public partial class RichTextBox
{

   private void RichTextBox_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
   {
      if (IsReadOnly) return;

      FlowDoc.InsertText(e.Text);
      UpdateCurrentParagraphLayout();

      if (PreeditOverlay.IsVisible)
         HideIMEOverlay();
   }

   // TODO: вынести метод в спец файл для внешних портов
   public void InsertVariable(string name)
   {
      if (IsReadOnly)
         return;

      string text = $"[[{name}]]";
      var selection = FlowDoc.Selection;

      if (selection.Length > 0)
         FlowDoc.DeleteSelection();

      var paragraph = selection.StartParagraph;
      var startInline = FlowDoc.GetStartInline(selection.Start);

      if (startInline is not IEditable editable)
         return;

      int charPos = editable.GetCharPosInInline(selection.Start);

      if (editable is not EditableRun run)
         return;

      var splitRuns = FlowDoc.SplitRunAtPos(selection.Start, run, charPos);

      var beforeRun = splitRuns[0];
      int runIndex = paragraph.Inlines.IndexOf(beforeRun);

      var variableRun = new EditableRun(text)
      {
         IsVariable = true,
         VariableName = name,
         FontFamily = run.FontFamily,
         FontSize = run.FontSize,
         FontWeight = run.FontWeight,
         FontStyle = run.FontStyle,
         TextDecorations = run.TextDecorations,
         Foreground = run.Foreground,
         Background = run.Background
      };

      paragraph.Inlines.Insert(runIndex + 1, variableRun);

      paragraph.CallRequestInlinesUpdate();
      FlowDoc.UpdateBlockAndInlineStarts(paragraph);

      FlowDoc.Select(selection.Start + text.Length, 0);
      FlowDoc.UpdateSelection();
   }

   // TODO: вынести метод в спец файл для внешних портов
   public void DeleteVarByName(string name)
   {
      foreach (var block in FlowDoc.Blocks)
      {
         if (block is not Paragraph paragraph)
            continue;

         for (int i = paragraph.Inlines.Count - 1; i >= 0; i--)
            if (paragraph.Inlines[i] is EditableRun run && run.IsVariable && run.VariableName == name)
               paragraph.Inlines.RemoveAt(i);

         paragraph.CallRequestInlinesUpdate();
         FlowDoc.UpdateBlockAndInlineStarts(paragraph);
      }

      FlowDoc.UpdateSelection();
   }

   // TODO: вынести метод в спец файл для внешних портов
   public void ChangeVarToValue(string name, string value)
   {
      foreach (var block in FlowDoc.Blocks)
      {
         if (block is not Paragraph paragraph)
            continue;

         for (int i = paragraph.Inlines.Count - 1; i >= 0; i--)
            if (paragraph.Inlines[i] is EditableRun run && run.IsVariable && run.VariableName == name)
               run.Text = value;

         paragraph.CallRequestInlinesUpdate();
         FlowDoc.UpdateBlockAndInlineStarts(paragraph);
      }

      FlowDoc.UpdateSelection();
   }

   // TODO: вынести метод в спец файл для внешних портов
   public void ChangeVars(string oldVarName, string newVarName)
   {
      foreach (var block in FlowDoc.Blocks)
      {
         if (block is not Paragraph paragraph)
            continue;

         for (int i = paragraph.Inlines.Count - 1; i >= 0; i--)
            if (paragraph.Inlines[i] is EditableRun run && run.IsVariable && run.VariableName == oldVarName)
            {
               run.VariableName = newVarName;
               run.Text = $"[[{newVarName}]]";
            }

         paragraph.CallRequestInlinesUpdate();
         FlowDoc.UpdateBlockAndInlineStarts(paragraph);
      }

      FlowDoc.UpdateSelection();
   }

   // TODO: вынести метод в спец файл для внешних портов
   public void DeleteAllVariables()
   {
      foreach (var block in FlowDoc.Blocks)
      {
         if (block is not Paragraph paragraph)
            continue;

         for (int i = paragraph.Inlines.Count - 1; i >= 0; i--)
         {
            if (paragraph.Inlines[i] is EditableRun r && r.IsVariable)
               paragraph.Inlines.RemoveAt(i);
         }

         paragraph.CallRequestInlinesUpdate();
         FlowDoc.UpdateBlockAndInlineStarts(paragraph);
      }
   }

   private void HideIMEOverlay()
   {
      _preeditText = "";
      PreeditOverlay.IsVisible = false;

   }

   internal void UpdateCurrentParagraphLayout()
   {
      this.UpdateLayout();
      rtbVM.UpdateCaretVisible();
   }

   internal void InsertParagraph()
   {
      if (IsReadOnly) return;

      FlowDoc.InsertParagraph(true, FlowDoc.Selection.Start);
      UpdateCurrentParagraphLayout();

   }

   internal void InsertLineBreak()
   {
      if (IsReadOnly) return;

      FlowDoc.InsertLineBreak();
      UpdateCurrentParagraphLayout();

   }

   public void SearchText(string searchText)
   {
      MatchCollection matches = Regex.Matches(FlowDoc.Text, searchText);

      if (matches.Count > 0)
         FlowDoc.Select(matches[0].Index, matches[0].Length);


      foreach (Match m in matches)
      {
         TextRange trange = new(FlowDoc, m.Index, m.Index + m.Length);
         FlowDoc.ApplyFormattingRange(Inline.FontStretchProperty, FontStretch.UltraCondensed, trange);
         FlowDoc.ApplyFormattingRange(Inline.ForegroundProperty, new SolidColorBrush(Colors.BlueViolet), trange);
         FlowDoc.ApplyFormattingRange(Inline.BackgroundProperty, new SolidColorBrush(Colors.Wheat), trange);
      }



   }


   private void PerformDelete(bool backspace)
   {
      if (IsReadOnly) return;

      if (FlowDoc.Selection!.Length > 0)
         FlowDoc.DeleteSelection();
      else
      {
         if (backspace)
            if (FlowDoc.Selection.Start == 0)
            {
               if (FlowDoc.Blocks.Count > 0 && FlowDoc.Blocks[0] is Paragraph paragraph)
                  paragraph.TextAlignment = TextAlignment.Left;
               rtbVM.CaretMargin = new Thickness(0, 0, 0, 0);
               return;
            }
            else
            if (FlowDoc.Selection.Start >= FlowDoc.Selection.StartParagraph.StartInDoc + FlowDoc.Selection.StartParagraph.BlockLength)
               return;

         FlowDoc.DeleteChar(backspace);
      }

      UpdateCurrentParagraphLayout();
   }


}
