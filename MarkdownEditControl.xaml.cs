using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RichMarkdownBox
{
    public sealed partial class MarkdownEditControl : UserControl
    {
        public RichMarkdownBox Editor
        {
            get
            {
                return editor;
            }
        }
        public void setMarkdown(string m)
        {
            editor.setMarkdown(m);
        }
        public string getMarkdown()
        {
            return editor.getMarkdown();
        }
        private bool formatEffectToBool(Windows.UI.Text.FormatEffect e)
        {
            return (e == Windows.UI.Text.FormatEffect.On);
        }
        public MarkdownEditControl()
        {
            this.InitializeComponent();
        }
        enum operation : byte
        {
            bold, underline, italic, h1,h2,h3,h4, noheader
        };
        private void TextFormatButton_Click(object sender, RoutedEventArgs e)
        {
            operation ToDo = 0;
            var obj = (Windows.UI.Xaml.FrameworkElement)sender;
            switch (obj.Name)
            {
                case "BoldButton":
                    ToDo = operation.bold;
                    break;
                case "ItalicButton":
                    ToDo = operation.italic;
                    break;
                case "underLineButton":
                    ToDo = operation.underline;
                    break;
            }
            executeStyling(ToDo);
        }
        private void executeStyling(operation z)
        {
            Windows.UI.Text.ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                switch (z)
                {
                    case operation.bold:
                        charFormatting.Bold = Windows.UI.Text.FormatEffect.Toggle;
                        BoldButton.IsChecked = formatEffectToBool(charFormatting.Bold);
                        break;
                    case operation.italic:
                        charFormatting.Italic = Windows.UI.Text.FormatEffect.Toggle;
                        ItalicButton.IsChecked = formatEffectToBool(charFormatting.Italic);
                        break;
                    case operation.underline:
                        if (charFormatting.Underline == Windows.UI.Text.UnderlineType.None)
                            charFormatting.Underline = Windows.UI.Text.UnderlineType.Single;
                        else
                            charFormatting.Underline = Windows.UI.Text.UnderlineType.None;
                        underLineButton.IsChecked = (charFormatting.Underline != Windows.UI.Text.UnderlineType.None);
                        break;
                    case operation.h1:
                        charFormatting.Weight = 600;
                        charFormatting.Size = 20.5F;
                        break;
                    case operation.h2:
                        charFormatting.Weight = 500;
                        charFormatting.Size = 20.5F;
                        break;
                    case operation.h3:
                        charFormatting.Weight = 600;
                        charFormatting.Size = 18.3F;
                        break;
                    case operation.h4:
                        charFormatting.Weight = 500;
                        charFormatting.Size = 18.3F;
                        break;
                    case operation.noheader:
                        var def = editor.Document.GetDefaultCharacterFormat();
                        charFormatting.Weight = def.Weight;
                        charFormatting.Size = def.Size;
                        break;
                        
                }
            }
        }
        private bool doNotrun_selchanged = false;
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!doNotrun_selchanged)
            {
                operation ToDo = 0;
                var obj = (Windows.UI.Xaml.Controls.ComboBox)sender;
                switch (((Windows.UI.Xaml.Controls.ComboBoxItem)obj.SelectedItem).Name)
                {
                    case "noHeader":
                        ToDo = operation.noheader;
                        break;
                    case "h1Button":
                        ToDo = operation.h1;
                        break;
                    case "h2Button":
                        ToDo = operation.h2;
                        break;
                    case "h3Button":
                        ToDo = operation.h3;
                        break;
                    case "h4Button":
                        ToDo = operation.h4;
                        break;
                }
                editor.Focus(FocusState.Programmatic);
                editor.Document.Selection.SetRange(tempsel.StartPosition, tempsel.EndPosition);
                executeStyling(ToDo);
            }
            else
                doNotrun_selchanged = false;
        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            doNotrun_selchanged = true;
            var defaultChar = editor.Document.GetDefaultCharacterFormat();
            var charType = editor.Document.Selection.CharacterFormat;
            BoldButton.IsChecked = formatEffectToBool(charType.Bold);
            BoldButton.IsEnabled = charType.Size == defaultChar.Size;
            ItalicButton.IsChecked = formatEffectToBool(charType.Italic);
            underLineButton.IsChecked = (charType.Underline != Windows.UI.Text.UnderlineType.None);

            if (charType.Size == defaultChar.Size)
                headerSelector.SelectedIndex = 0;
            else if (charType.Weight == 600 && charType.Size == 20.5)
                headerSelector.SelectedIndex = 1;
            else if (charType.Weight == 500 && charType.Size == 20.5)
                headerSelector.SelectedIndex = 2;
            else if (charType.Weight == 600 && charType.Size == 18.3)
                headerSelector.SelectedIndex = 3;
            else if (charType.Weight == 500 && charType.Size == 18.3)
                headerSelector.SelectedIndex = 4;
            else
                headerSelector.SelectedIndex = 0;
        }
        private Windows.UI.Text.ITextSelection tempsel;


        private void editor_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            tempsel = editor.Document.Selection;
        }

        private void headerSelector_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void headerSelector_DropDownOpened(object sender, object e)
        {
            tempsel = editor.Document.Selection;
            doNotrun_selchanged = false;
        }
    }
}
