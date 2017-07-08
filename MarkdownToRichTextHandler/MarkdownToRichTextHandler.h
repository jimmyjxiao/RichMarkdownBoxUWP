#pragma once
#include "../Hoedown-UWPLIB/hoedown/src/document.h"
namespace MarkdownToRichTextHandler
{

	class FormattedDocumentRenderer
	{
	public:
		FormattedDocumentRenderer(Windows::UI::Text::ITextDocument^ doc);
		void getDocumentFromMD(Platform::String^ md);
		~FormattedDocumentRenderer();
	private:
		hoedown_document* doc;
		static std::wstring getStringfromHoeBuf(const hoedown_buffer* content);
		Windows::UI::Text::ITextRange^ range;
		hoedown_renderer * myRenderer;
		Windows::UI::Text::ITextDocument^ OutputDoc;
		static void _paragraph(hoedown_buffer *ob, const hoedown_buffer *content, const hoedown_renderer_data *data);
		static void _header(hoedown_buffer *ob, const hoedown_buffer *content, int level, const hoedown_renderer_data *data);
		static int _underline(hoedown_buffer *ob, const hoedown_buffer *content, const hoedown_renderer_data *data);
		static int _italic(hoedown_buffer *ob, const hoedown_buffer *content, const hoedown_renderer_data *data);
		static int _bold(hoedown_buffer *ob, const hoedown_buffer *content, const hoedown_renderer_data *data);
		static int _linebreak(hoedown_buffer *ob, const hoedown_renderer_data *data);
		static int _italicANDbold(hoedown_buffer *ob, const hoedown_buffer *content, const hoedown_renderer_data *data);
		static void _normalText(hoedown_buffer *ob, const hoedown_buffer *text, const hoedown_renderer_data *data);

	};
	public ref class MarkdownHelper sealed
	{
	public:
		MarkdownHelper(Windows::UI::Text::ITextDocument^ doc);
		MarkdownHelper(){}
		virtual ~MarkdownHelper()
		{
			delete docSetHelper;
		}
		Platform::String^ getHtmlFromMarkdown(Platform::String^ md);
		void setMarkdowntoDocument(Platform::String^ md)
		{
			docSetHelper->getDocumentFromMD(md);
		}
	private:
		FormattedDocumentRenderer * docSetHelper;
	};
}
