# RichMarkdownBoxUWP
A rich text control for Markdown that works with UWP.


Can getMarkdown from the RichEditBox, and you can setMarkdown too. 

setMarkdown functionality depends on the C markdown parser hoedown.

Markdown syntax follows Reddit, rather than CommonMark, or other markdown flavors. For example, underline is done by 6 headers. Should be pretty easy to edit for other features, other than the basic bold, italic, and header that supported right now. 

The `markdowneditcontrol` gives you basic editor functionality like bold, italic, and markdown headings, while the `richmarkdownbox` control only gives you a `richeditbox` that has `getMarkdown()` and `setMarkdown()` functionality. 
