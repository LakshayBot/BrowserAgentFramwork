using BrowserAgent.Api.Browser.BrowserManager;
using BrowserAgent.Api.Browser.Interfaces;
using Microsoft.Playwright;
using System.Text.Json;

namespace BrowserAgent.Api.Browser.Extraction;

public class FormExtractor : IFormExtractor
{
    private readonly PlaywrightSessionManager _browserManager;
    private readonly ILogger<FormExtractor> _logger;

    public FormExtractor(PlaywrightSessionManager browserManager, ILogger<FormExtractor> logger)
    {
        _browserManager = browserManager;
        _logger = logger;
    }

    public async Task<ExtractedPage> ExtractPageAsync(string sessionId, CancellationToken ct = default)
    {
        var session = _browserManager.GetSession(sessionId);
        if (session is null) return new ExtractedPage();
        ct.ThrowIfCancellationRequested();

        try
        {
            var page = session.Page;

            var headingsResult = await page.EvaluateAsync<JsonElement>(GetHeadingsScript());
            var linksResult = await page.EvaluateAsync<JsonElement>(GetLinksScript());
            var buttonsResult = await page.EvaluateAsync<JsonElement>(GetButtonsScript());
            var formsResult = await page.EvaluateAsync<JsonElement>(GetFormsScript());

            var headings = headingsResult.EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .ToList();

            var links = linksResult.EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .ToList();

            var buttons = JsonSerializer.Deserialize<List<ButtonData>>(buttonsResult.GetRawText()) ?? new();
            var forms = ParseForms(formsResult);

            if (forms.Count == 0)
            {
                var formlessFields = await page.EvaluateAsync<JsonElement>(GetFormlessFieldsScript());
                forms = ParseFormlessFields(formlessFields);
            }

            if (forms.Count == 0)
            {
                forms = await ExtractFromFrames(sessionId, page, ct);
            }

            _logger.LogInformation("[{SessionId}] Page extracted: {Forms} forms, {Buttons} buttons",
                sessionId, forms.Count, buttons.Count);

            return new ExtractedPage
            {
                Headings = headings,
                Links = links,
                Buttons = buttons.Select(b => new ExtractedButton
                {
                    Text = b.Text,
                    Type = b.Type,
                    Visible = b.Visible,
                    Enabled = b.Enabled
                }).ToList(),
                Forms = forms
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{SessionId}] Page extraction failed", sessionId);
            return new ExtractedPage();
        }
    }

    public async Task<ExtractedForm?> ExtractFormAsync(string sessionId, CancellationToken ct = default)
    {
        var page = await ExtractPageAsync(sessionId, ct);
        return page.Forms.FirstOrDefault();
    }

    private List<ExtractedForm> ParseForms(JsonElement formsResult)
    {
        var forms = new List<ExtractedForm>();

        foreach (var formEl in formsResult.EnumerateArray())
        {
            var form = new ExtractedForm
            {
                FormId = GetStringProp(formEl, "formId"),
                FormName = GetStringProp(formEl, "formName")
            };

            if (formEl.TryGetProperty("fields", out var fields))
            {
                foreach (var field in fields.EnumerateArray())
                {
                    form.Fields.Add(new ExtractedField
                    {
                        Id = GetStringProp(field, "id") ?? "",
                        Label = GetStringProp(field, "label"),
                        Placeholder = GetStringProp(field, "placeholder"),
                        FieldType = GetStringProp(field, "type") ?? "text",
                        Required = GetBoolProp(field, "required"),
                        Disabled = GetBoolProp(field, "disabled"),
                        Readonly = GetBoolProp(field, "readonly"),
                        AvailableOptions = GetStringArrayProp(field, "options"),
                        CurrentValue = GetStringProp(field, "currentValue")
                    });
                }
            }

            forms.Add(form);
        }

        return forms;
    }

    private static string? GetStringProp(JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() : null;
    }

