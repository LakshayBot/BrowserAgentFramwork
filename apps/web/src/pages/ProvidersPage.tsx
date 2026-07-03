import { useState, useEffect } from 'react';
import { api } from '@/lib/api';

interface Provider {
  id: string; providerType: string; modelName: string;
  baseUrl?: string; temperature: number; maxTokens: number; isDefault: boolean;
}

const defaultForm = {
  providerType: 'DeepSeek', modelName: 'deepseek-chat', apiKey: '',
  baseUrl: '', temperature: 0.7, maxTokens: 4096, isDefault: false,
};

export default function ProvidersPage() {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(defaultForm);

  const load = () => {
    api.get<Provider[]>('/providers').then(setProviders).catch(() => {});
  };

  useEffect(() => { load(); }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/providers', form);
      setForm(defaultForm);
      setShowForm(false);
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

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">AI Providers</h1>
        <button onClick={() => setShowForm(!showForm)}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90">
          {showForm ? 'Cancel' : 'Add Provider'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={create} className="mb-6 rounded-lg border bg-white p-6 shadow-sm space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="text-sm font-medium">Provider</label>
              <select value={form.providerType} onChange={e => setForm({...form, providerType: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
                <option value="DeepSeek">DeepSeek</option>
                <option value="Ollama">Ollama</option>
              </select>
            </div>
            <div>
              <label className="text-sm font-medium">Model</label>
              <input value={form.modelName} onChange={e => setForm({...form, modelName: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="deepseek-chat" />
            </div>
            <div>
              <label className="text-sm font-medium">API Key</label>
              <input value={form.apiKey} onChange={e => setForm({...form, apiKey: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" type="password" placeholder="sk-..." />
            </div>
            <div>
              <label className="text-sm font-medium">Base URL</label>
              <input value={form.baseUrl} onChange={e => setForm({...form, baseUrl: e.target.value})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" placeholder="https://api.deepseek.com" />
            </div>
            <div>
              <label className="text-sm font-medium">Temperature</label>
              <input type="number" step="0.1" min="0" max="2" value={form.temperature}
                onChange={e => setForm({...form, temperature: parseFloat(e.target.value)})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="text-sm font-medium">Max Tokens</label>
              <input type="number" value={form.maxTokens}
                onChange={e => setForm({...form, maxTokens: parseInt(e.target.value)})}
                className="mt-1 w-full rounded-md border px-3 py-2 text-sm" />
            </div>
          </div>
          <button type="submit" className="rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90">
            Save Provider
          </button>
        </form>
      )}

      <div className="space-y-2">
        {providers.map((p) => (
          <div key={p.id} className="flex items-center justify-between rounded-lg border bg-white p-4 shadow-sm">
            <div>
              <p className="font-medium">{p.providerType} · {p.modelName}</p>
              <p className="text-xs text-muted-foreground">
                {p.baseUrl || 'Default URL'} · Temp: {p.temperature} · Max: {p.maxTokens}
                {p.isDefault && <span className="ml-2 text-green-600 font-medium">Default</span>}
              </p>
            </div>
            <div className="flex gap-2">
              {!p.isDefault && (
                <button onClick={() => setDefault(p.id)}
                  className="rounded-md border px-3 py-1 text-xs hover:bg-muted">Set Default</button>
              )}
              <button onClick={() => remove(p.id)}
                className="rounded-md border px-3 py-1 text-xs text-red-600 hover:bg-red-50">Delete</button>
            </div>
          </div>
        ))}
        {providers.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No providers configured.</p>}
      </div>
    </div>
  );
}
