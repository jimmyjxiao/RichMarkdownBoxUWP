using System;


namespace RichMarkdownBox
{
   
    public sealed class RichMarkdownBox : Windows.UI.Xaml.Controls.RichEditBox
    {
        public RichMarkdownBox()
        {
            var c = Document.GetDefaultCharacterFormat();
            c.Size = 16;
            Document.SetDefaultCharacterFormat(c);
            Paste += RichMarkdownBox_Paste;
        }
        private async void RichMarkdownBox_Paste(object sender, Windows.UI.Xaml.Controls.TextControlPasteEventArgs e)
        {
            //only paste plaintext
            //TODO: paste formatting, but remove pictures, and handle font size and weight to markdown spec
            e.Handled = true;
            var clip = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if(clip.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
            {
                string plaintext = await clip.GetTextAsync();
                Document.Selection.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults, plaintext);
            }
        }

        public string getMarkdown()
        {
            // Takes a RichEditBox control and returns a 
            // simple markdown version of its contents 
            string strMD;
            float shtSize;
            int lngOriginalStart, lngOriginalLength;
            int intCount;
            int FontWeight;
            Windows.UI.Text.ITextRange tr = Document.GetRange(0, 40000);
            // If nothing in the box, exit 
            if (tr.Length != 0)
            {
                var defaultChar = Document.GetDefaultCharacterFormat();
                // Store original selections, then select first character 
                lngOriginalStart = tr.StartPosition;
                lngOriginalLength = tr.EndPosition;
                tr.SetRange(0, 1);
                // intialize string
                strMD = "";
                // Set up initial parameters 
               
                shtSize = tr.CharacterFormat.Size;
                FontWeight = tr.CharacterFormat.Weight;
                var isBold = tr.CharacterFormat.Bold;
                var isItalic = tr.CharacterFormat.Italic;
                var isUnderline = tr.CharacterFormat.Underline;
                //Debug.WriteLine("Colour: " + strColour); 
                // Include first 'style' parameters in the HTML 
                if(FontWeight != defaultChar.Weight && isUnderline == Windows.UI.Text.UnderlineType.None)
                {
                    if (shtSize != defaultChar.Size)
                    {
                        switch (FontWeight)
                        {
                            case 600:
                                if (shtSize == 20.5)
                                    strMD += "#";
                                else if (shtSize == 18.3)
                                    strMD += "###";
                                break;
                            case 500:
                                if (shtSize == 20.5)
                                    strMD += "##";
                                else if (shtSize == 18.3)
                                    strMD += "####";
                                break;
                        }
                    }
                    else if (isBold == Windows.UI.Text.FormatEffect.On)
                    {
                        strMD += "**";
                    }
                }
                //Reddit Markdown, which this control is designed for, does underline through six headers. It doesn't seem to work with other headers hence the else if.
                //we can combine it with the bold tag, which is as close as we'll get to a header'd underline
                else if(isUnderline != Windows.UI.Text.UnderlineType.None)
                {
                    strMD += "######";
                    if(isBold == Windows.UI.Text.FormatEffect.On)
                    {
                        strMD += "**";
                    }
                }
                if(isItalic == Windows.UI.Text.FormatEffect.On)
                {
                    strMD += "*";
                }
                
                // Finally, add our first character 
                strMD += tr.Character;
                // Loop around all remaining characters 
                for (intCount = lngOriginalStart + 1; intCount < lngOriginalLength - 1; intCount++)
                {
                    // Select current character 
                    tr.SetRange(intCount, intCount + 1);
                    
                    //     If this is a line break, add two line breaks because markdown
                    if (tr.Character == Convert.ToChar(13))
                    {
                        strMD += (Environment.NewLine + Environment.NewLine);
                        isBold = defaultChar.Bold;
                        isItalic = defaultChar.Italic;
                        FontWeight = defaultChar.Weight;
                        shtSize = defaultChar.Size;
                    }
                    else
                    {
                        //   Check/implement any changes in header/bold/underline
                        if (tr.CharacterFormat.Size != shtSize || tr.CharacterFormat.Weight != FontWeight || tr.CharacterFormat.Underline != isUnderline)
                        {
                            //Check if we need to close out the bold tag or start a new one
                            if ((FontSize == defaultChar.Size && isBold == Windows.UI.Text.FormatEffect.On && tr.CharacterFormat.Bold != Windows.UI.Text.FormatEffect.On) || (tr.CharacterFormat.Size == defaultChar.Size && tr.CharacterFormat.Bold == Windows.UI.Text.FormatEffect.On))
                            {
                                strMD += "**";
                            }
                            //else, line break once to get out of any existing tags or formatting, but check if there are any italic tags we need to close out before linebreaking
                            else
                            {
                                bool possibleReItalic = false;
                                if (isItalic == Windows.UI.Text.FormatEffect.On)
                                {
                                    strMD += "*";
                                    possibleReItalic = true;
                                }
                                strMD += Environment.NewLine;
                                //then check if we need another header
                                if (tr.CharacterFormat.Weight != defaultChar.Weight && tr.CharacterFormat.Underline == Windows.UI.Text.UnderlineType.None)
                                {
                                    if (tr.CharacterFormat.Size != defaultChar.Size)
                                    {
                                        switch (FontWeight)
                                        {
                                            case 600:
                                                if (tr.CharacterFormat.Size == 20.5)
                                                    strMD += "#";
                                                else if (tr.CharacterFormat.Size == 18.3)
                                                    strMD += "###";
                                                break;
                                            case 500:
                                                if (tr.CharacterFormat.Size == 20.5)
                                                    strMD += "##";
                                                else if (tr.CharacterFormat.Size == 18.3)
                                                    strMD += "####";
                                                break;
                                        }
                                    }
                                    else if (tr.CharacterFormat.Bold == Windows.UI.Text.FormatEffect.On)
                                    {
                                        strMD += "**";
                                    }
                                }
                                else if (tr.CharacterFormat.Underline != Windows.UI.Text.UnderlineType.None)
                                {
                                    strMD += "######";
                                    if (isBold == Windows.UI.Text.FormatEffect.On)
                                    {
                                        strMD += "**";
                                    }
                                }
                                //after we linebroke, maybe we're still in italic, which we need to redo
                                if (tr.CharacterFormat.Italic == Windows.UI.Text.FormatEffect.On && possibleReItalic)
                                {
                                    strMD += "*";
                                }
                            }
                        }
                        //    Check for italic changes 
                        if (tr.CharacterFormat.Italic != isItalic)
                        {
                            strMD += "*";
                        }
                        //    ' Add the actual character 
                        strMD += tr.Character;
                        //    ' Update variables with current style 
                        isBold = tr.CharacterFormat.Bold;
                        isItalic = tr.CharacterFormat.Italic;
                        FontWeight = tr.CharacterFormat.Weight;
                        shtSize = tr.CharacterFormat.Size;
                    }
                }
                // Close off any open bold/italic tags 
                if (isBold == Windows.UI.Text.FormatEffect.On)
                    strMD += "**";
                if (isItalic == Windows.UI.Text.FormatEffect.On)
                    strMD += "*";
                //' Restore original RichTextBox selection 
                tr.SetRange(lngOriginalStart, lngOriginalLength);
                // Return Markdown
                
                return strMD;
            }
            else return null;
        }
    }
}
