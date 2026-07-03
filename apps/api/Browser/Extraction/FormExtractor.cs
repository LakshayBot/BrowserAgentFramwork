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
        Array.from(document.querySelectorAll('button, input[type=submit], input[type=button], a[role=button]'))
            .map(el => ({
                text: el.textContent?.trim() || el.value || '',
                type: el.type || 'button',
                visible: el.offsetParent !== null,
                enabled: !el.disabled
            }))
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

    private class ButtonData
    {
        public string? Text { get; set; }
        public string? Type { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
    }
}
