#include <string>
#include "MarkdownToRichTextHandler.h"
#include <locale>
#include <codecvt>
#include "../Hoedown-UWPLIB/hoedown/src/document.h"
#include "../Hoedown-UWPLIB/hoedown/src/html.h"
#include <ppltasks.h>
using namespace MarkdownToRichTextHandler;
using namespace Platform;

MarkdownHelper::MarkdownHelper(Windows::UI::Text::ITextDocument^ doc) : docSetHelper(new FormattedDocumentRenderer(doc))
{
}

Platform::String ^ MarkdownToRichTextHandler::MarkdownHelper::getHtmlFromMarkdown(Platform::String ^ md)
{
	static std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> conv;
	std::string mdstr = conv.to_bytes(md->Data());
	hoedown_renderer *renderer = hoedown_html_renderer_new(hoedown_html_flags::HOEDOWN_HTML_USE_XHTML, 0);
	hoedown_document * doc = hoedown_document_new(renderer, (hoedown_extensions)0, 16);
	auto buf = hoedown_buffer_new(mdstr.size());
	hoedown_document_render(doc, buf, (const uint8_t*)mdstr.data(), mdstr.size());
	auto returning = ref new Platform::String(conv.from_bytes(hoedown_buffer_cstr(buf)).data());
	hoedown_buffer_free(buf);
	hoedown_html_renderer_free(renderer);
	hoedown_document_free(doc);
	return returning;
}


MarkdownToRichTextHandler::FormattedDocumentRenderer::FormattedDocumentRenderer(Windows::UI::Text::ITextDocument^ doc)
{
	OutputDoc = doc;
	myRenderer = new hoedown_renderer();
	myRenderer->opaque = this;
	myRenderer->linebreak = _linebreak;
	myRenderer->underline = _underline;
	myRenderer->emphasis = _italic;
	myRenderer->double_emphasis = _bold;
	myRenderer->triple_emphasis = _italicANDbold;
	myRenderer->header = _header;
	myRenderer->normal_text = _normalText;
	myRenderer->paragraph = _paragraph;
	range = OutputDoc->GetRange(0, 0);
}

void MarkdownToRichTextHandler::FormattedDocumentRenderer::getDocumentFromMD(Platform::String ^ md)
{
	OutputDoc->GetRange(0, INT_MAX)->CharacterFormat = OutputDoc->GetDefaultCharacterFormat();
	OutputDoc->SetText(Windows::UI::Text::TextSetOptions::ApplyRtfDocumentDefaults, ref new Platform::String());
	doc = hoedown_document_new(myRenderer, (hoedown_extensions)0, 16);

	static std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> conv;
	std::string mdstr = conv.to_bytes(md->Data());
	auto buf = hoedown_buffer_new(64); //we don't actually need this
	hoedown_document_render(doc, buf, (const uint8_t*)mdstr.data(), mdstr.size());
	OutputDoc->Selection->SetRange(0, OutputDoc->Selection->StoryLength);
	hoedown_buffer_free(buf);
}

MarkdownToRichTextHandler::FormattedDocumentRenderer::~FormattedDocumentRenderer()
{
	hoedown_document_free(doc);
	delete myRenderer;
}

std::wstring MarkdownToRichTextHandler::FormattedDocumentRenderer::getStringfromHoeBuf(const hoedown_buffer * content)
{
	static std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>, wchar_t> conv;
	//gotta add some null termination because hoes don't tell you when to stop
	char* z = (char*)malloc(content->size);
	z = strncpy(z, (const char*)content->data, content->size);
	z[content->size] = '\0';
	std::wstring converted = conv.from_bytes(z);

	//Reddit specific: Reddit doesn't do line breaks unless there is a new paragraph. So replace the linebreak that appears between at the beggining or end of string with a space, as Reddit does
	if (converted[0] == L'\n')
		converted[0] = L' ';
	if (converted.back() == L'\n')
		converted.pop_back();
	return converted;
}

void MarkdownToRichTextHandler::FormattedDocumentRenderer::_paragraph(hoedown_buffer * ob, const hoedown_buffer * content, const hoedown_renderer_data * data)
{
	_linebreak(ob, data);
}

void MarkdownToRichTextHandler::FormattedDocumentRenderer::_header(hoedown_buffer *ob, const hoedown_buffer *content, int level, const hoedown_renderer_data *data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	//todo, figure out how to handle stylized (underlined, bold, or italicized) headers
	float Fontsize = 0;
	int weight = 0;

	switch (level)
	{
	case 1:
		Fontsize = 20.5f, weight = 600;
		break;
	case 2:
		Fontsize = 20.5f, weight = 500;
		break;
	case 3:
		Fontsize = 18.3f, weight = 600;
		break;
	case 4:
		Fontsize = 20.5f, weight = 600;
		break;
	case 5:
		_bold(nullptr, content, data);
		return;
		break;
	case 6:
		_underline(nullptr, content, data); //Reddit Specific
		return;
		break;
	}
	range->CharacterFormat->Size = Fontsize;
	range->CharacterFormat->Weight = weight;
}

int MarkdownToRichTextHandler::FormattedDocumentRenderer::_underline(hoedown_buffer * ob, const hoedown_buffer * content, const hoedown_renderer_data * data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	range->CharacterFormat->Underline = Windows::UI::Text::UnderlineType::Single;
	return 1;
}

int MarkdownToRichTextHandler::FormattedDocumentRenderer::_italic(hoedown_buffer * ob, const hoedown_buffer * content, const hoedown_renderer_data * data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	range->CharacterFormat->Italic = Windows::UI::Text::FormatEffect::On;
	return 1;
}

int MarkdownToRichTextHandler::FormattedDocumentRenderer::_bold(hoedown_buffer * ob, const hoedown_buffer * content, const hoedown_renderer_data * data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	range->CharacterFormat->Bold = Windows::UI::Text::FormatEffect::On;
	return 1;
}

int MarkdownToRichTextHandler::FormattedDocumentRenderer::_linebreak(hoedown_buffer * ob, const hoedown_renderer_data * data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	range->EndOf(Windows::UI::Text::TextRangeUnit::Story, false);
	range->SetText(Windows::UI::Text::TextSetOptions::ApplyRtfDocumentDefaults, L"\n");
	return 1;
}

int MarkdownToRichTextHandler::FormattedDocumentRenderer::_italicANDbold(hoedown_buffer * ob, const hoedown_buffer * content, const hoedown_renderer_data * data)
{
	auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
	range->CharacterFormat->Bold = Windows::UI::Text::FormatEffect::On;
	range->CharacterFormat->Italic = Windows::UI::Text::FormatEffect::On;
	return 1;
}

void MarkdownToRichTextHandler::FormattedDocumentRenderer::_normalText(hoedown_buffer * ob, const hoedown_buffer * text, const hoedown_renderer_data * data)
{
	if (text->size != 0)
	{
		auto range = static_cast<FormattedDocumentRenderer*>(data->opaque)->range;
		range->EndOf(Windows::UI::Text::TextRangeUnit::Story, false);
		auto str = getStringfromHoeBuf(text);
		range->SetText(Windows::UI::Text::TextSetOptions::ApplyRtfDocumentDefaults, Platform::StringReference(getStringfromHoeBuf(text).data()));
	}
}
