import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '@/lib/api';
import { Play, FileText, Key } from 'lucide-react';

interface Summary {
  id: string; status: string; currentUrl?: string; createdAt: string;
}

interface ProviderSummary {
  id: string; providerType: string; isDefault: boolean;
}

interface DocSummary {
  id: string; documentType: string; displayName: string;
}

export default function DashboardPage() {
  const [workflows, setWorkflows] = useState<Summary[]>([]);
  const [providers, setProviders] = useState<ProviderSummary[]>([]);
  const [docs, setDocs] = useState<DocSummary[]>([]);

  useEffect(() => {
    api.get<Summary[]>('/workflows?page=1&pageSize=5').then(setWorkflows).catch(() => {});
    api.get<ProviderSummary[]>('/providers').then(setProviders).catch(() => {});
    api.get<DocSummary[]>('/documents').then(setDocs).catch(() => {});
  }, []);

  const stats = [
    { icon: Play, label: 'Workflows', value: workflows.length, color: 'text-blue-600', to: '/workflows' },
    { icon: Key, label: 'AI Providers', value: providers.length, color: 'text-purple-600', to: '/providers' },
    { icon: FileText, label: 'Documents', value: docs.length, color: 'text-green-600', to: '/documents' },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Dashboard</h1>

      <div className="mb-8 grid gap-4 sm:grid-cols-3">
        {stats.map((s) => {
          const Icon = s.icon;
          return (
            <Link key={s.label} to={s.to} className="rounded-lg border bg-white p-5 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex items-center gap-3">
                <Icon className={`h-8 w-8 ${s.color}`} />
                <div>
                  <p className="text-2xl font-bold">{s.value}</p>
                  <p className="text-sm text-muted-foreground">{s.label}</p>
                </div>
              </div>
            </Link>
          );
        })}
      </div>

      <div className="grid gap-6 sm:grid-cols-2">
        <div className="rounded-lg border bg-white p-5 shadow-sm">
          <h2 className="mb-3 font-semibold">Recent Workflows</h2>
          {workflows.length === 0 && <p className="text-sm text-muted-foreground">No workflows yet.</p>}
          {workflows.map((w) => (
            <div key={w.id} className="flex items-center justify-between py-2 text-sm">
              <span className="truncate">{w.currentUrl ?? 'No URL'}</span>
              <span className="rounded-full bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-700">{w.status}</span>
            </div>
          ))}
        </div>

        <div>
          <div className="rounded-lg border bg-white p-5 shadow-sm">
            <h2 className="mb-3 font-semibold">Quick Actions</h2>
            <div className="space-y-2">
              <Link to="/workflows" className="flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:opacity-90">
                <Play className="h-4 w-4" /> New Workflow
              </Link>
              <Link to="/providers" className="flex items-center gap-2 rounded-md border px-4 py-2 text-sm font-medium hover:bg-muted">
                <Key className="h-4 w-4" /> Configure AI Provider
              </Link>
              <Link to="/documents" className="flex items-center gap-2 rounded-md border px-4 py-2 text-sm font-medium hover:bg-muted">
                <FileText className="h-4 w-4" /> Upload Resume
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