    private static bool GetBoolProp(JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var p) && p.GetBoolean();
    }

    private static List<string>? GetStringArrayProp(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        return arr.EnumerateArray()
            .Select(x => x.GetString() ?? "")
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    private static string GetHeadingsScript() => """
        Array.from(document.querySelectorAll('h1, h2, h3, h4, h5, h6'))
            .map(h => h.textContent?.trim())
            .filter(Boolean)
        """;

    private static string GetLinksScript() => """
        Array.from(document.querySelectorAll('a[href]'))
            .map(a => ({ href: a.href, text: a.textContent?.trim() }))
            .filter(l => l.href && l.text && !l.href.startsWith('javascript:'))
            .slice(0, 50)
            .map(l => l.text + ' -> ' + l.href)
        """;

    private static string GetButtonsScript() => """
        (() => {
            const results = [];
            const seen = new Set();
            const add = (el) => {
                const id = el.outerHTML?.substring(0, 200);
                if (!id || seen.has(id)) return;
                seen.add(id);
                const text = (el.textContent || el.value || el.getAttribute('aria-label') || el.title || '').trim();
                const tag = el.tagName.toLowerCase();
                results.push({
                    text: text,
                    type: tag === 'a' ? 'link' : (el.type || tag),
                    visible: el.offsetParent !== null,
                    enabled: !el.disabled,
                    tag: tag,
                    href: el.href || ''
                });
            };

            document.querySelectorAll('button, input[type=submit], input[type=button], [role=button]').forEach(add);
            document.querySelectorAll('a[href]').forEach(el => {
                const text = (el.textContent || '').trim();
                if (text && (
                    /\b(apply|submit|login|sign.?in|register|create.account|upload|next|continue|save|proceed|i.?m.?interested)\b/i.test(text) ||
                    /apply|submit|btn|button|cta/i.test(el.className || '')
                )) {
                    add(el);
                }
            });

            return results;
        })()
        """;

    private static string GetFormsScript() => """
        Array.from(document.querySelectorAll('form')).map(form => ({
            formId: form.id || '',
            formName: form.name || form.getAttribute('aria-label') || '',
            fields: Array.from(form.querySelectorAll('input, select, textarea')).map(input => {
                var label = form.querySelector('label[for="' + input.id + '"]');
                var ariaLabel = input.getAttribute('aria-label');
                var placeholder = input.getAttribute('placeholder');
                var parentLabel = input.closest('label');
                return {
                    id: input.id || input.name || Math.random().toString(36).slice(2),
                    label: (label ? label.textContent : ariaLabel || (parentLabel ? parentLabel.textContent : ''))?.trim() || '',
                    placeholder: placeholder || '',
                    type: input.type || (input.tagName === 'TEXTAREA' ? 'textarea' : input.tagName === 'SELECT' ? 'select' : 'text'),
                    required: input.required || input.getAttribute('aria-required') === 'true',
                    disabled: input.disabled || input.readOnly,
                    readonly: input.readOnly,
                    options: input.tagName === 'SELECT' ? Array.from(input.options).map(o => o.text?.trim()).filter(Boolean) : [],
                    currentValue: input.value || ''
                };
            })
        }))
        """;

    private static string GetFormlessFieldsScript() => """
        (() => {
            const fields = [];
            const inputs = document.querySelectorAll('input, select, textarea');
            const parentForm = (el) => el.closest('form') ?? null;

            inputs.forEach(input => {
                if (parentForm(input)) return;
                const tag = input.tagName;
                const labelEl = input.closest('label');
                let labelText = input.getAttribute('aria-label') || input.placeholder || '';
                if (!labelText && labelEl) labelText = labelEl.textContent?.trim() || '';
                if (!labelText && input.id) {
                    const lbl = document.querySelector('label[for="' + input.id + '"]');
                    if (lbl) labelText = lbl.textContent?.trim() || '';
                }

                fields.push({
                    id: input.id || input.name || 'field_' + Math.random().toString(36).slice(2, 8),
                    label: labelText,
                    placeholder: input.getAttribute('placeholder') || '',
                    type: input.type || (tag === 'TEXTAREA' ? 'textarea' : tag === 'SELECT' ? 'select' : 'text'),
                    required: input.required || input.getAttribute('aria-required') === 'true',
                    disabled: input.disabled || input.readOnly,
                    readonly: input.readOnly,
                    options: tag === 'SELECT' ? Array.from(input.options).map(o => o.text?.trim()).filter(Boolean) : [],
                    currentValue: input.value || ''
                });
            });

            return [{
                formId: '_page_scan',
                formName: 'Page-level form fields (no <form> tags)',
                fields: fields
            }];
        })()
        """;

    private List<ExtractedForm> ParseFormlessFields(JsonElement fieldsResult)
    {
        return ParseForms(fieldsResult);
    }

    private async Task<List<ExtractedForm>> ExtractFromFrames(string sessionId, IPage mainPage, CancellationToken ct)
    {
        var allForms = new List<ExtractedForm>();

        foreach (var frame in mainPage.Frames)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (frame == mainPage.MainFrame) continue;
                var formsResult = await frame.EvaluateAsync<JsonElement>(GetFormsScript());
                var frameForms = ParseForms(formsResult);

                if (frameForms.Count == 0)
                {
                    var formless = await frame.EvaluateAsync<JsonElement>(GetFormlessFieldsScript());
                    frameForms = ParseFormlessFields(formless);
                }

                if (frameForms.Count > 0)
                {
                    _logger.LogInformation("[{SessionId}] Found {Count} forms in iframe: {Url}",
                        sessionId, frameForms.Count, frame.Url);
                }

                allForms.AddRange(frameForms);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[{SessionId}] Frame extraction skipped: {Url}", sessionId, frame.Url);
            }
        }

        return allForms;
    }

    private class ButtonData
    {
        public string? Text { get; set; }
        public string? Type { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
    }
}
