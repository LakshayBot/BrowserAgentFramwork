import { useState, useEffect } from 'react';
import { api } from '@/lib/api';
import { Plus, Trash2, Star, ExternalLink } from 'lucide-react';

interface Provider { id: string; providerType: string; modelName: string; baseUrl?: string; temperature: number; maxTokens: number; isDefault: boolean; createdAt: string; }
interface Preset { key: string; displayName: string; baseUrl: string; models: string[]; }

const EMPTY = { providerType: '', modelName: '', apiKey: '', baseUrl: '', temperature: 0.7, maxTokens: 4096, isDefault: false };

export default function ProvidersPage() {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [presets, setPresets] = useState<Preset[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(EMPTY);

  const load = () => {
    api.get<Provider[]>('/providers').then(setProviders).catch(() => {});
  };

  useEffect(() => {
    load();
    api.get<Preset[]>('/providers/presets').then(setPresets).catch(() => {});
  }, []);

  const onProviderChange = (key: string) => {
    const preset = presets.find(p => p.key === key);
    if (preset) {
      setForm({ ...form, providerType: key, baseUrl: preset.baseUrl, modelName: preset.models[0] || '' });
    } else {
      setForm({ ...form, providerType: key, baseUrl: '', modelName: '' });
    }
  };

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/providers', form);
      setForm(EMPTY); setShowForm(false);
      load();
    } catch (err: any) { alert(err.message); }
  };

  const setDefault = async (id: string) => {
    try { await api.post(`/providers/${id}/default`); load(); }
    catch (err: any) { alert(err.message); }
  };

  const remove = async (id: string) => {
    try { await api.delete(`/providers/${id}`); load(); }
    catch (err: any) { alert(err.message); }
  };

  const currentPreset = presets.find(p => p.key === form.providerType);
  const modelOptions = currentPreset?.models || [];

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">AI Providers</h1>
          <p className="text-sm text-muted-foreground mt-1">Configure your AI API keys. Only the key is needed — we fill in the rest.</p>
        </div>
        <button onClick={() => setShowForm(!showForm)}
          className="flex items-center gap-1 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90">
          <Plus className="h-4 w-4" /> {showForm ? 'Cancel' : 'Add Provider'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={create} className="mb-6 rounded-lg border bg-white p-6 shadow-sm space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <label className="text-sm font-medium">Provider</label>
              <select value={form.providerType} onChange={e => onProviderChange(e.target.value)}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                <option value="">-- Select a provider --</option>
                {presets.map(p => (
                  <option key={p.key} value={p.key}>{p.displayName}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-sm font-medium">Model</label>
              {modelOptions.length > 0 ? (
                <select value={form.modelName} onChange={e => setForm({...form, modelName: e.target.value})}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                  {modelOptions.map(m => <option key={m} value={m}>{m}</option>)}
                </select>
              ) : (
                <input value={form.modelName} onChange={e => setForm({...form, modelName: e.target.value})}
                  className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="e.g. deepseek-chat" />
              )}
            </div>
            <div>
              <label className="text-sm font-medium">API Key</label>
              <input value={form.apiKey} onChange={e => setForm({...form, apiKey: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" type="password" placeholder="sk-..." required />
            </div>
            <div>
              <label className="text-sm font-medium">Base URL</label>
              <input value={form.baseUrl} onChange={e => setForm({...form, baseUrl: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm bg-gray-50" placeholder="Auto-filled from provider" />
              {currentPreset && (
                <p className="text-[10px] text-muted-foreground mt-0.5 flex items-center gap-1">
                  <ExternalLink className="h-3 w-3" /> Default: {currentPreset.baseUrl}
                </p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium">Temperature <span className="text-xs text-muted-foreground">(0-2)</span></label>
              <input type="number" step="0.1" min="0" max="2" value={form.temperature}
                onChange={e => setForm({...form, temperature: parseFloat(e.target.value) || 0.7})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="text-sm font-medium">Max Tokens</label>
              <input type="number" value={form.maxTokens}
                onChange={e => setForm({...form, maxTokens: parseInt(e.target.value) || 4096})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isDefault" checked={form.isDefault}
                onChange={e => setForm({...form, isDefault: e.target.checked})}
                className="h-4 w-4 rounded border-gray-300" />
              <label htmlFor="isDefault" className="text-sm font-medium">Set as default</label>
            </div>
          </div>
          <button type="submit"
            disabled={!form.providerType || !form.apiKey}
            className="rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
            Save Provider
          </button>
        </form>
      )}

      <div className="space-y-2">
        {providers.map((p) => {
          const preset = presets.find(pr => pr.key === p.providerType);
          return (
            <div key={p.id} className="flex items-center justify-between rounded-lg border bg-white p-4 shadow-sm">
              <div>
                <p className="font-medium">
                  {preset?.displayName || p.providerType}
                  <span className="ml-2 text-sm text-muted-foreground font-normal">{p.modelName}</span>
                  {p.isDefault && <Star className="ml-1 inline h-3.5 w-3.5 text-yellow-500 fill-yellow-500" />}
                </p>
                <p className="text-xs text-muted-foreground">
                  {p.baseUrl || (preset?.baseUrl || 'Custom URL')} · T={p.temperature} · Tokens={p.maxTokens}
                </p>
              </div>
              <div className="flex gap-2">
                {!p.isDefault && (
                  <button onClick={() => setDefault(p.id)}
                    className="rounded-md border px-3 py-1 text-xs hover:bg-muted">Set Default</button>
                )}
                <button onClick={() => remove(p.id)}
                  className="flex items-center gap-1 rounded-md border px-3 py-1 text-xs text-red-600 hover:bg-red-50">
                  <Trash2 className="h-3 w-3" /> Delete
                </button>
              </div>
            </div>
          );
        })}
        {providers.length === 0 && (
          <p className="py-8 text-center text-sm text-muted-foreground">No providers configured. Add one to enable AI-powered field mapping.</p>
        )}
      </div>
    </div>
  );
}
