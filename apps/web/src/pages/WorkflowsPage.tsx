import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '@/lib/api';

interface WorkflowSummary {
  id: string; status: string; currentStep?: string; currentUrl?: string; createdAt: string;
}

export default function WorkflowsPage() {
  const [workflows, setWorkflows] = useState<WorkflowSummary[]>([]);
  const [url, setUrl] = useState('');
  const [busy, setBusy] = useState(false);

  const load = () => {
    api.get<WorkflowSummary[]>('/workflows?page=1&pageSize=20').then(setWorkflows).catch(() => {});
  };

  useEffect(() => { load(); }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!url) return;
    setBusy(true);
    try {
      await api.post('/workflows', { url });
      setUrl('');
      load();
    } catch (err: any) {
      alert(err.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Workflows</h1>

      <form onSubmit={create} className="mb-6 flex gap-3">
        <input
          value={url} onChange={e => setUrl(e.target.value)}
          placeholder="Paste a job URL..."
          className="flex-1 rounded-md border px-4 py-2 text-sm"
        />
        <button type="submit" disabled={busy || !url}
          className="rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50">
          {busy ? 'Creating...' : 'Create'}
        </button>
      </form>

      <div className="space-y-2">
        {workflows.map((w) => (
          <Link key={w.id} to={`/workflows/${w.id}`}
            className="flex items-center justify-between rounded-lg border bg-white p-4 shadow-sm hover:shadow-md transition-shadow">
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium">{w.currentUrl ?? 'No URL'}</p>
              <p className="text-xs text-muted-foreground">{w.currentStep ?? '—'} · {new Date(w.createdAt).toLocaleDateString()}</p>
            </div>
            <span className={`ml-3 rounded-full px-2.5 py-0.5 text-xs font-medium ${
              w.status === 'Running' ? 'bg-green-50 text-green-700' :
              w.status === 'Paused' ? 'bg-yellow-50 text-yellow-700' :
              w.status === 'Created' ? 'bg-blue-50 text-blue-700' :
              w.status === 'Failed' ? 'bg-red-50 text-red-700' :
              'bg-gray-50 text-gray-700'
            }`}>{w.status}</span>
          </Link>
        ))}
        {workflows.length === 0 && <p className="py-8 text-center text-sm text-muted-foreground">No workflows yet. Paste a URL above to get started.</p>}
      </div>
    </div>
  );
}
