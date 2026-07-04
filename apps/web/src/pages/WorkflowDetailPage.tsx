import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '@/lib/api';
import { ArrowLeft, Play, Pause, RotateCcw, XCircle } from 'lucide-react';
import WorkflowDebugPanel from '@/components/WorkflowDebugPanel';

interface Step {
  stepNumber: number; stepName: string; status: string; errorMessage?: string;
}

interface Workflow {
  id: string; status: string; currentStep?: string; currentUrl?: string;
  createdAt: string; steps: Step[];
}

export default function WorkflowDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [wf, setWf] = useState<Workflow | null>(null);
  const [error, setError] = useState('');

  const load = () => {
    if (!id) return;
    api.get<Workflow>(`/workflows/${id}`).then(setWf).catch(() => setError('Workflow not found'));
  };

  useEffect(() => { load(); }, [id]);

  const action = async (path: string) => {
    if (!id) return;
    try {
      await api.post(`/workflows/${id}${path}`);
      load();
    } catch (err: any) { alert(err.message); }
  };

  if (error) return <p className="py-8 text-center text-red-600">{error}</p>;
  if (!wf) return <p className="py-8 text-center text-muted-foreground">Loading...</p>;

  return (
    <div>
      <Link to="/workflows" className="mb-4 flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="h-4 w-4" /> Back to Workflows
      </Link>

      <div className="mb-6 rounded-lg border bg-white p-6 shadow-sm">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-xl font-bold">Workflow</h1>
            <p className="mt-1 text-sm text-muted-foreground break-all">{wf.currentUrl ?? 'No URL'}</p>
            <p className="text-xs text-muted-foreground">Created {new Date(wf.createdAt).toLocaleString()}</p>
          </div>
          <span className={`rounded-full px-3 py-1 text-sm font-medium ${
            wf.status === 'Running' ? 'bg-green-50 text-green-700' :
            wf.status === 'Paused' ? 'bg-yellow-50 text-yellow-700' :
            wf.status === 'Created' ? 'bg-blue-50 text-blue-700' :
            wf.status === 'Completed' ? 'bg-green-100 text-green-800' :
            'bg-gray-50 text-gray-700'
          }`}>{wf.status}</span>
        </div>

        <div className="mt-4 flex gap-2">
          {wf.status === 'Created' && (
            <button onClick={() => action('/start')} className="flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:opacity-90">
              <Play className="h-4 w-4" /> Start
            </button>
          )}
          {wf.status === 'Running' && (
            <button onClick={() => action('/pause')} className="flex items-center gap-1 rounded-md border px-3 py-1.5 text-sm hover:bg-muted">
              <Pause className="h-4 w-4" /> Pause
            </button>
          )}
          {wf.status === 'Paused' && (
            <button onClick={() => action('/resume')} className="flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-sm text-primary-foreground hover:opacity-90">
              <RotateCcw className="h-4 w-4" /> Resume
            </button>
          )}
          {(wf.status === 'Running' || wf.status === 'Paused') && (
            <button onClick={() => action('/cancel')} className="flex items-center gap-1 rounded-md border px-3 py-1.5 text-sm hover:bg-muted">
              <XCircle className="h-4 w-4" /> Cancel
            </button>
          )}
        </div>
      </div>

      <div className="rounded-lg border bg-white p-6 shadow-sm">
        <h2 className="mb-4 font-semibold">Steps</h2>
        <div className="space-y-2">
          {wf.steps.map((s) => (
            <div key={s.stepNumber} className="flex items-center gap-3 rounded-md border p-3 text-sm">
              <div className={`flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold text-white ${
                s.status === 'Completed' ? 'bg-green-500' :
                s.status === 'Failed' ? 'bg-red-500' :
                s.status === 'InProgress' ? 'bg-blue-500' :
                'bg-gray-300'
              }`}>{s.stepNumber}</div>
              <div className="flex-1">
                <p className="font-medium">{s.stepName}</p>
                {s.errorMessage && <p className="text-xs text-red-600">{s.errorMessage}</p>}
              </div>
              <span className="text-xs text-muted-foreground">{s.status}</span>
            </div>
          ))}
        </div>
      </div>

      <WorkflowDebugPanel
        workflowId={id!}
        isActive={wf.status === 'Running' || wf.status === 'Paused'}
      />
    </div>
  );
}
